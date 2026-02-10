import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: false,
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <button mat-icon-button (click)="sidenav.toggle()" aria-label="Toggle menu">
        <mat-icon>menu</mat-icon>
      </button>
      <span class="app-title">PayorClaims</span>

      <span class="spacer"></span>

      @if (auth.actor(); as actor) {
        <mat-chip-set class="role-chips" aria-label="User roles">
          @for (role of actor.roles; track role) {
            <mat-chip highlighted>{{ role }}</mat-chip>
          }
        </mat-chip-set>
        <span class="user-name">{{ actor.displayName }}</span>
      }

      <button mat-icon-button (click)="logout()" aria-label="Logout" matTooltip="Logout">
        <mat-icon>logout</mat-icon>
      </button>
    </mat-toolbar>

    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #sidenav mode="side" opened class="sidenav">
        <mat-nav-list>
          <a mat-list-item routerLink="/"
             routerLinkActive="active-link"
             [routerLinkActiveOptions]="{ exact: true }">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>

          @if (auth.hasAnyRole('Admin', 'Adjuster', 'Provider', 'Member')) {
            <a mat-list-item routerLink="/members"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>people</mat-icon>
              <span matListItemTitle>Members</span>
            </a>
          }

          @if (auth.isAuthenticated()) {
            <a mat-list-item routerLink="/claims"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>description</mat-icon>
              <span matListItemTitle>Claims</span>
            </a>
          }

          @if (auth.hasAnyRole('Provider', 'Admin', 'Adjuster')) {
            <a mat-list-item routerLink="/claims/submit"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>note_add</mat-icon>
              <span matListItemTitle>Submit Claim</span>
            </a>
          }

          @if (auth.hasAnyRole('Admin', 'Adjuster')) {
            <a mat-list-item routerLink="/adjudication"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>gavel</mat-icon>
              <span matListItemTitle>Adjudication</span>
            </a>
          }

          @if (auth.hasAnyRole('Member', 'Admin')) {
            <a mat-list-item routerLink="/exports"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>file_download</mat-icon>
              <span matListItemTitle>Exports</span>
            </a>
          }

          @if (auth.hasAnyRole('Admin')) {
            <a mat-list-item routerLink="/admin/webhooks"
               routerLinkActive="active-link">
              <mat-icon matListItemIcon>webhook</mat-icon>
              <span matListItemTitle>Webhooks</span>
            </a>
          }

          <mat-divider></mat-divider>

          <a mat-list-item routerLink="/docs"
             routerLinkActive="active-link">
            <mat-icon matListItemIcon>explore</mat-icon>
            <span matListItemTitle>GraphQL Explorer</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="main-content">
        <router-outlet></router-outlet>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    .app-toolbar { position: sticky; top: 0; z-index: 1000; }
    .app-title { font-size: 1.25rem; font-weight: 500; margin-left: 8px; }
    .spacer { flex: 1 1 auto; }
    .role-chips { margin-right: 12px; }
    .user-name { margin-right: 8px; font-size: 0.875rem; opacity: 0.9; }
    .sidenav-container { flex: 1; }
    .sidenav { width: 240px; }
    .main-content { padding: 24px; }
    .active-link { background-color: rgba(0, 0, 0, 0.04); font-weight: 500; }
  `],
})
export class ShellComponent {
  constructor(
    public auth: AuthService,
    private router: Router,
  ) {}

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
