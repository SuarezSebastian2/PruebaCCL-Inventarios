import { Injectable } from '@angular/core';
import { InventoryUiObserver, InventoryUiEvent } from './inventory-ui-observer.model';

/** GoF ConcreteObserver: traza en consola (no invasivo) para depuración / demos. */
@Injectable({ providedIn: 'root' })
export class ConsoleInventoryUiObserver implements InventoryUiObserver {
  onInventoryEvent(event: InventoryUiEvent): void {
    console.debug(
      `[Inventory UI #${event.correlationId}] ${event.tipo} · ${event.detail}`
    );
  }
}
