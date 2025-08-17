import { Component, OnInit } from '@angular/core';
import {
  QuizService,
  TestSummary,
  AddQuestionPayload,          // ⬅️ use the payload type from the service
  QuestionTypeStr              // ⬅️ 'objective' | 'subjective'
} from 'src/app/services/quiz.service';

@Component({
  selector: 'app-admin-create-question',
  templateUrl: './admin-create-question.component.html',
  styleUrls: ['./admin-create-question.component.css']
})
export class AdminCreateQuestionComponent implements OnInit {
  tests: TestSummary[] = [];
  testId: number | null = null;

  type: QuestionTypeStr = 'objective';
  text = '';
  modelAnswer = '';
  image?: File | null;
  options: string[] = ['', '', '', ''];
  correctIndex = 0;

  creating = false;
   trackByIdx = (i: number) => i;

  constructor(private quiz: QuizService) {}

  ngOnInit(): void {
    this.quiz.getTests().subscribe({
      next: (res: TestSummary[]) => (this.tests = res),
      error: (err: any) => console.error(err)
    });
  }

  onImg(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const f = (input?.files && input.files[0]) || null;
    this.image = f;
  }

  createTest(): void {
    const title = prompt('New test title?');
    if (!title) return;

    this.quiz.createTest(title).subscribe({
      next: (t: TestSummary) => {
        alert('Test created.');
        // put newest on top
        this.tests = [{ id: t.id, title: t.title, createdAt: t.createdAt, questionCount: 0 }, ...this.tests];
        this.testId = t.id;
      },
      error: (err: any) => {
        console.error(err);
        alert(err?.error?.message || 'Failed to create test.');
      }
    });
  }

  save(): void {
    if (!this.testId) return alert('Pick a test.');
    if (!this.text.trim()) return alert('Enter question text.');

    const base: AddQuestionPayload = {
      type: this.type,
      text: this.text.trim()
    };

    // add type-specific fields
    if (this.type === 'objective') {
      base.options = this.options.map(o => (o ?? '').trim()).filter(o => o.length > 0);
      base.correctIndex = this.correctIndex;
    } else {
      base.modelAnswer = this.modelAnswer?.trim();
    }

    if (this.image) {
      base.image = this.image;
    }

    this.creating = true;
    // ✅ correct API helper name
    this.quiz.addQuestionToTest(this.testId, base).subscribe({
      next: () => {
        alert('Question added.');
        this.creating = false;

        // reset form
        this.text = '';
        this.modelAnswer = '';
        this.image = null;
        this.options = ['', '', '', ''];
        this.correctIndex = 0;
        this.type = 'objective';
      },
      error: (err: any) => {
        this.creating = false;
        console.error(err);
        alert(err?.error?.message || 'Failed to add question.');
      }
    });
  }
}
