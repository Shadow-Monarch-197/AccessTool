// import { Component } from '@angular/core';
// import { UserService } from 'src/app/services/user.service';

// @Component({
//   selector: 'app-admin-dashboard',
//   templateUrl: './admin-dashboard.component.html',
//   styleUrls: ['./admin-dashboard.component.css']
// })
// export class AdminDashboardComponent {
//   alluserlist : any[] = [];

//   constructor(
//     private userService : UserService
//   ){

//   }

  // getAllUsers(){
  //   this.userService.getallusers().subscribe({
  //     next:(res) => {
  //       this.alluserlist = res;
  //     },
  //     error: (err) => {
  //       console.log(err);
  //       alert(err.error?.message || 'An error occurred during registration.');
  //     }
  //   })
  // }
// }


import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
// NEW
import { QuizService } from 'src/app/services/quiz.service';
import { AttemptListItem, TestSummary } from 'src/app/services/quiz.service'; // adjust path if you keep types elsewhere

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  alluserlist: any[] = [];

  // NEW
  tests: TestSummary[] = [];
  attempts: AttemptListItem[] = [];
  selectedTestId: number | null = null;
  loadingAttempts = false;

showAttempts = false;
attemptsLoaded = false;

  constructor(
    private userService: UserService,
    // NEW
    private quiz: QuizService
  ) {}

  ngOnInit(): void {
    // OPTIONAL: load users as before
//     this.getAllUsers();

    // NEW: load tests & recent attempts
    this.loadTests();
    this.loadAttempts();
  }

//   getAllUsers() {
//     this.userService.getallusers().subscribe({
//       next: (res) => this.alluserlist = res || [],
//       error: (err) => {
//         console.log(err);
//         alert(err.error?.message || 'Failed to load users.');
//       }
//     });
//   }

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

  // NEW
  loadTests() {
    this.quiz.getTests().subscribe({
      next: (res) => this.tests = res || [],
      error: (err) => console.error(err)
    });
  }

  toggleAttempts(): void {                    // NEW
    this.showAttempts = !this.showAttempts;
    if (this.showAttempts && !this.attemptsLoaded) {
      this.loadTests();
      this.loadAttempts();
      this.attemptsLoaded = true;
    }
  }
  // NEW
  loadAttempts(testId?: number) {
    this.loadingAttempts = true;
    this.quiz.getAttempts(testId).subscribe({
      next: (res) => {
        this.attempts = res || [];
        this.loadingAttempts = false;
      },
      error: (err) => {
        console.error(err);
        this.loadingAttempts = false;
        alert(err?.error?.message || 'Failed to load attempts.');
      }
    });
  }

  // NEW
  onFilterChange(id: number | null) {
  this.selectedTestId = id;
  this.loadAttempts(this.selectedTestId ?? undefined);
}

  // NEW (handy for display)
  trackByAttempt = (_: number, a: AttemptListItem) => a.id;
}
