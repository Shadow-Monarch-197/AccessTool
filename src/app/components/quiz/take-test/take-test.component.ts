import { Component, OnInit, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { apiConstants } from 'src/app/Helpers/api-constants';
import { QuizService, SubmitAttemptBody, SubmitAnswer } from 'src/app/services/quiz.service';

@Component({
  selector: 'app-take-test',
  templateUrl: './take-test.component.html',
  styleUrls: ['./take-test.component.css']
})
export class TakeTestComponent implements OnInit {
  test: any;

  // Objective answers (radio)
  selectedOptions: Record<number, number | undefined> = {};
  // Subjective answers (free text)
  subjectiveText: Record<number, string> = {};

  email: string = localStorage.getItem('userEmail') || localStorage.getItem('userName') || '';
  result: any;

readonly fileBase = apiConstants.base_host; // NEW

  // helper to build absolute URL for images
  assetUrl(u?: string | null): string {
    if (!u) return '';
    return u.startsWith('http') ? u : `${this.fileBase}${u}`;
  }


  // UI state
  showErrors = false;
  answeredCount = 0;
  unansweredCount = 0;
  submitting = false;

  @ViewChild('scorePanel') scorePanel?: ElementRef<HTMLDivElement>;

  constructor(
    private route: ActivatedRoute,
    private quiz: QuizService,
    private cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.quiz.getTest(id).subscribe(res => this.test = res);
  }

  // Helpers to interpret question type safely
  isSubjective(q: any): boolean {
    const t = (q?.type || 'objective').toString().toLowerCase();
    return t === 'subjective';
  }
  isObjective(q: any): boolean {
    return !this.isSubjective(q);
  }

  // Mark selection for objective questions
  selectOption(qId: number, optId: number) {
    this.selectedOptions[qId] = optId;
  }

  // Is a question answered?
  isAnswered(q: any): boolean {
    if (this.isSubjective(q)) {
      const v = this.subjectiveText[q.id];
      return !!v && v.trim().length > 0;
    }
    // objective
    return !!this.selectedOptions[q.id];
  }

  // Count answered (objective + subjective)
  get selectedCount(): number {
    if (!this.test?.questions) return 0;
    return this.test.questions.reduce((acc: number, q: any) => acc + (this.isAnswered(q) ? 1 : 0), 0);
  }

  get canSubmit(): boolean {
    return !!this.email; // don't block by unanswered; just require an email
  }

  submit() {
    this.showErrors = true;

    const totalQ = this.test?.questions?.length || 0;
    this.answeredCount = this.selectedCount;
    this.unansweredCount = totalQ - this.answeredCount;

    // Build answers for ALL questions
    const answers: SubmitAnswer[] = (this.test?.questions || []).map((q: any) => {
      if (this.isSubjective(q)) {
        return {
          questionId: q.id,
          subjectiveText: this.subjectiveText[q.id] ?? null,
          selectedOptionId: null
        };
      }
      // objective
      return {
        questionId: q.id,
        selectedOptionId: this.selectedOptions[q.id] ?? null,
        subjectiveText: null
      };
    });

    const body: SubmitAttemptBody = {
      testId: this.test.id,
      userEmail: this.email || 'anonymous@local',
      answers
    };

    this.submitting = true;
    this.quiz.submitAttempt(body).subscribe({
      next: (res) => {
        this.result = res;
        this.submitting = false;
        this.cd.detectChanges();
        this.scorePanel?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
        this.scorePanel?.nativeElement.focus({ preventScroll: true });
      },
      error: (err) => {
        this.submitting = false;
        alert(err.error?.message || 'Submit failed');
      }
    });
  }
}
