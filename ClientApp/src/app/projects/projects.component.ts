import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationExtras } from '@angular/router';

import { Project } from '../project';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { AuthService } from '../auth.service';
import { Observable } from 'rxjs';
import { Extra } from '../common/extra';
import { Format } from '../common/format';

@Component({
  selector: 'app-projects',
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})
export class ProjectsComponent implements OnInit {

  public projects: Project[];

  public visibleProjects: Project[];
  public search: string = "";
  public loggedInRole$: Observable<string>;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private authService: AuthService) {
    this.projects = [];
    this.visibleProjects = [];

    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  async ngOnInit() {

    await this.http.get<Project[]>(this.baseUrl + 'api/Projects', Token.getHeader()).toPromise().then(result => {
      this.projects = result;
    }).catch(error => HandleError.handleError(error, this.router, this.authService));

    this.http.get<Project[]>(this.baseUrl + 'api/Projects/Closed', Token.getHeader()).subscribe(result => {
      this.projects = this.projects.concat(result);
      this.projects.sort((a, b) => (a.isComplete ? 1 : 0) - (b.isComplete ? 1 : 0) + a.startDate.localeCompare(b.startDate) * -0.5);
      this.visibleProjects = this.projects;
    }, error => HandleError.handleError(error, this.router, this.authService));

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

  format(date: string) {
    return Format.formatDate(date);
  }
}

