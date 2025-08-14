import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { QuizService } from 'src/app/services/quiz.service';

@Component({
  selector: 'app-user-tests',
  templateUrl: './user-tests.component.html',
  styleUrls: ['./user-tests.component.css']
})
export class UserTestsComponent implements OnInit {
  tests: any[] = [];
  constructor(private quiz: QuizService, private router: Router) {}
  ngOnInit(): void {
    this.quiz.listTests().subscribe(res => this.tests = res);
  }
  start(id: number) {
    this.router.navigate(['/take-test', id]);
  }
}