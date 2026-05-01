import { Injectable } from '@angular/core';
import { environment } from '../../environment';

/**
 * GoF Factory Method: centraliza construcción de URLs del API sin repetir concatenaciones.
 */
@Injectable({ providedIn: 'root' })
export class ApiEndpointFactory {
  private readonly base = environment.apiBaseUrl;

  /** Origen del API (para interceptores / comparación de URL). */
  origin(): string {
    return this.base;
  }

  login(): string {
    return `${this.base}/auth/login`;
  }

  inventario(): string {
    return `${this.base}/productos/inventario`;
  }

  movimiento(): string {
    return `${this.base}/productos/movimiento`;
  }

  /** Recurso REST de producto (GET/PUT/DELETE por id). */
  productoPorId(id: number): string {
    return `${this.base}/productos/${id}`;
  }

  /** Alta de producto (POST /productos). */
  productosColeccion(): string {
    return `${this.base}/productos`;
  }
}
