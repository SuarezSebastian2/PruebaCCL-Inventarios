import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import Swal from 'sweetalert2';
import { InventoryUiFacade } from '../../core/patterns/facade/inventory-ui.facade';
import { cclSwalTheme } from '../../core/ccl-swal';
import { readApiErrorMessage } from '../../core/read-api-message';

@Component({
  selector: 'app-producto-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './producto-form.component.html',
  styleUrl: './producto-form.component.css'
})
export class ProductoFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly inventory = inject(InventoryUiFacade);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  productoId: number | null = null;
  /** Stock actual solo informativo en edición (no editable aquí). */
  stockActual: number | null = null;
  loadingProducto = false;
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    nombre: [
      '',
      [Validators.required, Validators.minLength(1), Validators.maxLength(200)]
    ],
    cantidad: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    const raw = this.route.snapshot.paramMap.get('id');
    if (raw == null || raw === '') {
      this.productoId = null;
      this.stockActual = null;
      this.form.controls.cantidad.setValidators([Validators.required, Validators.min(0)]);
      this.form.reset({ nombre: '', cantidad: 0 });
      this.form.controls.cantidad.updateValueAndValidity({ emitEvent: false });
      return;
    }
    const id = Number(raw);
    if (Number.isNaN(id)) {
      void this.router.navigateByUrl('/inventario');
      return;
    }
    this.productoId = id;
    this.loadingProducto = true;
    this.inventory.obtenerProducto(id).subscribe({
      next: (p) => {
        this.loadingProducto = false;
        this.stockActual = p.cantidad;
        this.form.patchValue({ nombre: p.nombre, cantidad: p.cantidad });
        this.form.controls.cantidad.clearValidators();
        this.form.controls.cantidad.updateValueAndValidity({ emitEvent: false });
      },
      error: () => {
        this.loadingProducto = false;
        void Swal.fire({
          icon: 'error',
          title: 'Producto no encontrado',
          text: 'Revise la lista de inventario o el enlace utilizado.',
          confirmButtonColor: cclSwalTheme.confirmButtonColor
        }).then(() => this.router.navigateByUrl('/inventario'));
      }
    });
  }

  get titulo(): string {
    return this.productoId != null ? 'Editar producto' : 'Nuevo producto';
  }

  enviar(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.submitting = true;

    if (this.productoId != null) {
      this.inventory.actualizarProducto(this.productoId, { nombre: raw.nombre }).subscribe({
        next: (p) => {
          this.submitting = false;
          void Swal.fire({
            icon: 'success',
            title: 'Producto actualizado',
            text: `Se actualizó el nombre a «${p.nombre}». El stock sigue igual; ajústelo en Movimiento.`,
            confirmButtonColor: cclSwalTheme.confirmButtonColor
          }).then(() => this.router.navigateByUrl('/inventario'));
        },
        error: (err) => {
          this.submitting = false;
          void Swal.fire({
            icon: 'error',
            title: 'No se pudo guardar',
            text: readApiErrorMessage(err),
            confirmButtonColor: cclSwalTheme.confirmButtonColor
          });
        }
      });
    } else {
      this.inventory.crearProducto(raw).subscribe({
        next: (p) => {
          this.submitting = false;
          void Swal.fire({
            icon: 'success',
            title: 'Producto creado',
            text: `«${p.nombre}» quedó registrado con cantidad inicial ${p.cantidad}.`,
            confirmButtonColor: cclSwalTheme.confirmButtonColor
          }).then(() => this.router.navigateByUrl('/inventario'));
        },
        error: (err) => {
          this.submitting = false;
          void Swal.fire({
            icon: 'error',
            title: 'No se pudo crear',
            text: readApiErrorMessage(err),
            confirmButtonColor: cclSwalTheme.confirmButtonColor
          });
        }
      });
    }
  }
}
