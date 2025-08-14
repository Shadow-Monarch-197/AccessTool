// import { Component } from '@angular/core';
// import { QuestionService } from 'src/app/services/question.service';

// @Component({
//   selector: 'app-import-questions',
//   templateUrl: './import-questions.component.html',
//   styleUrls: ['./import-questions.component.css']
// })

// export class ImportQuestionsComponent {
//   file?: File;
//   result: any;
//   loading = false;

//   constructor(private qs: QuestionService) {}

//   onFileChange(ev: Event) {
//     const input = ev.target as HTMLInputElement;
//     if (input.files && input.files.length) {
//       this.file = input.files[0];
//     }
//   }

//   upload() {
//     if (!this.file) return;
//     this.loading = true;
//     this.qs.importExcel(this.file).subscribe({
//       next: (res) => { this.result = res; this.loading = false; },
//       error: (err) => { this.result = err?.error ?? err; this.loading = false; }
//     });
//   }
// }