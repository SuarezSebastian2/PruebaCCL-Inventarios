import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from './environment';

export interface ProductoInventario {
  id: number;
  nombre: string;
  cantidad: number;
}

export interface MovimientoRequest {
  productoId: number;
  tipo: string;
  cantidad: number;
}

@Injectable({ providedIn: 'root' })
export class InventarioApiService {
  private readonly base = `${environment.apiBaseUrl}/productos`;

  constructor(private readonly http: HttpClient) {}

  inventario(): Observable<ProductoInventario[]> {
    return this.http.get<ProductoInventario[]>(`${this.base}/inventario`);
  }

  movimiento(body: MovimientoRequest): Observable<ProductoInventario> {
    return this.http.post<ProductoInventario>(`${this.base}/movimiento`, body);
  }
}
