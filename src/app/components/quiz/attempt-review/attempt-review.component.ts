import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { QuizService } from 'src/app/services/quiz.service';
import { apiConstants } from 'src/app/Helpers/api-constants';

@Component({
  selector: 'app-attempt-review',
  templateUrl: './attempt-review.component.html',
  styleUrls: ['./attempt-review.component.css']
})
export class AttemptReviewComponent implements OnInit {
  loading = true;
  saving = false;

  attempt: {
    attemptId: number;
    testId: number;
    testTitle: string;
    userEmail: string;
    score: number;
    total: number;
    attemptedAt: string;
    answers: Array<{
      questionId: number;
      questionText: string;
      type: 'objective' | 'subjective' | string;
      imageUrl?: string | null;
      selectedOptionId?: number | null;
      selectedOptionText?: string | null;
      isCorrect?: boolean | null;
      correctOptionText?: string | null;
      subjectiveText?: string | null;
      modelAnswer?: string | null;
    }>;
  } | null = null;

  newScore = 0;

  constructor(private route: ActivatedRoute, private quiz: QuizService) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    // uses your new endpoint
    this.quiz.getAttemptDetail(id).subscribe({
      next: (res) => { this.attempt = res; this.newScore = res.score; this.loading = false; },
      error: () => { this.loading = false; alert('Failed to load attempt.'); }
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

  isSubjective(a: any) {
  const t = (a?.type ?? '').toString().toLowerCase();
  return t === 'subjective' || a?.type === 1;
}

  saveScore() {
    if (!this.attempt) return;
    if (this.newScore < 0) this.newScore = 0;
    if (this.newScore > this.attempt.total) this.newScore = this.attempt.total;

    this.saving = true;

    // uses PATCH /Tests/attempts/{attemptId}/score with { score }
    this.quiz.updateAttemptScore(this.attempt.attemptId, this.newScore).subscribe({
      next: () => {
        if (this.attempt) this.attempt.score = this.newScore;
        this.saving = false;
        alert('Score updated.');
      },
      error: (err) => {
        this.saving = false;
        alert(err?.error?.message || 'Failed to update score.');
      }
    });
  }
}
