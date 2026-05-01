import { Injectable } from '@angular/core';

/**
 * GoF Singleton (una instancia en la app vía `providedIn: 'root'`):
 * correlación simple de acciones de UI / inventario.
 */
@Injectable({ providedIn: 'root' })
export class CorrelationIdService {
  private seq = 0;

  next(): number {
    this.seq += 1;
    return this.seq;
  }
}
