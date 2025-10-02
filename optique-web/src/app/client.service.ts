import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
const API = '/api/v1'; // ou '/api' selon tes routes Swagger

export interface ClientDto { id: number; nom: string; prenom?: string | null; }

@Injectable({ providedIn: 'root' })
export class ClientService {
  private http = inject(HttpClient);
  list() { return this.http.get<ClientDto[]>(`${API}/clients`); }
}
