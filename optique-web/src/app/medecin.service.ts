import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
const API = '/api/v1'; // ou '/api'

export interface MedecinDto { id: number; nom: string; prenom?: string | null; }

@Injectable({ providedIn: 'root' })
export class MedecinService {
  private http = inject(HttpClient);
  list() { return this.http.get<MedecinDto[]>(`${API}/medecins`); }
}
