import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  template: `
    <div class="login-wrapper">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-icon mat-card-avatar class="login-icon">lock</mat-icon>
          <mat-card-title>PayorClaims Login</mat-card-title>
          <mat-card-subtitle>Healthcare Claims Management</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <p class="instructions">
            Paste your JWT token from the backend to authenticate.
          </p>

          <mat-form-field appearance="outline" class="token-field">
            <mat-label>JWT Token</mat-label>
            <textarea matInput
                      [(ngModel)]="token"
                      rows="5"
                      placeholder="eyJhbGciOiJIUzI1NiIs...">
            </textarea>
            <mat-hint>Obtain a token from the backend API</mat-hint>
          </mat-form-field>

          @if (errorMessage) {
            <p class="error-message">{{ errorMessage }}</p>
          }
        </mat-card-content>

        <mat-card-actions align="end">
          <button mat-raised-button
                  color="primary"
                  (click)="login()"
                  [disabled]="!token.trim()">
            <mat-icon>login</mat-icon>
            Sign In
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-wrapper { display: flex; justify-content: center; align-items: center; min-height: 100vh; background-color: #f5f5f5; }
    .login-card { width: 480px; max-width: 90vw; }
    .login-icon { font-size: 40px; width: 40px; height: 40px; color: #3f51b5; }
    .instructions { margin: 16px 0; color: rgba(0, 0, 0, 0.6); }
    .token-field { width: 100%; }
    .error-message { color: #f44336; font-size: 0.875rem; margin-top: 8px; }
  `],
})
export class LoginComponent {
  token = '';
  errorMessage = '';

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  login(): void {
    const jwt = this.token.trim();
    if (!jwt) return;
    try {
      this.auth.login(jwt);
      if (this.auth.isAuthenticated()) {
        this.errorMessage = '';
        this.router.navigate(['/']);
      } else {
        this.errorMessage = 'Token is invalid or expired. Please try again.';
      }
    } catch {
      this.errorMessage = 'Failed to parse JWT token. Please check the format.';
    }
  }
}
