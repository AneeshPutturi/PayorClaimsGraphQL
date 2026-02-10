export interface JwtPayload {
  sub: string;
  role?: string | string[];
  memberId?: string;
  npi?: string;
  iat?: number;
  exp?: number;
}

export type AppRole = 'Admin' | 'Adjuster' | 'Provider' | 'Member';

export interface ActorContext {
  sub: string;
  roles: AppRole[];
  memberId?: string;
  npi?: string;
  displayName: string;
}
