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

export interface CrearProductoPayload {
  nombre: string;
  cantidad: number;
}

export interface ActualizarProductoPayload {
  nombre: string;
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

  obtenerProducto(id: number): Observable<ProductoInventario> {
    return this.http.get<ProductoInventario>(this.endpoints.productoPorId(id));
  }

  crearProducto(body: CrearProductoPayload): Observable<ProductoInventario> {
    return this.http.post<ProductoInventario>(this.endpoints.productosColeccion(), body);
  }

  actualizarProducto(id: number, body: ActualizarProductoPayload): Observable<ProductoInventario> {
    return this.http.put<ProductoInventario>(this.endpoints.productoPorId(id), body);
  }

  eliminarProducto(id: number): Observable<void> {
    return this.http.delete<void>(this.endpoints.productoPorId(id));
  }
}
