import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

const API = '/api/v1';  // grâce au proxy, PAS d'URL absolue ici

export interface LigneFactureCreateDto {
  designation: string;
  qte: number;
  prixUnitaire: number;
}
export interface FactureCreateDto {
  clientId: number;
  medecinId?: number | null;
  ordonnanceId?: number | null;
  ordonnance?: any | null;
  notes?: string | null;
  lignes: LigneFactureCreateDto[];
}

export interface CreateFactureResponse {
  id: number;
  numero?: string;   // ex. "INV-2025-001" si ton API le renvoie
  pdfUrl?: string;   // optionnel (sinon on génère via /pdf)
}

@Injectable({ providedIn: 'root' })
export class FactureService {
  private http = inject(HttpClient);

  create(dto: FactureCreateDto) {
    // ⬇️ change le type de retour
    return this.http.post<CreateFactureResponse>(`${API}/factures`, dto);
  }

  pdf(id: number) {
    return this.http.get(`${API}/factures/${id}/pdf`, { responseType: 'blob' });
  }
}

