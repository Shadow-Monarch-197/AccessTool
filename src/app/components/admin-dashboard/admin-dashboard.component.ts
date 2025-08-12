import { Component } from '@angular/core';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent {
  alluserlist : any[] = [];

  constructor(
    private userService : UserService
  ){

  }

  getAllUsers(){
    this.userService.getallusers().subscribe({
      next:(res) => {
        this.alluserlist = res;
      },
      error: (err) => {
        console.log(err);
        alert(err.error?.message || 'An error occurred during registration.');
      }
    })
  }
}
