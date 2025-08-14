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
import { Router, CanActivateFn, CanMatchFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? true : router.parseUrl('/login');
};

export const authMatchGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? true : router.parseUrl('/login');
};