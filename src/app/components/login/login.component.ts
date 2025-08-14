// import { Component, OnInit } from '@angular/core';
// import { FormBuilder, FormGroup, Validators } from '@angular/forms';
// import { LoginModel } from 'src/app/models/login-model';
// import { UserService } from 'src/app/services/user.service';
// import { Router } from '@angular/router';

// @Component({
//   selector: 'app-login',
//   templateUrl: './login.component.html',
//   styleUrls: ['./login.component.css']
// })
// export class LoginComponent implements OnInit{
//   loginForm!: FormGroup;
//   registerForm! : FormGroup;
//   loginUserObj!: LoginModel;
//   toggleForm = true;

//   constructor(
//      private formBuilder : FormBuilder,
//      private userService : UserService,
//      private router: Router
    
//   ){
     
//   }
//   ngOnInit(): void {
//     this.loginForm = this.formBuilder.group({
//       email: ['', [Validators.required, Validators.email]],
//       password: ['', [Validators.required]],
//     });

//     this.registerForm = this.formBuilder.group({
//       name: ['', [Validators.required]],
//       email: ['', [Validators.required, Validators.email]],
//       mobileno: [''],
//       password: ['', [Validators.required]]
//     });
//   }
//   onLogin() {
//   if (this.loginForm.valid) {
//     const loginUserObj = {
//       email: this.loginForm.value.email,
//       password: this.loginForm.value.password,
//     };

//     this.userService.loginuser(loginUserObj).subscribe({
//       next: (res) => {
//         localStorage.setItem('userRole', res.role);
//         localStorage.setItem('userName', res.name);
//         localStorage.setItem('token',res.token ?? '');

//         if (res.role === 'admin') {
//           this.router.navigate(['/admin-dashboard']);
//         } else if (res.role === 'basic') {
//           this.router.navigate(['/user-dashboard']);
//         } else {
//           alert('Unknown role. Access denied.');
//         }
//       },
//       error: (err) => {
//         console.log(err);
//         alert(err.error?.message || 'An error occurred during login.');
//       }
//     });
//   } else {
//     Object.keys(this.loginForm.controls).forEach(key => {
//       const control = this.loginForm.get(key);
//       if (control && control.invalid) {
//         control.markAsTouched();
//       }
//     });
//   }
// }


//  onRegister() {
//   if (this.registerForm.valid) {
//     const registerUserObj = {
//       name: this.registerForm.value.name,
//       email: this.registerForm.value.email,
//       mobileno: this.registerForm.value.mobileno,
//       password: this.registerForm.value.password,
//     };

//     this.userService.registeruser(registerUserObj).subscribe({
//       next: (res) => {
//         alert('Registration successful!');
//         this.toggleForm = true; // switch to login form after registration
//         this.registerForm.reset();
//       },
//       error: (err) => {
//         console.log(err);
//         alert(err.error?.message || 'An error occurred during registration.');
//       }
//     });
//   } else {
//     Object.keys(this.registerForm.controls).forEach(key => {
//       this.registerForm.get(key)?.markAsTouched();
//     });
//   }
// }

  
// }

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoginModel } from 'src/app/models/login-model';
import { UserService } from 'src/app/services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  registerForm!: FormGroup;
  loginUserObj!: LoginModel;
  toggleForm = true;

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
    });

    this.registerForm = this.formBuilder.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      mobileno: [''],
      password: ['', [Validators.required]]
    });

    // NEW: Optional – if a logged-in user hits /login, send them away nicely
    // const token = localStorage.getItem('token');
    // const role = (localStorage.getItem('role') || '').toLowerCase();
    // if (token) {
    //   this.router.navigate([role === 'admin' ? '/admin-dashboard' : '/user-dashboard']);
    // }
  }

  onLogin() {
    if (this.loginForm.valid) {
      const loginUserObj = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
      };

      this.userService.loginuser(loginUserObj).subscribe({
        next: (res) => {
          // (OLD)
          localStorage.setItem('userRole', res.role);
          localStorage.setItem('userName', res.name);
          localStorage.setItem('token', res.token ?? '');

          // NEW: Store a plain 'role' key so the functional guards can read it
          localStorage.setItem('role', (res.role ?? '').toLowerCase()); // NEW

          // CHANGED: Simple role-based navigation (admin -> admin-dashboard, everyone else -> user-dashboard)
          if ((res.role ?? '').toLowerCase() === 'admin') {               // CHANGED
            this.router.navigate(['/admin-dashboard']);                   // CHANGED
          } else {                                                        // CHANGED
            this.router.navigate(['/user-dashboard']);                    // CHANGED
          }
        },
        error: (err) => {
          console.log(err);
          alert(err.error?.message || 'An error occurred during login.');
        }
      });
    } else {
      Object.keys(this.loginForm.controls).forEach(key => {
        const control = this.loginForm.get(key);
        if (control && control.invalid) {
          control.markAsTouched();
        }
      });
    }
  }

  onRegister() {
    if (this.registerForm.valid) {
      const registerUserObj = {
        name: this.registerForm.value.name,
        email: this.registerForm.value.email,
        mobileno: this.registerForm.value.mobileno,
        password: this.registerForm.value.password,
      };

      this.userService.registeruser(registerUserObj).subscribe({
        next: (res) => {
          alert('Registration successful!');
          this.toggleForm = true; // switch to login form after registration
          this.registerForm.reset();
        },
        error: (err) => {
          console.log(err);
          alert(err.error?.message || 'An error occurred during registration.');
        }
      });
    } else {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
    }
  }
}