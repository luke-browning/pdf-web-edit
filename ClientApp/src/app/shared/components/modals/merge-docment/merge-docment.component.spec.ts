import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MergeDocmentComponent } from './merge-docment.component';

describe('MergeDocmentComponent', () => {
  let component: MergeDocmentComponent;
  let fixture: ComponentFixture<MergeDocmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MergeDocmentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MergeDocmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
