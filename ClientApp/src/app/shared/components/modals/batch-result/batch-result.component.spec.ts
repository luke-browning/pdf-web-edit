import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BatchResultComponent } from './batch-result.component';

describe('BatchResultComponent', () => {
  let component: BatchResultComponent;
  let fixture: ComponentFixture<BatchResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BatchResultComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BatchResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
