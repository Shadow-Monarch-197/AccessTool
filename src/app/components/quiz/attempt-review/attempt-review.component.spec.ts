import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AttemptReviewComponent } from './attempt-review.component';

describe('AttemptReviewComponent', () => {
  let component: AttemptReviewComponent;
  let fixture: ComponentFixture<AttemptReviewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AttemptReviewComponent]
    });
    fixture = TestBed.createComponent(AttemptReviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
