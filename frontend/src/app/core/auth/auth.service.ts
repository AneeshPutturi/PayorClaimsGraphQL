import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { ActorContext, AppRole, JwtPayload } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenSignal = signal<string | null>(null);

  readonly isAuthenticated = computed(() => !!this.tokenSignal() && !this.isExpired());
  readonly actor = computed<ActorContext | null>(() => {
    const token = this.tokenSignal();
    if (!token) return null;
    try {
      const payload = jwtDecode<JwtPayload>(token);
      const roles = this.extractRoles(payload);
      return {
        sub: payload.sub,
        roles,
        memberId: payload.memberId,
        npi: payload.npi,
        displayName: payload.sub,
      };
    } catch {
      return null;
    }
  });

  get token(): string | null {
    return this.tokenSignal();
  }

  login(jwt: string): void {
    this.tokenSignal.set(jwt);
  }

  logout(): void {
    this.tokenSignal.set(null);
  }

  hasRole(role: AppRole): boolean {
    return this.actor()?.roles.includes(role) ?? false;
  }

  hasAnyRole(...roles: AppRole[]): boolean {
    return roles.some(r => this.hasRole(r));
  }

  private extractRoles(payload: JwtPayload): AppRole[] {
    if (!payload.role) return [];
    const raw = Array.isArray(payload.role) ? payload.role : [payload.role];
    return raw.filter((r): r is AppRole =>
      ['Admin', 'Adjuster', 'Provider', 'Member'].includes(r)
    );
  }

  private isExpired(): boolean {
    const token = this.tokenSignal();
    if (!token) return true;
    try {
      const payload = jwtDecode<JwtPayload>(token);
      if (!payload.exp) return false;
      return Date.now() >= payload.exp * 1000;
    } catch {
      return true;
    }
  }
}
