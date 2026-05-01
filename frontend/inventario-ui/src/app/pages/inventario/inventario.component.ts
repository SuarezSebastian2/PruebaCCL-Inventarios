import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import Swal from 'sweetalert2';
import { ProductoInventario } from '../../core/inventario-api.service';
import { cclSwalTheme } from '../../core/ccl-swal';
import { readApiErrorMessage } from '../../core/read-api-message';
import { InventoryUiFacade } from '../../core/patterns/facade/inventory-ui.facade';

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './inventario.component.html',
  styleUrl: './inventario.component.css'
})
export class InventarioComponent implements OnInit {
  private readonly inventory = inject(InventoryUiFacade);

  productos: ProductoInventario[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.error = '';
    this.loading = true;
    this.inventory.listInventario().subscribe({
      next: (data) => {
        this.productos = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'No se pudo cargar el inventario.';
      }
    });
  }

  async confirmarEliminar(p: ProductoInventario, event?: Event): Promise<void> {
    event?.preventDefault();
    event?.stopPropagation();
    const r = await Swal.fire({
      title: '¿Eliminar este producto?',
      text: `Se quitará «${p.nombre}» del inventario. Esta acción no se puede deshacer.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
      focusCancel: true,
      confirmButtonColor: cclSwalTheme.confirmButtonColor,
      cancelButtonColor: cclSwalTheme.cancelButtonColor
    });
    if (!r.isConfirmed) {
      return;
    }
    this.inventory.eliminarProducto(p.id).subscribe({
      next: () => {
        void Swal.fire({
          icon: 'success',
          title: 'Producto eliminado',
          toast: true,
          position: 'top-end',
          showConfirmButton: false,
          timer: 2800,
          timerProgressBar: true
        });
        this.load();
      },
      error: (err) => {
        void Swal.fire({
          icon: 'error',
          title: 'No se pudo eliminar',
          text: readApiErrorMessage(err),
          confirmButtonColor: cclSwalTheme.confirmButtonColor
        });
      }
    });
  }
}
