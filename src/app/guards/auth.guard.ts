// import { Injectable } from '@angular/core';
// import { CanActivate, Router, UrlTree } from '@angular/router';
// import { AuthService } from '../services/auth.service';

// @Injectable({ providedIn: 'root' })
// export class AdminGuard implements CanActivate {
//   constructor(private auth: AuthService, private router: Router) {}
//   canActivate(): boolean | UrlTree {
//     if (!this.auth.isLoggedIn()) return this.router.parseUrl('/login');
//     return this.auth.getRole() === 'admin' ? true : this.router.parseUrl('/user-dashboard');
//   }
// }

import { inject } from '@angular/core';
import {
  CanActivateFn,
  CanMatchFn,
  Router,
  UrlTree,
} from '@angular/router';
import { AuthService } from '../services/auth.service';

function requireLoggedIn(): boolean | UrlTree {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.parseUrl('/login');
  }
  return true;
}

// For route activation (navigating to a component)
export const authGuard: CanActivateFn = () => requireLoggedIn();

// For route matching (e.g., guarding the route from being selected)
export const authMatchGuard: CanMatchFn = (_route, _segments) => requireLoggedIn();
