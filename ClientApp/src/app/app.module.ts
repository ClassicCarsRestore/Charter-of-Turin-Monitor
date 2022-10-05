import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { MaterialModule } from '../material.module';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ProjectsComponent } from './projects/projects.component';
import { ProjectDetailsComponent } from './project-details/project-details.component';
import { DiagramComponent } from './diagram/diagram.component';
import { ProjectFormComponent } from './project-form/project-form.component';
import { DatePickerComponent } from './date-picker/date-picker.component';
import { ClosedProjectsComponent } from './closed-projects/closed-projects.component';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { ProfileComponent } from './profile/profile.component';
import { AuthService } from './auth.service';
import { EditProjectComponent } from './edit-project/edit-project.component';
import { AdminPageComponent } from './admin-page/admin-page.component';
import { ProjectDetailsUserComponent } from './project-details-user/project-details-user.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TreeNodeComponent } from './tree-node/tree-node.component';
import { PinterestComponent } from './pinterest/pinterest.component';
import { TaskDetailsComponent } from './task-details/task-details.component';
import { UserProjectsComponent } from './user-projects/user-projects.component';
import { PolicyComponent } from './policy/policy.component';
import { DiagramBaseComponent } from './diagram-base/diagram-base.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    ProjectsComponent,
    ProjectDetailsComponent,
    DiagramComponent,
    ProjectFormComponent,
    DatePickerComponent,
    ClosedProjectsComponent,
    LoginComponent,
    HomeComponent,
    ProfileComponent,
    EditProjectComponent,
    AdminPageComponent,
    ProjectDetailsUserComponent,
    TreeNodeComponent,
    PinterestComponent,
    TaskDetailsComponent,
    UserProjectsComponent,
    PolicyComponent,
    DiagramBaseComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'diagram', component: DiagramBaseComponent },
      { path: 'diagram/:definitionId', component: DiagramBaseComponent },
      { path: 'pinterest', component: PinterestComponent },
      { path: 'projects', component: ProjectsComponent, pathMatch: 'full' },
      { path: 'projects/open', component: UserProjectsComponent },
      { path: 'projects/closed', component: ClosedProjectsComponent },
      { path: 'projects/new', component: ProjectFormComponent },
      { path: 'projects/:projectId', component: ProjectDetailsUserComponent },
      { path: 'projects/details/:projectId', component: ProjectDetailsComponent },
      { path: 'projects/edit/:projectId', component: EditProjectComponent },
      { path: 'login', component: LoginComponent },
      { path: 'admin', component: AdminPageComponent },
      { path: 'profile', component: ProfileComponent },
      { path: 'policy', component: PolicyComponent }
    ]),
    BrowserAnimationsModule
  ],
  providers: [AuthService],
  bootstrap: [AppComponent]
})
export class AppModule { }
