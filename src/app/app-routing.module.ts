import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';


const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Login
  { path: 'login', component: LoginComponent },

  // Admin dashboard
  {
    path: 'admin-dashboard',
    component: AdminDashboardComponent,
  
  },

  // User dashboard
  {
    path: 'user-dashboard',
    component: UserDashboardComponent,
   
  },

  // Wildcard route (404)
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
