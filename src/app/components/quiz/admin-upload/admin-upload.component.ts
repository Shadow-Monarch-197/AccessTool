import { Component } from '@angular/core';
import { QuizService } from 'src/app/services/quiz.service';

@Component({
  selector: 'app-admin-upload',
  templateUrl: './admin-upload.component.html',
  styleUrls: ['./admin-upload.component.css']
})
export class AdminUploadComponent {
  file?: File;
  title = '';
  result: any;
  tests: any[] = [];

  constructor(private quiz: QuizService) { this.load(); }

  onFile(ev: any) {
    const f: File = ev.target.files?.[0];
    this.file = f;
  }

  onUpload() {
    if (!this.file) return;
    this.quiz.uploadExcel(this.file, this.title).subscribe({
      next: (res) => { this.result = res; this.load(); },
      error: (err) => alert(err.error?.message || 'Upload failed')
    });
  }

  load() {
    this.quiz.listTests().subscribe({
      next: (res) => this.tests = res,
      error: () => {}
    });
  }


  confirmDelete(t: any) {
  const ok = confirm(`Delete test "${t.title}"? This will remove its questions and attempts.`);
  if (!ok) return;

  this.quiz.deleteTest(t.id).subscribe({
    next: () => {
      // remove locally for snappy UX or just reload
      this.tests = this.tests.filter(x => x.id !== t.id);
      // or: this.load();
    },
    error: (err) => {
      console.error(err);
      alert(err?.error?.message || 'Failed to delete test.');
    }
  });
}
}