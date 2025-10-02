import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormBuilder, Validators, FormGroup, FormArray, FormControl, AbstractControl
} from '@angular/forms';

import { FactureService, CreateFactureResponse } from './facture.service';
import { ClientService, ClientDto } from './client.service';
import { MedecinService, MedecinDto } from './medecin.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
// Angular Material
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule }      from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule }    from '@angular/material/card';
import { MatIconModule }    from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { OrdonnanceService } from './ordonnance.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatAutocompleteModule, MatOptionModule, MatButtonModule,
    MatToolbarModule, MatCardModule, MatIconModule, MatDividerModule, MatTooltipModule,
    MatSnackBarModule, MatDialogModule, MatProgressSpinnerModule
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})


export class AppComponent {
  // --- état global ---
  
  form!: FormGroup;
  lastId: number | null = null;          // id de la dernière facture
  createdMeta: { id: number; numero?: string; pdfUrl?: string } | null = null; // pour le bandeau persistant
  isSubmitting = false;                  // bouton "Valider" en loading
  locked = false;                        // formulaire verrouillé après succès
  submitted = false;                     // tenté de soumettre au moins 1 fois (pour afficher les erreurs si besoin)
  lastDraft: any | null = null;          // copie du DTO pour "Dupliquer"
  ordUploading = false;
  ordUrl: string | null = null;
  // --- autocomplete ---
  clientCtrl = new FormControl<string | ClientDto | null>('');
  medecinCtrl = new FormControl<string | MedecinDto | null>('');
  clients: ClientDto[] = [];
  medecins: MedecinDto[] = [];
  filteredClients$!: Observable<ClientDto[]>;
  filteredMedecins$!: Observable<MedecinDto[]>;

  constructor(
    private fb: FormBuilder,
    private api: FactureService,
    private clientsApi: ClientService,
    private medecinsApi: MedecinService,
    private snack: MatSnackBar,
    private dialog: MatDialog,
    private ordApi: OrdonnanceService

  ) {
    
    this.form = this.fb.group({
  clientId: [null, Validators.required],   // OBLIGATOIRE
  medecinId: [null],                       // optionnel
  notes: [''],
  lignes: this.fb.array([
    this.fb.group({
      designation: ['', Validators.required],
      qte: [1, [Validators.required, Validators.min(1)]],
      prixUnitaire: [0, [Validators.required, Validators.min(0)]]
    })
  ]),
  // ⬇️ ORDONNANCE (entièrement OPTIONNEL)
  ordonnance: this.fb.group({
    dateOrdonnance: [new Date().toISOString().slice(0,10)], // "yyyy-MM-dd"
    // LOIN
    Loin_OD_Sph: [null], Loin_OD_Cyl: [null], Loin_OD_Axe: [null],
    Loin_OG_Sph: [null], Loin_OG_Cyl: [null], Loin_OG_Axe: [null],
    // PRES
    Pres_OD_Sph: [null], Pres_OD_Cyl: [null], Pres_OD_Axe: [null],
    Pres_OG_Sph: [null], Pres_OG_Cyl: [null], Pres_OG_Axe: [null],
    ADD_PRES: [null]
  })
});


    // charger données
    this.clientsApi.list().subscribe({
      next: data => { this.clients = data; this.initFilters(); },
      error: _ => { this.clients = []; this.initFilters(); }
    });
    this.medecinsApi.list().subscribe({
      next: data => { this.medecins = data; this.initFilters(); },
      error: _ => { this.medecins = []; this.initFilters(); }
    });
  }

  

  // ---------- Autocomplete ----------
  private initFilters() {
    this.filteredClients$ = this.clientCtrl.valueChanges.pipe(
      startWith(this.clientCtrl.value ?? ''),
      map(v => {
        const term = typeof v === 'string' ? v.toLowerCase() : (v ? `${v.nom} ${v.prenom ?? ''}`.toLowerCase() : '');
        return this.clients.filter(c => `${c.nom} ${c.prenom ?? ''}`.toLowerCase().includes(term));
      })
    );
    this.filteredMedecins$ = this.medecinCtrl.valueChanges.pipe(
      startWith(this.medecinCtrl.value ?? ''),
      map(v => {
        const term = typeof v === 'string' ? v.toLowerCase() : (v ? `${v.nom} ${v.prenom ?? ''}`.toLowerCase() : '');
        return this.medecins.filter(m => `${m.nom} ${m.prenom ?? ''}`.toLowerCase().includes(term));
      })
    );
  }

  displayClient = (v: ClientDto | string | null): string =>
    typeof v === 'string' ? v : (v ? `${v.nom} ${v.prenom ?? ''}`.trim() : '');
  displayMedecin = (v: MedecinDto | string | null): string =>
    typeof v === 'string' ? v : (v ? `${v.nom} ${v.prenom ?? ''}`.trim() : '');

  onClientSelected(c: ClientDto) { this.form.patchValue({ clientId: c?.id ?? null }); }
  onMedecinSelected(m: MedecinDto) { this.form.patchValue({ medecinId: m?.id ?? null }); }

  // ---------- Lignes ----------
  get ordonnance(): FormGroup { return this.form.get('ordonnance') as FormGroup; }

  get lignes(): FormArray { return this.form.get('lignes') as FormArray; }
  ctrlAt(i: number, name: 'designation'|'qte'|'prixUnitaire'): AbstractControl {
    return (this.lignes.at(i) as FormGroup).get(name)!;
  }
  addLigne() {
    if (this.locked) return;
    this.lignes.push(this.fb.group({
      designation: ['', Validators.required],
      qte: [1, [Validators.required, Validators.min(1)]],
      prixUnitaire: [0, [Validators.required, Validators.min(0)]]
    }));
  }
  removeLigne(i: number) {
    if (this.locked) return;
    if (this.lignes.length > 1) this.lignes.removeAt(i);
  }

  // ---------- Totaux & Validation ----------
  get total(): number {
    return this.lignes.controls.reduce((sum, g: any) => {
      const q = +g.value.qte || 0;
      const pu = +g.value.prixUnitaire || 0;
      return sum + (q * pu);
    }, 0);
  }
  get hasLines(): boolean { return this.lignes.length > 0; }

  // règle: client obligatoire + au moins 1 ligne + total > 0
  get canSubmit(): boolean {
    return !!this.form.get('clientId')?.value && this.hasLines && this.total > 0 && this.form.valid && !this.locked && !this.isSubmitting;
  }

  // ---------- Actions ----------
  onSubmit() {
    this.submitted = true;
    if (!this.canSubmit) {
      // on n’affiche pas de message global au départ: on se contente de désactiver/activer le bouton
      this.snack.open('Complète les champs obligatoires (client, lignes, montant > 0).', 'OK', { duration: 3000 });
      return;
    }

    const hasAnyOrdValue =
  this.ordUrl ||
  Object.values(this.ordonnance.value).some(v => v !== null && v !== '' && v !== undefined);

    // construire DTO et mémoriser pour "Dupliquer"
    const dto = {
  clientId: Number(this.form.value.clientId),
  medecinId: this.form.value.medecinId ? Number(this.form.value.medecinId) : null,
  notes: this.form.value.notes || null,
  lignes: this.lignes.controls.map((g: any) => ({
    designation: g.value.designation!,
    qte: Number(g.value.qte),
    prixUnitaire: Number(g.value.prixUnitaire)
  })),
  ordonnance: hasAnyOrdValue ? {
    clientId: Number(this.form.value.clientId),
    medecinId: this.form.value.medecinId ? Number(this.form.value.medecinId) : null,
    dateOrdonnance: this.ordonnance.value.dateOrdonnance,
    Loin_OD_Sph: this.ordonnance.value.Loin_OD_Sph,
    Loin_OD_Cyl: this.ordonnance.value.Loin_OD_Cyl,
    Loin_OD_Axe: this.ordonnance.value.Loin_OD_Axe,
    Loin_OG_Sph: this.ordonnance.value.Loin_OG_Sph,
    Loin_OG_Cyl: this.ordonnance.value.Loin_OG_Cyl,
    Loin_OG_Axe: this.ordonnance.value.Loin_OG_Axe,
    Pres_OD_Sph: this.ordonnance.value.Pres_OD_Sph,
    Pres_OD_Cyl: this.ordonnance.value.Pres_OD_Cyl,
    Pres_OD_Axe: this.ordonnance.value.Pres_OD_Axe,
    Pres_OG_Sph: this.ordonnance.value.Pres_OG_Sph,
    Pres_OG_Cyl: this.ordonnance.value.Pres_OG_Cyl,
    Pres_OG_Axe: this.ordonnance.value.Pres_OG_Axe,
    ADD_PRES: this.ordonnance.value.ADD_PRES,
    PhotoUrl: this.ordUrl ?? null
  } : null
};
    this.lastDraft = dto;

    // submit avec spinner + anti double-clic
    this.isSubmitting = true;
    this.api.create(dto).subscribe({
      next: (res: CreateFactureResponse) => {
        this.isSubmitting = false;

        this.lastId = res.id;
        this.createdMeta = { id: res.id, numero: res.numero, pdfUrl: res.pdfUrl };

        // verrouiller le formulaire (lecture seule)
        this.lockForm();

        // dialog de succès
        const ref = this.dialog.open(SuccessDialogComponent, {
          data: {
            id: res.id,
            numero: res.numero ?? `#${res.id}`,
            total: this.total
          },
          width: '420px'
        });

        ref.afterClosed().subscribe(r => {
          if (r?.action === 'download') this.downloadPdf();
          else if (r?.action === 'print') this.printPdf();
          else if (r?.action === 'share-wa') this.shareWhatsApp();
          else if (r?.action === 'share-mail') this.shareEmail();
          // "Aller à la facture" → si tu ajoutes un routing plus tard, navigue ici.
        });
      },
      error: err => {
        this.isSubmitting = false;
        this.snack.open('Erreur lors de la création. Réessaie.', 'Réessayer', { duration: 5000 })
          .onAction().subscribe(() => this.onSubmit());
        console.error(err);
      }
    });
  }

  downloadPdf() {
    if (!this.lastId) return;
    this.api.pdf(this.lastId).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = `Facture_${this.createdMeta?.numero ?? this.lastId}.pdf`; a.click();
      URL.revokeObjectURL(url);
    });
  }

  

  printPdf() {
    if (!this.lastId) return;
    this.api.pdf(this.lastId).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const w = window.open(url, '_blank');
      if (w) w.onload = () => w.print();
    });
  }

  shareWhatsApp() {
    const label = `Facture ${this.createdMeta?.numero ?? '#' + this.lastId}`;
    const text = `${label} - montant: ${this.total.toFixed(2)} MAD`;
    const url = `https://wa.me/?text=${encodeURIComponent(text)}`;
    window.open(url, '_blank');
  }

  onOrdFilePick(ev: Event) {
  const input = ev.target as HTMLInputElement;
  const file = input.files && input.files[0];
  if (!file) return;

  const ok = /image\/(png|jpe?g)|application\/pdf/.test(file.type);
  if (!ok) {
    this.snack.open('Formats autorisés: PNG, JPG, PDF.', 'OK', { duration: 2500 });
    return;
  }

  this.ordUploading = true;
this.ordApi.upload(file).subscribe({
  next: (url: string) => {
    this.ordUploading = false;
    this.ordUrl = url;
    this.snack.open('Document importé.', 'OK', { duration: 1500 });
  },
  error: () => {
    this.ordUploading = false;
    this.snack.open('Échec upload document.', 'Fermer', { duration: 3000 });
  }
});
}

  shareEmail() {
    const subject = encodeURIComponent(`Facture ${this.createdMeta?.numero ?? '#' + this.lastId}`);
    const body = encodeURIComponent(`Bonjour,\n\nVeuillez trouver la facture ci-jointe.\nMontant: ${this.total.toFixed(2)} MAD\n\nCordialement`);
    window.location.href = `mailto:?subject=${subject}&body=${body}`;
  }

  // ---------- Verrouillage / Nouvelle / Dupliquer ----------
  private lockForm() {
    this.locked = true;
    this.form.disable({ emitEvent: false });
  }

  newInvoice() {
    // tout remettre à zéro (déverrouillé)
    this.locked = false;
    this.createdMeta = null;
    this.lastId = null;
    this.submitted = false;

    this.form.enable({ emitEvent: false });
    this.form.reset({ clientId: null, medecinId: null, notes: '' });

    while (this.lignes.length > 1) this.lignes.removeAt(this.lignes.length - 1);
    (this.lignes.at(0) as FormGroup).reset({ designation: '', qte: 1, prixUnitaire: 0 });

    this.clientCtrl.setValue('');
    this.medecinCtrl.setValue('');
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  duplicate() {
    if (!this.lastDraft) return;
    this.locked = false;
    this.createdMeta = null;
    this.lastId = null;
    this.submitted = false;

    this.form.enable({ emitEvent: false });
    this.form.reset({
      clientId: this.lastDraft.clientId ?? null,
      medecinId: this.lastDraft.medecinId ?? null,
      notes: this.lastDraft.notes ?? ''
    });

    // recréer les lignes depuis lastDraft
    while (this.lignes.length) this.lignes.removeAt(0);
    for (const l of this.lastDraft.lignes) {
      this.lignes.push(this.fb.group({
        designation: [l.designation, Validators.required],
        qte: [l.qte, [Validators.required, Validators.min(1)]],
        prixUnitaire: [l.prixUnitaire, [Validators.required, Validators.min(0)]]
      }));
    }

    // mettre l’autocomplete en cohérence visuelle (optionnel)
    const c = this.clients.find(x => x.id === this.lastDraft.clientId);
    this.clientCtrl.setValue(c ? `${c.nom} ${c.prenom ?? ''}` : '');
    const m = this.medecins.find(x => x.id === this.lastDraft.medecinId);
    this.medecinCtrl.setValue(m ? `${m.nom} ${m.prenom ?? ''}` : '');

    this.form.markAsPristine();
    this.form.markAsUntouched();
  }
}

/* --------- Dialog de succès multi-actions --------- */
@Component({
  selector: 'success-dialog',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDividerModule],
  template: `
  <div class="dialog">
    <div class="head">
      <mat-icon class="ok">check_circle</mat-icon>
      <h3>Facture {{data?.numero}}</h3>
    </div>
    <p>Créée avec succès (ID: {{data?.id}}).</p>
    <p>Total: <b>{{ (data?.total ?? 0) | number:'1.2-2' }} MAD</b></p>
    <mat-divider></mat-divider>
    <div class="actions">
      <button mat-stroked-button (click)="close()">Fermer</button>
      <button mat-stroked-button (click)="emit('share-mail')"><mat-icon>email</mat-icon>&nbsp;Partager email</button>
      <button mat-stroked-button (click)="emit('share-wa')"><mat-icon>share</mat-icon>&nbsp;WhatsApp</button>
      <button mat-raised-button color="primary" (click)="emit('download')">
        <mat-icon>picture_as_pdf</mat-icon>&nbsp;Télécharger
      </button>
      <button mat-raised-button (click)="emit('print')">
        <mat-icon>print</mat-icon>&nbsp;Imprimer
      </button>
    </div>
  </div>
  `,
  styles: [`
    .dialog { padding: 8px 4px; }
    .head { display:flex; align-items:center; gap:8px; margin-bottom:4px; }
    .ok { color:#2e7d32; }
    .actions { display:flex; flex-wrap: wrap; justify-content:flex-end; gap:10px; margin-top:10px; }
  `]
})
export class SuccessDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { id: number, numero: string, total: number },
    private ref: MatDialogRef<SuccessDialogComponent>
  ){}
  close(){ this.ref.close(); }
  emit(action: 'download'|'print'|'share-wa'|'share-mail'){ this.ref.close({ action }); }
}
