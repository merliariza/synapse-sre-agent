import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'incidents/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/incidents/create/create-incident.component').then(m => m.CreateIncidentComponent)
  },
  {
    path: 'incidents/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/incidents/detail/incident-detail.component').then(m => m.IncidentDetailComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];