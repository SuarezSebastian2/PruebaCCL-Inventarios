import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiEndpointFactory } from './patterns/factory/api-endpoint.factory';

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
  private readonly http = inject(HttpClient);
  private readonly endpoints = inject(ApiEndpointFactory);

  inventario(): Observable<ProductoInventario[]> {
    return this.http.get<ProductoInventario[]>(this.endpoints.inventario());
  }

  movimiento(body: MovimientoRequest): Observable<ProductoInventario> {
    return this.http.post<ProductoInventario>(this.endpoints.movimiento(), body);
  }
}
