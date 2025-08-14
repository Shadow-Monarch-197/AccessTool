// import { Injectable } from '@angular/core';
// import { jwtDecode } from 'jwt-decode';

// interface DecodedToken {
//   exp?: number;
//   role?: string;        // ClaimTypes.Role in your token
//   unique_name?: string; // ClaimTypes.Name (your email)
// }

// @Injectable({
//   providedIn: 'root'
// })
// export class AuthService {
//   get token(): string | null {
//     return localStorage.getItem('token');
//   }

//   isLoggedIn(): boolean {
//     const t = this.token;
//     if (!t) return false;
//     try {
//       const d = jwtDecode<DecodedToken>(t);
//       if (!d?.exp) return true; // if no exp, treat as logged-in (you do set exp)
//       const nowSec = Math.floor(Date.now() / 1000);
//       return d.exp > nowSec;
//     } catch {
//       return false;
//     }
//   }

//   getRole(): string | null {
//     const t = this.token;
//     if (!t) return null;
//     try {
//       const d = jwtDecode<DecodedToken>(t);
//       // Your JWT creates ClaimTypes.Role, which maps to "role" in many libs.
//       return (d as any)['role'] || null;
//     } catch {
//       return null;
//     }
//   }

//   logout(): void {
//     localStorage.removeItem('token');
//     localStorage.removeItem('userRole');
//     localStorage.removeItem('userName');
//   }

// }

import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

interface DecodedToken {
  exp?: number;
  role?: string;
  roles?: string | string[];
  // Some token issuers use the full claim URI:
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  unique_name?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  get token(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    const t = this.token;
    if (!t) return false;
    try {
      const d = jwtDecode<DecodedToken>(t);
      if (!d?.exp) return true; // your API sets exp; if missing, treat as logged in
      const nowSec = Math.floor(Date.now() / 1000);
      return d.exp > nowSec;
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
  return this.isLoggedIn() && this.getRole() === 'admin';
}

  getRole(): string | null {
    const t = this.token;
    if (!t) return null;
    try {
      const d = jwtDecode<DecodedToken>(t);

      // Prefer simple 'role'
      let role: string | string[] | undefined =
        d.role ??
        d.roles ??
        d['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (Array.isArray(role)) return role[0] ?? null;
      return role ?? null;
    } catch {
      return null;
    }
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
  }
}