import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'inventario',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/inventario/inventario.component').then((m) => m.InventarioComponent)
  },
  {
    path: 'movimiento',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/movimiento/movimiento.component').then((m) => m.MovimientoComponent)
  },
  { path: '', pathMatch: 'full', redirectTo: 'inventario' },
  { path: '**', redirectTo: 'inventario' }
];
