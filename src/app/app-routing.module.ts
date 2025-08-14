// import { NgModule } from '@angular/core';
// import { RouterModule, Routes } from '@angular/router';
// import { LoginComponent } from './components/login/login.component';
// import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
// import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';
// // import { ImportQuestionsComponent } from './components/import-questions/import-questions.component';


// const routes: Routes = [
//   { path: '', redirectTo: 'login', pathMatch: 'full' },

//   // Login
//   { path: 'login', component: LoginComponent },

//   // Admin dashboard
//   {
//     path: 'admin-dashboard',
//     component: AdminDashboardComponent,
  
//   },

//   // User dashboard
//   {
//     path: 'user-dashboard',
//     component: UserDashboardComponent,
   
//   },

//   // // User dashboard
//   // {
//   //   path: 'import-questions',
//   //   component: ImportQuestionsComponent,
//   // },

//   // Wildcard route (404)
//   { path: '**', redirectTo: 'login' }
// ];

// @NgModule({
//   imports: [RouterModule.forRoot(routes)],
//   exports: [RouterModule]
// })
// export class AppRoutingModule { }

// import { NgModule } from '@angular/core';
// import { RouterModule, Routes } from '@angular/router';
// import { AdminUploadComponent } from './components/quiz/admin-upload/admin-upload.component';
// import { UserTestsComponent } from './components/quiz/user-tests/user-tests.component';
// import { TakeTestComponent } from './components/quiz/take-test/take-test.component';
// import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
// import { LoginComponent } from './components/login/login.component';
// import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';


// const routes: Routes = [
//   { path: '', redirectTo: 'login', pathMatch: 'full' },

//   { path: 'login', component: LoginComponent },

//   { path: 'admin-dashboard', component: AdminDashboardComponent },
//   { path: 'admin-upload', component: AdminUploadComponent }, // new

//   { path: 'user-dashboard', component: UserDashboardComponent },
//   { path: 'tests', component: UserTestsComponent }, // list for users
//   { path: 'take-test/:id', component: TakeTestComponent },   // attempt page

//   { path: '**', redirectTo: 'login' }
// ];

// @NgModule({
//   imports: [RouterModule.forRoot(routes)],
//   exports: [RouterModule]
// })
// export class AppRoutingModule { }


// import { NgModule } from '@angular/core';
// import { RouterModule, Routes } from '@angular/router';

// import { AdminUploadComponent } from './components/quiz/admin-upload/admin-upload.component';
// import { UserTestsComponent } from './components/quiz/user-tests/user-tests.component';
// import { TakeTestComponent } from './components/quiz/take-test/take-test.component';
// import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
// import { LoginComponent } from './components/login/login.component';
// import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';

// // Functional guards (make sure these files exist exactly as provided)
// import { authGuard } from './guards/auth.guard';
// import { adminGuard } from './guards/admin.guard';

// const routes: Routes = [
//   { path: '', redirectTo: 'login', pathMatch: 'full' },

//   // Public
//   { path: 'login', component: LoginComponent },

//   // Admin-only
//   { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
//   { path: 'admin-upload', component: AdminUploadComponent, canActivate: [adminGuard] },

//   // Authenticated users
//   { path: 'user-dashboard', component: UserDashboardComponent, canActivate: [authGuard] },
//   { path: 'tests', component: UserTestsComponent, canActivate: [authGuard] },
//   { path: 'take-test/:id', component: TakeTestComponent, canActivate: [authGuard] },

//   // Fallback
//   { path: '**', redirectTo: 'login' }
// ];

// @NgModule({
//   imports: [RouterModule.forRoot(routes)],
//   exports: [RouterModule]
// })
// export class AppRoutingModule {}

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminUploadComponent } from './components/quiz/admin-upload/admin-upload.component';
import { UserTestsComponent } from './components/quiz/user-tests/user-tests.component';
import { TakeTestComponent } from './components/quiz/take-test/take-test.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';

import { authGuard, authMatchGuard } from './guards/auth.guard';
import { adminGuard, adminMatchGuard } from './guards/admin.guard';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Public
  { path: 'login', component: LoginComponent },

  // Admin-only
  {
    path: 'admin-dashboard',
    component: AdminDashboardComponent,
    canMatch: [adminMatchGuard],
    canActivate: [adminGuard],
  },
  {
    path: 'admin-upload',
    component: AdminUploadComponent,
    canMatch: [adminMatchGuard],
    canActivate: [adminGuard],
  },

  // Authenticated users
  {
    path: 'user-dashboard',
    component: UserDashboardComponent,
    canMatch: [authMatchGuard],
    canActivate: [authGuard],
  },
  {
    path: 'tests',
    component: UserTestsComponent,
    canMatch: [authMatchGuard],
    canActivate: [authGuard],
  },
  {
    path: 'take-test/:id',
    component: TakeTestComponent,
    canMatch: [authMatchGuard],
    canActivate: [authGuard],
  },

  // Fallback
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}