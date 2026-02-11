import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../../environments/environment';

interface LoginResponse {
  token: string;
  expiresAt: string;
  user: { username: string; displayName: string; roles: string[] };
}

@Component({
  selector: 'app-login',
  standalone: false,
  template: `
    <div class="login-wrapper">
      <mat-card class="login-card">
        <div class="card-top">
          <div class="logo-circle">
            <mat-icon>local_hospital</mat-icon>
          </div>
          <h1 class="app-name">PayorClaims</h1>
          <p class="app-tagline">Healthcare Claims Management</p>
        </div>

        <mat-card-content>
          <form (ngSubmit)="login()" class="login-form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Username</mat-label>
              <input matInput
                     [(ngModel)]="username"
                     name="username"
                     autocomplete="username"
                     required />
              <mat-icon matPrefix>person</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput
                     [type]="hidePassword ? 'password' : 'text'"
                     [(ngModel)]="password"
                     name="password"
                     autocomplete="current-password"
                     required />
              <mat-icon matPrefix>lock</mat-icon>
              <button mat-icon-button matSuffix type="button"
                      (click)="hidePassword = !hidePassword"
                      [attr.aria-label]="hidePassword ? 'Show password' : 'Hide password'">
                <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>

            @if (errorMessage) {
              <div class="error-banner">
                <mat-icon>error_outline</mat-icon>
                <span>{{ errorMessage }}</span>
              </div>
            }

            <button mat-raised-button
                    color="primary"
                    type="submit"
                    class="login-btn"
                    [disabled]="loading || !username.trim() || !password.trim()">
              @if (loading) {
                <mat-spinner diameter="20" class="btn-spinner"></mat-spinner>
              } @else {
                <mat-icon>login</mat-icon>
              }
              Sign In
            </button>
          </form>

          @if (!environment.production) {
            <mat-divider class="divider"></mat-divider>
            <div class="dev-hint">
              <p class="hint-title">Dev Credentials</p>
              <div class="credentials-table">
                <div class="cred-row header">
                  <span>Username</span><span>Password</span><span>Role</span>
                </div>
                <div class="cred-row" (click)="fillCredentials('admin', 'admin123')">
                  <span>admin</span><span>admin123</span><span>Admin</span>
                </div>
                <div class="cred-row" (click)="fillCredentials('adjuster', 'adjuster123')">
                  <span>adjuster</span><span>adjuster123</span><span>Adjuster</span>
                </div>
                <div class="cred-row" (click)="fillCredentials('provider', 'provider123')">
                  <span>provider</span><span>provider123</span><span>Provider</span>
                </div>
                <div class="cred-row" (click)="fillCredentials('member', 'member123')">
                  <span>member</span><span>member123</span><span>Member</span>
                </div>
              </div>
              <p class="hint-footer">Click a row to fill credentials</p>
            </div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-wrapper {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .login-card {
      width: 420px;
      max-width: 95vw;
      border-radius: 16px !important;
      overflow: hidden;
    }

    .card-top {
      text-align: center;
      padding: 32px 24px 16px;
    }

    .logo-circle {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea, #764ba2);
      margin-bottom: 12px;
    }

    .logo-circle mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: white;
    }

    .app-name {
      font-size: 1.5rem;
      font-weight: 600;
      margin: 0;
      color: rgba(0, 0, 0, 0.87);
    }

    .app-tagline {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.54);
      margin: 4px 0 0;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      padding: 0 8px;
    }

    .full-width {
      width: 100%;
    }

    .error-banner {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 14px;
      background: #fdecea;
      color: #b71c1c;
      border-radius: 8px;
      font-size: 0.875rem;
      margin-bottom: 16px;
    }

    .error-banner mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .login-btn {
      width: 100%;
      height: 48px;
      font-size: 1rem;
      font-weight: 500;
      letter-spacing: 0.5px;
      border-radius: 8px;
      margin-bottom: 8px;
    }

    .btn-spinner {
      display: inline-block;
      margin-right: 8px;
    }

    .divider {
      margin: 20px 0 12px;
    }

    .dev-hint {
      padding: 0 8px;
    }

    .hint-title {
      font-size: 0.8rem;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.5);
      margin: 0 0 8px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .credentials-table {
      font-size: 0.8rem;
      border: 1px solid rgba(0, 0, 0, 0.08);
      border-radius: 8px;
      overflow: hidden;
    }

    .cred-row {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      padding: 8px 12px;
      cursor: pointer;
      transition: background 0.15s;
    }

    .cred-row:not(.header):hover {
      background: rgba(103, 126, 234, 0.08);
    }

    .cred-row.header {
      background: rgba(0, 0, 0, 0.04);
      font-weight: 500;
      color: rgba(0, 0, 0, 0.6);
      cursor: default;
    }

    .cred-row + .cred-row {
      border-top: 1px solid rgba(0, 0, 0, 0.06);
    }

    .hint-footer {
      font-size: 0.75rem;
      color: rgba(0, 0, 0, 0.38);
      text-align: center;
      margin: 8px 0 4px;
    }
  `],
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage = '';
  loading = false;
  hidePassword = true;
  environment = environment;

  private baseUrl: string;

  constructor(
    private auth: AuthService,
    private router: Router,
    private http: HttpClient,
  ) {
    this.baseUrl = environment.apiUrl.replace('/graphql', '');
  }

  login(): void {
    if (!this.username.trim() || !this.password.trim()) return;

    this.loading = true;
    this.errorMessage = '';

    this.http.post<LoginResponse>(`${this.baseUrl}/api/auth/login`, {
      username: this.username.trim(),
      password: this.password,
    }).subscribe({
      next: (res) => {
        this.loading = false;
        this.auth.login(res.token);

        if (this.auth.isAuthenticated()) {
          this.router.navigate(['/']);
        } else {
          this.errorMessage = 'Login succeeded but token validation failed.';
        }
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 401) {
          this.errorMessage = 'Invalid username or password.';
        } else if (err.status === 0) {
          this.errorMessage = 'Cannot connect to server. Is the backend running?';
        } else {
          this.errorMessage = err.error?.error ?? 'Login failed. Please try again.';
        }
      },
    });
  }

  fillCredentials(username: string, password: string): void {
    this.username = username;
    this.password = password;
    this.errorMessage = '';
  }
}
