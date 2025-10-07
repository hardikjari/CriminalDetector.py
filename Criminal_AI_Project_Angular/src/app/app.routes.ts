import { Routes } from '@angular/router';
import { BaseComponent } from './views/layout/base/base.component';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: 'auth', loadChildren: () => import('./views/pages/auth/auth.routes')},
  {
    path: '',
    component: BaseComponent,
    children: [
      {
        path: 'dashboard',
        canActivate: [AuthGuard],
        loadChildren: () => import('./views/pages/dashboard/dashboard.routes')
      },
      {
        path: 'criminals',
        canActivate: [AuthGuard],
        loadComponent: () => import('./views/pages/criminals/criminals.component').then(c => c.CriminalsComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'auth/login', pathMatch: 'full' }
];
