import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  InventarioApiService,
  MovimientoRequest,
  ProductoInventario
} from '../../inventario-api.service';
import { CorrelationIdService } from '../singleton/correlation-id.service';
import { ConsoleInventoryUiObserver } from '../observer/console-inventory-ui.observer';
import { InventorySubjectService } from '../observer/inventory-subject.service';
import { MovimientoMessageStrategyFactory } from '../strategy/movimiento-message.strategy';

/**
 * GoF Facade: punto único para pantallas que consumen inventario / movimientos
 * y orquesta API + notificación a observadores + correlación.
 */
@Injectable({ providedIn: 'root' })
export class InventoryUiFacade {
  constructor(
    private readonly api: InventarioApiService,
    private readonly subject: InventorySubjectService,
    private readonly correlation: CorrelationIdService,
    private readonly messages: MovimientoMessageStrategyFactory,
    consoleObserver: ConsoleInventoryUiObserver
  ) {
    this.subject.attach(consoleObserver);
  }

  listInventario(): Observable<ProductoInventario[]> {
    return this.api.inventario();
  }

  registrarMovimiento(body: MovimientoRequest): Observable<ProductoInventario> {
    return this.api.movimiento(body).pipe(
      tap((producto) => {
        const strategy = this.messages.get(body.tipo);
        const detail =
          strategy?.buildMessage(producto, body.cantidad) ??
          `Movimiento (${body.tipo}) · ${producto.nombre} → ${producto.cantidad}.`;

        this.subject.publish({
          tipo: body.tipo,
          producto,
          cantidad: body.cantidad,
          correlationId: this.correlation.next(),
          detail
        });
      })
    );
  }
}
