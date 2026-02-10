import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './core/layout/login.component';
import { ForbiddenComponent } from './core/layout/forbidden.component';
import { ShellComponent } from './core/layout/shell.component';
import { authGuard, roleGuard } from './core/auth/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'forbidden', component: ForbiddenComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'members',
        loadChildren: () =>
          import('./features/member360/member360.module').then(m => m.Member360Module),
      },
      {
        path: 'claims',
        loadChildren: () =>
          import('./features/claims/claims.module').then(m => m.ClaimsModule),
      },
      {
        path: 'adjudication',
        canActivate: [roleGuard('Admin', 'Adjuster')],
        loadChildren: () =>
          import('./features/adjudication/adjudication.module').then(m => m.AdjudicationModule),
      },
      {
        path: 'exports',
        canActivate: [roleGuard('Member', 'Admin')],
        loadChildren: () =>
          import('./features/exports/exports.module').then(m => m.ExportsModule),
      },
      {
        path: 'admin/webhooks',
        canActivate: [roleGuard('Admin')],
        loadChildren: () =>
          import('./features/admin-webhooks/admin-webhooks.module').then(m => m.AdminWebhooksModule),
      },
      {
        path: 'docs',
        loadChildren: () =>
          import('./features/docs/docs.module').then(m => m.DocsModule),
      },
      { path: '', redirectTo: 'members', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'members' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
