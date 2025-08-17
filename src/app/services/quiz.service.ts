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

// NEW: shapes for submit payloads and admin question creation
export interface SubmitAnswer {
  questionId: number;
  selectedOptionId?: number | null;  // objective
  subjectiveText?: string | null;    // subjective
}


export interface SubmitAttemptBody {
  testId: number;
  userEmail: string;
  answers: SubmitAnswer[];
}


export type QuestionTypeStr = 'objective' | 'subjective';

export interface AddQuestionPayload {
  type: QuestionTypeStr;
  text: string;
  // objective
  options?: string[];
  correctIndex?: number; // 0-based
  // subjective
  modelAnswer?: string;
  // optional image for either type
  image?: File;
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
//NEW
   submitAttempt(body: SubmitAttemptBody): Observable<any> {
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

 // ===== NEW admin helpers =====

  /** Create an empty test with a title (admin) */
  createTest(title: string): Observable<TestSummary> {
    return this.http.post<TestSummary>(`${this.base}/Tests`, { title });
  }

  /** Add a question (objective/subjective) to a test (admin) */
  addQuestionToTest(testId: number, payload: AddQuestionPayload): Observable<{ questionId: number }> {
    const fd = new FormData();
    fd.append('type', payload.type);
    fd.append('text', payload.text);

    if (payload.type === 'objective') {
      (payload.options ?? []).forEach(o => fd.append('options', o));
      if (payload.correctIndex !== undefined && payload.correctIndex !== null) {
        fd.append('correctIndex', String(payload.correctIndex));
      }
    } else {
      if (payload.modelAnswer) fd.append('modelAnswer', payload.modelAnswer);
    }

    if (payload.image) {
      fd.append('image', payload.image, payload.image.name);
    }

    return this.http.post<{ questionId: number }>(`${this.base}/Tests/${testId}/questions`, fd);
  }

}