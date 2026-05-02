import { Component, HostListener, inject, OnInit } from '@angular/core';
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

  /** El contenedor de tabla usaba overflow-x:auto, lo que recorta menús absolutos y fuerza scroll vertical. */
  onRowMenuToggle(ev: Event): void {
    const details = ev.target as HTMLDetailsElement;
    if (!details.classList.contains('row-menu')) {
      return;
    }
    const panel = details.querySelector('.row-menu__panel') as HTMLElement | null;
    if (!panel) {
      return;
    }
    if (details.open) {
      document.querySelectorAll('details.row-menu[open]').forEach((el) => {
        if (el !== details) {
          const other = el as HTMLDetailsElement;
          const op = other.querySelector('.row-menu__panel') as HTMLElement | null;
          other.open = false;
          if (op) {
            this.resetRowMenuPanel(op);
          }
        }
      });
      panel.classList.add('row-menu__panel--anchored');
      requestAnimationFrame(() => this.positionRowMenuPanel(details, panel));
    } else {
      this.resetRowMenuPanel(panel);
    }
  }

  @HostListener('window:scroll')
  @HostListener('window:resize')
  closeOpenRowMenus(): void {
    document.querySelectorAll('details.row-menu[open]').forEach((el) => {
      const d = el as HTMLDetailsElement;
      const panel = d.querySelector('.row-menu__panel') as HTMLElement | null;
      d.open = false;
      if (panel) {
        this.resetRowMenuPanel(panel);
      }
    });
  }

  private resetRowMenuPanel(panel: HTMLElement): void {
    panel.classList.remove('row-menu__panel--anchored');
    panel.style.removeProperty('top');
    panel.style.removeProperty('left');
    panel.style.removeProperty('right');
    panel.style.removeProperty('visibility');
  }

  private positionRowMenuPanel(details: HTMLDetailsElement, panel: HTMLElement): void {
    const summary = details.querySelector('summary') as HTMLElement | null;
    if (!summary || !details.open) {
      return;
    }
    const sr = summary.getBoundingClientRect();
    const edge = 8;
    const gap = 6;
    const vw = globalThis.innerWidth;
    const vh = globalThis.innerHeight;

    panel.style.visibility = 'hidden';
    panel.style.left = '0';
    panel.style.top = '0';
    const pr = panel.getBoundingClientRect();
    panel.style.visibility = '';

    let top = sr.bottom + gap;
    let left = sr.right - pr.width;
    if (left < edge) {
      left = edge;
    }
    if (left + pr.width > vw - edge) {
      left = Math.max(edge, vw - pr.width - edge);
    }
    if (top + pr.height > vh - edge) {
      top = Math.max(edge, sr.top - pr.height - gap);
    }

    panel.style.top = `${Math.round(top)}px`;
    panel.style.left = `${Math.round(left)}px`;
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
    if (p.cantidad !== 0) {
      void Swal.fire({
        icon: 'info',
        title: 'No se puede eliminar',
        text: 'El producto debe tener stock en 0. Use Movimiento para registrar salidas hasta agotar existencias.',
        confirmButtonColor: cclSwalTheme.confirmButtonColor
      });
      return;
    }
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
