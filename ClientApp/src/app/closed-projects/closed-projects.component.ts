import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { Project } from '../project';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { AuthService } from '../auth.service';
import { Format } from '../common/format';
import { Extra } from '../common/extra';

@Component({
  selector: 'app-closed-projects',
  templateUrl: './closed-projects.component.html',
  styleUrls: ['./closed-projects.component.css']
})
export class ClosedProjectsComponent implements OnInit {

  public projects: Project[];

  public visibleProjects: Project[];
  public search: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private router: Router, private authService: AuthService) {
    this.projects = [];
    this.visibleProjects = [];
    this.search = "";

    http.get<Project[]>(baseUrl + 'api/Projects/Closed', Token.getHeader()).subscribe(result => {
      this.projects = result;
      this.visibleProjects = this.projects;
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  ngOnInit(): void {
  }

  changeVisibleProjects() {
    let searchWord = this.search.trim().toLowerCase().split(" ");
    this.visibleProjects = this.projects.filter(p => Extra.search(p, searchWord))
  }

  goToProjectDetails(project: Project) {
    let navigationExtras: NavigationExtras = {
      state: {
        project: project
      }
    };
    this.router.navigate(['/projects/details/' + project.id], navigationExtras);
  }

  format(date: string) {
    return Format.formatDate(date);
  }
}
