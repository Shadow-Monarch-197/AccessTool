import { Injectable } from '@angular/core';
import { Router } from '@angular/router';                // NEW
import { jwtDecode } from 'jwt-decode';

interface DecodedToken {
  exp?: number;
  role?: string | string[];
  roles?: string | string[];
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  unique_name?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private router: Router) {}                 // NEW

  get token(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    const t = this.token;
    if (!t) return false;
    try {
      const d = jwtDecode<DecodedToken>(t);
      if (!d?.exp) return true;
      const now = Math.floor(Date.now() / 1000);
      return d.exp > now;
    } catch {
      return false;
    }
  }

  getRole(): string | null {
    const t = this.token;
    if (!t) return null;
    try {
      const d = jwtDecode<DecodedToken>(t);
      let role: string | string[] | undefined =
        d.role ?? d.roles ?? d['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (Array.isArray(role)) role = role[0];
      return typeof role === 'string' ? role.toLowerCase() : null;
    } catch {
      const stored = localStorage.getItem('role');
      return stored ? stored.toLowerCase() : null;
    }
  }

  isAdmin(): boolean {
    return this.isLoggedIn() && this.getRole() === 'admin';
  }

  logout(toLogin = true): void {                        // NEW
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
    localStorage.removeItem('role');
    if (toLogin) this.router.navigate(['/login']);
  }
}
