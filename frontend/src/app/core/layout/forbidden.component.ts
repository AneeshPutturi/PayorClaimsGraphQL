import { Component } from '@angular/core';

@Component({
  selector: 'app-forbidden',
  standalone: false,
  template: `
    <div class="forbidden-wrapper">
      <mat-card class="forbidden-card">
        <mat-card-content>
          <mat-icon class="forbidden-icon" color="warn">block</mat-icon>
          <h1>403</h1>
          <h2>Access Denied</h2>
          <p>You do not have permission to access this resource.</p>
          <a mat-raised-button color="primary" routerLink="/">
            <mat-icon>home</mat-icon>
            Back to Dashboard
          </a>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .forbidden-wrapper { display: flex; justify-content: center; align-items: center; min-height: 80vh; }
    .forbidden-card { text-align: center; padding: 48px; max-width: 480px; }
    .forbidden-icon { font-size: 64px; width: 64px; height: 64px; }
    h1 { font-size: 4rem; margin: 16px 0 0; color: #f44336; font-weight: 700; }
    h2 { font-size: 1.5rem; margin: 0 0 16px; color: rgba(0, 0, 0, 0.7); }
    p { margin-bottom: 24px; color: rgba(0, 0, 0, 0.54); }
  `],
})
export class ForbiddenComponent {}
