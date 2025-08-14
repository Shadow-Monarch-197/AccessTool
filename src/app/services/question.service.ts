// // src/app/services/question.service.ts
// import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
// import { Injectable } from '@angular/core';
// import { Observable } from 'rxjs';
// import { apiConstants } from '../Helpers/api-constants';

// @Injectable({ providedIn: 'root' })
// export class QuestionService {

//   constructor(private http: HttpClient) {}

//   importExcel(file: File): Observable<any> {
//     const form = new FormData();
//     form.append('file', file, file.name);
//     return this.http.post(apiConstants.import_excel_file, form);
//   }


//   getAll(): Observable<any[]> {
//     return this.http.get<any[]>(apiConstants.import_excel_file);
//   }
// }