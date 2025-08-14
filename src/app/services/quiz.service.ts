import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { apiConstants } from '../Helpers/api-constants';
import { Observable } from 'rxjs';


// new
export interface AttemptListItem {
  id: number;
  testId: number;
  testTitle: string;
  userEmail: string;
  score: number;
  total: number;
  percent: number;
  attemptedAt: string; // ISO
}

export interface TestSummary {
  id: number;
  title: string;
  questionCount: number;
  createdAt: string;
}


@Injectable({ providedIn: 'root' })
export class QuizService {

  
  private base = apiConstants.base_api; 
  constructor(private http: HttpClient) {
  
}

  uploadExcel(file: File, title?: string): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    if (title) form.append('title', title);
    return this.http.post(`${this.base}/Tests/upload`, form);
  }

  listTests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/Tests`);
  }

  getTest(id: number): Observable<any> {
    return this.http.get(`${this.base}/Tests/${id}`);
  }

  submitAttempt(body: { testId: number; userEmail: string; answers: { questionId: number; selectedOptionId?: number }[] }): Observable<any> {
    return this.http.post(`${this.base}/Tests/submit`, body);
  }

  deleteTest(id: number) {
    return this.http.delete(`${this.base}/Tests/${id}`);
  }

//new
getAttempts(testId?: number) {
  const qs = typeof testId === 'number' ? `?testId=${testId}` : '';
  return this.http.get<AttemptListItem[]>(`${this.base}/Tests/attempts${qs}`);
}

getTests() {
  return this.http.get<TestSummary[]>(`${this.base}/Tests`);
}

}