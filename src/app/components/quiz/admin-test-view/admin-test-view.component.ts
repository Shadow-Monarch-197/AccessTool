import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { QuizService } from 'src/app/services/quiz.service';
import { apiConstants } from 'src/app/Helpers/api-constants';

@Component({
  selector: 'app-admin-test-view',
  templateUrl: './admin-test-view.component.html',
  styleUrls: ['./admin-test-view.component.css']
})
export class AdminTestViewComponent implements OnInit {
  loading = true;
  saving = false;

  // payload from GET /Tests/{id}/admin
  test: {
    id: number;
    title: string;
    questions: Array<{
      id: number;
      text: string;
      type: 'objective' | 'subjective' | string;
      imageUrl?: string | null;
      modelAnswer?: string | null;
      options: Array<{ id: number; text: string; isCorrect: boolean }>;
    }>;
  } | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private quiz: QuizService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    // CHANGED: uses your new service name & endpoint
    this.quiz.getAdminTest(id).subscribe({
      next: (res) => { this.test = res; this.loading = false; },
      error: () => { this.loading = false; alert('Failed to load test.'); }
    });
  }

  buildUrl(p?: string | null): string {
    if (!p) return '';
    const base = apiConstants.base_api.replace(/\/api\/?$/i, '');
    return p.startsWith('http') ? p : base + p;
  }

  onImgError(ev: Event) {
    (ev.target as HTMLImageElement).style.display = 'none';
  }

  deleteQuestion(qId: number) {
    if (!this.test) return;
    if (!confirm('Delete this question?')) return;

    this.saving = true;

    // CHANGED: now only passes questionId
    this.quiz.deleteQuestion(qId).subscribe({
      next: () => {
        if (this.test) this.test.questions = this.test.questions.filter(q => q.id !== qId);
        this.saving = false;
      },
      error: (err) => {
        this.saving = false;
        alert(err?.error?.message || 'Failed to delete question.');
      }
    });
  }

  back() {
    this.router.navigate(['/admin-dashboard']);
  }
}
