import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import {
  InventarioApiService,
  ProductoInventario
} from '../../core/inventario-api.service';

@Component({
  selector: 'app-movimiento',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './movimiento.component.html',
  styleUrl: './movimiento.component.css'
})
export class MovimientoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(InventarioApiService);

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

  ngOnInit(): void {
    this.loadingList = true;
    this.api.inventario().subscribe({
      next: (data) => {
        this.productos = data;
        this.loadingList = false;
      },
      error: () => {
        this.loadingList = false;
        this.error = 'No se pudieron cargar los productos.';
      }
    });
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
    this.api
      .movimiento({
        productoId,
        tipo: raw.tipo,
        cantidad: raw.cantidad
      })
      .subscribe({
        next: (p) => {
          this.submitting = false;
          this.success = `Movimiento registrado. ${p.nombre}: ${p.cantidad} unidades.`;
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
    this.api.inventario().subscribe({
      next: (data) => (this.productos = data)
    });
  }
}
