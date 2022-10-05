import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectDetailsUserComponent } from './project-details-user.component';

describe('ProjectDetailsUserComponent', () => {
  let component: ProjectDetailsUserComponent;
  let fixture: ComponentFixture<ProjectDetailsUserComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProjectDetailsUserComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProjectDetailsUserComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
