import { ProductoInventario } from '../../inventario-api.service';

/** Evento publicado tras movimiento exitoso (contexto UI). */
export interface InventoryUiEvent {
  tipo: string;
  producto: ProductoInventario;
  cantidad: number;
  correlationId: number;
  /** Texto amigable generado por la estrategia de mensajes. */
  detail: string;
}

/** GoF Observer: reacciona a publicaciones de inventario en UI. */
export interface InventoryUiObserver {
  onInventoryEvent(event: InventoryUiEvent): void;
}
