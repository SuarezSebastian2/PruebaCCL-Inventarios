import { Injectable } from '@angular/core';
import { InventoryUiEvent, InventoryUiObserver } from './inventory-ui-observer.model';

/**
 * GoF Subject: mantiene observadores y les notifica eventos de inventario en la SPA.
 */
@Injectable({ providedIn: 'root' })
export class InventorySubjectService {
  private readonly observers: InventoryUiObserver[] = [];

  attach(observer: InventoryUiObserver): void {
    if (this.observers.includes(observer)) {
      return;
    }
    this.observers.push(observer);
  }

  publish(event: InventoryUiEvent): void {
    for (const observer of this.observers) {
      observer.onInventoryEvent(event);
    }
  }
}
