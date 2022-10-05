import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationExtras } from '@angular/router';

import { Project } from '../project';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { AuthService } from '../auth.service';
import { Observable } from 'rxjs';
import { Extra } from '../common/extra';

@Component({
  selector: 'app-user-projects',
  templateUrl: './user-projects.component.html',
  styleUrls: ['./user-projects.component.css']
})
export class UserProjectsComponent implements OnInit {
  public projects: Project[];

  public visibleProjects: Project[];
  public search: string = "";
  public loggedInRole$: Observable<string>;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private router: Router, private authService: AuthService) {
    this.projects = [];
    this.visibleProjects = [];

    http.get<Project[]>(baseUrl + 'api/Projects', Token.getHeader()).subscribe(result => {
      this.projects = result;
      this.visibleProjects = this.projects;
    }, error => HandleError.handleError(error, this.router, this.authService));

    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  ngOnInit() {
    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  changeVisibleProjects() {
    let searchWord = this.search.trim().toLowerCase().split(" ");
    this.visibleProjects = this.projects.filter(p => Extra.search(p, searchWord))
  }

  goToProjectDetails(project: Project) {
    let details = "";
    if (this.authService.loggedInRole != "owner")
      details = "details/"

    let navigationExtras: NavigationExtras = {
      state: {
        project: project
      }
    };
    this.router.navigate(['/projects/' + details + project.id], navigationExtras);
  }

}
