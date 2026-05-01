import { Injectable } from '@angular/core';
import { ProductoInventario } from '../../inventario-api.service';

/** GoF Strategy: mensaje de éxito según tipo de movimiento. */
export interface MovimientoMessageStrategy {
  readonly tipo: 'entrada' | 'salida';

  buildMessage(producto: ProductoInventario, cantidad: number): string;
}

export class EntradaMovimientoMessageStrategy implements MovimientoMessageStrategy {
  readonly tipo = 'entrada' as const;

  buildMessage(producto: ProductoInventario, cantidad: number): string {
    return `Entrada de ${cantidad} u. · ${producto.nombre} → stock ${producto.cantidad}.`;
  }
}

export class SalidaMovimientoMessageStrategy implements MovimientoMessageStrategy {
  readonly tipo = 'salida' as const;

  buildMessage(producto: ProductoInventario, cantidad: number): string {
    return `Salida de ${cantidad} u. · ${producto.nombre} → stock ${producto.cantidad}.`;
  }
}

/**
 * GoF Factory sobre estrategias de mensaje (sin estado; instancias fijas).
 */
@Injectable({ providedIn: 'root' })
export class MovimientoMessageStrategyFactory {
  private readonly strategies = new Map<string, MovimientoMessageStrategy>();

  constructor() {
    const list: MovimientoMessageStrategy[] = [
      new EntradaMovimientoMessageStrategy(),
      new SalidaMovimientoMessageStrategy()
    ];
    for (const s of list) {
      this.strategies.set(s.tipo, s);
    }
  }

  get(tipo: string): MovimientoMessageStrategy | undefined {
    return this.strategies.get(tipo.trim().toLowerCase());
  }
}
