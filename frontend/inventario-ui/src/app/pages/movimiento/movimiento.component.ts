import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProductoInventario } from '../../core/inventario-api.service';
import { InventoryUiFacade } from '../../core/patterns/facade/inventory-ui.facade';
import { MovimientoMessageStrategyFactory } from '../../core/patterns/strategy/movimiento-message.strategy';

@Component({
  selector: 'app-movimiento',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './movimiento.component.html',
  styleUrl: './movimiento.component.css'
})
export class MovimientoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly inventory = inject(InventoryUiFacade);
  private readonly messageStrategies = inject(MovimientoMessageStrategyFactory);
  private readonly route = inject(ActivatedRoute);

  productos: ProductoInventario[] = [];
  loadingList = false;
  submitting = false;
  success = '';
  error = '';

  readonly form = this.fb.nonNullable.group({
    productoId: this.fb.control<number | null>(null, Validators.required),
    tipo: ['entrada', Validators.required],
    cantidad: [1, [Validators.required, Validators.min(1)]]
  });

  /** Botón deshabilitado hasta formulario válido (p. ej. producto elegido). */
  get movimientoSubmitDisabled(): boolean {
    return this.submitting || this.productos.length === 0 || this.form.invalid;
  }

  get movimientoSubmitTooltip(): string {
    if (this.submitting) {
      return 'Enviando al servidor…';
    }
    if (this.productos.length === 0) {
      return 'No hay productos en inventario; agregue uno desde Inventario antes de mover stock.';
    }
    if (this.form.controls.productoId.invalid) {
      return 'Seleccione un producto de la lista para habilitar el registro del movimiento.';
    }
    if (this.form.controls.cantidad.invalid) {
      return 'Indique una cantidad válida (entero mayor o igual a 1).';
    }
    return 'Registra el movimiento con el producto, tipo y cantidad indicados.';
  }

  ngOnInit(): void {
    this.loadingList = true;
    this.inventory.listInventario().subscribe({
      next: (data) => {
        this.productos = data;
        this.loadingList = false;
        this.applyProductoFromQuery();
      },
      error: () => {
        this.loadingList = false;
        this.error = 'No se pudieron cargar los productos.';
      }
    });

    this.route.queryParamMap.subscribe(() => {
      if (this.productos.length > 0) {
        this.applyProductoFromQuery();
      }
    });
  }

  private applyProductoFromQuery(): void {
    const raw = this.route.snapshot.queryParamMap.get('productoId');
    if (raw == null) {
      return;
    }
    const id = Number(raw);
    if (Number.isNaN(id) || !this.productos.some((p) => p.id === id)) {
      return;
    }
    this.form.patchValue({ productoId: id });
  }

  submit(): void {
    this.success = '';
    this.error = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const productoId = raw.productoId;
    if (productoId == null) {
      return;
    }
    this.submitting = true;
    const body = {
      productoId,
      tipo: raw.tipo,
      cantidad: raw.cantidad
    };
    this.inventory.registrarMovimiento(body).subscribe({
      next: (p) => {
        this.submitting = false;
        const strategy = this.messageStrategies.get(raw.tipo);
        this.success =
          strategy?.buildMessage(p, raw.cantidad) ??
          `Movimiento registrado. ${p.nombre}: ${p.cantidad} unidades.`;
        this.refreshList();
      },
      error: (err) => {
        this.submitting = false;
        const msg = err?.error?.message;
        this.error = typeof msg === 'string' ? msg : 'No se pudo registrar el movimiento.';
      }
    });
  }

  private refreshList(): void {
    this.inventory.listInventario().subscribe({
      next: (data) => (this.productos = data)
    });
  }
}
