import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminTestViewComponent } from './admin-test-view.component';

describe('AdminTestViewComponent', () => {
  let component: AdminTestViewComponent;
  let fixture: ComponentFixture<AdminTestViewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdminTestViewComponent]
    });
    fixture = TestBed.createComponent(AdminTestViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
