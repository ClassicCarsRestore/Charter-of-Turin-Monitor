import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiagramBaseComponent } from './diagram-base.component';

describe('DiagramBaseComponent', () => {
  let component: DiagramBaseComponent;
  let fixture: ComponentFixture<DiagramBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DiagramBaseComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DiagramBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
