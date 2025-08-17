import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminCreateQuestionComponent } from './admin-create-question.component';

describe('AdminCreateQuestionComponent', () => {
  let component: AdminCreateQuestionComponent;
  let fixture: ComponentFixture<AdminCreateQuestionComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminCreateQuestionComponent]
    });
    fixture = TestBed.createComponent(AdminCreateQuestionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
