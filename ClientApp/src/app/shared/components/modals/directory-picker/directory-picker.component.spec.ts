import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DirectoryPickerComponent } from './directory-picker.component';

describe('DirectoryPickerComponent', () => {
  let component: DirectoryPickerComponent;
  let fixture: ComponentFixture<DirectoryPickerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DirectoryPickerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DirectoryPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
