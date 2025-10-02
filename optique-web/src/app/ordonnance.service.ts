import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

const API = '/api/v1'; // adapte si besoin

@Injectable({ providedIn: 'root' })
export class OrdonnanceService {
  private http = inject(HttpClient);

  upload(file: File) {
    const fd = new FormData();
    fd.append('file', file, file.name); // garder le nom pour l’extension
    return this.http.post<string>(`${API}/files/upload-ordonnance`, fd);
  }
}
