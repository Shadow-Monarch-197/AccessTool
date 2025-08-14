// import { Component, OnInit } from '@angular/core';
// import { ActivatedRoute } from '@angular/router';
// import { QuizService } from 'src/app/services/quiz.service';

// @Component({
//   selector: 'app-take-test',
//   templateUrl: './take-test.component.html',
//   styleUrls: ['./take-test.component.css']
// })
// export class TakeTestComponent implements OnInit {
//   test: any;
//   selected: Record<number, number|undefined> = {};
//   email: string = localStorage.getItem('userEmail') || localStorage.getItem('userName') || '';
//   result: any;

//   constructor(private route: ActivatedRoute, private quiz: QuizService) {}

//   ngOnInit(): void {
//     const id = Number(this.route.snapshot.paramMap.get('id'));
//     this.quiz.getTest(id).subscribe(res => this.test = res);
//   }

//   select(qId: number, optId: number) {
//     this.selected[qId] = optId;
//   }


// ///new code
//   get selectedCount(): number {
//     return this.test?.questions?.reduce((acc: number, q: any) => acc + (this.selected?.[q.id] ? 1 : 0), 0) || 0;
//   }

//   get canSubmit(): boolean {
//     return !!this.email && this.selectedCount === (this.test?.questions?.length || 0);
//   }

//   submit() {
//     const answers = Object.keys(this.selected).map(k => ({
//       questionId: Number(k),
//       selectedOptionId: this.selected[Number(k)]
//     }));
//     const body = {
//       testId: this.test.id,
//       userEmail: this.email || 'anonymous@local',
//       answers
//     };
//     this.quiz.submitAttempt(body).subscribe({
//       next: (res) => this.result = res,
//       error: (err) => alert(err.error?.message || 'Submit failed')
//     });
//   }
// }

import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core'; // NEW
import { ActivatedRoute } from '@angular/router';
import { QuizService } from 'src/app/services/quiz.service';

@Component({
  selector: 'app-take-test',
  templateUrl: './take-test.component.html',
  styleUrls: ['./take-test.component.css']
})
export class TakeTestComponent implements OnInit {
  test: any;
  selected: Record<number, number | undefined> = {};
  email: string = localStorage.getItem('userEmail') || localStorage.getItem('userName') || '';
  result: any;

  // NEW: UI state + summary counts
  showErrors = false;          // NEW – toggles “Question not answered” hints
  answeredCount = 0;           // NEW
  unansweredCount = 0;         // NEW
  submitting = false;          // NEW

  @ViewChild('scorePanel') scorePanel?: ElementRef<HTMLDivElement>; // NEW

  constructor(
    private route: ActivatedRoute,
    private quiz: QuizService,
    private cd: ChangeDetectorRef // NEW
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.quiz.getTest(id).subscribe(res => this.test = res);
  }

  select(qId: number, optId: number) {
    this.selected[qId] = optId;
  }

  get selectedCount(): number {
    return this.test?.questions?.reduce((acc: number, q: any) => acc + (this.selected?.[q.id] ? 1 : 0), 0) || 0;
  }

  get canSubmit(): boolean {
    return !!this.email; // CHANGED: don’t require all questions answered
  }

  // NEW: helper to query answered state in template
  isAnswered(qId: number): boolean {
    return !!this.selected[qId];
  }

  submit() {
    this.showErrors = true; // NEW: reveal “Question not answered” messages

    // NEW: compute summary (without blocking submit)
    const totalQ = this.test?.questions?.length || 0;
    this.answeredCount = totalQ ? Object.keys(this.selected).length : 0;
    this.unansweredCount = totalQ - this.answeredCount;

    // CHANGED: include ALL questions; unanswered -> null
    const answers = (this.test?.questions || []).map((q: any) => ({
      questionId: q.id,
      selectedOptionId: this.selected[q.id] ?? null
    }));

    const body = {
      testId: this.test.id,
      userEmail: this.email || 'anonymous@local',
      answers
    };

    this.submitting = true; // NEW
    this.quiz.submitAttempt(body).subscribe({
      next: (res) => {
        this.result = res;
        this.submitting = false;        // NEW
        this.cd.detectChanges();        // NEW: ensure #scorePanel exists
        this.scorePanel?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' }); // NEW
        this.scorePanel?.nativeElement.focus({ preventScroll: true }); // NEW (a11y)
      },
      error: (err) => {
        this.submitting = false; // NEW
        alert(err.error?.message || 'Submit failed');
      }
    });
  }
}