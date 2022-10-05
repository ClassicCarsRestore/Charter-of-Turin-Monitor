import { Component, Inject, OnInit, ViewChild, ViewChildren, ViewEncapsulation } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute, NavigationExtras } from '@angular/router';

import { Project } from '../project';
import { CompletedHistoryTasks, PathTask, Task } from '../task';
import { DiagramComponent } from '../diagram/diagram.component';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { AuthService } from '../auth.service';
import { Observable } from 'rxjs';
import { Format } from '../common/format';
import { PathNode, PathProcess } from '../process';

@Component({
  selector: 'app-project-details',
  templateUrl: './project-details.component.html',
  styleUrls: ['./project-details.component.css']
  ,
  encapsulation: ViewEncapsulation.None
})
export class ProjectDetailsComponent implements OnInit {

  @ViewChild(DiagramComponent, {static: false})
  public diagramComponent!: DiagramComponent;

  public project?: Project;

  public root?: PathNode;
  public selectedNode?: string;

  importError?: Error;

  public loggedInRole$: Observable<string>;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  async ngOnInit(): Promise<void> {
    // get the selected Project info
    if (this.router != null && this.router.getCurrentNavigation()?.extras.state)
      this.project = this.router.getCurrentNavigation()!.extras.state!.project;
    else { // retrieve the project by id from url
      let projectId: string = "";

      // get id from url
      this.activatedRoute.paramMap.subscribe(params => {
        var tmpProjectId: string | null = params.get('projectId');
        if (tmpProjectId != null)
          projectId = tmpProjectId;
      });

      await this.http.get<Project>(this.baseUrl + 'api/Projects/' + projectId + '/DTO', Token.getHeader()).toPromise().then(result => {
        this.project = result;
      }).catch(error => {
        HandleError.handleError(error, this.router, this.authService)
        alert("This project couldn't be loaded.");
        this.router.navigate(['/projects']);
      });

      if (this.project && !this.project.isComplete)
        await this.http.get<PathNode>(this.baseUrl + 'api/Tasks/' + this.project?.caseInstanceId + '/Path', Token.getHeader()).toPromise().then(result => {
          this.root = result;
          let node = this.root;
          while (node.children.length > 0)
            node = node.children[0];
          this.selectedNode = node.self.instanceId;
        }).catch(error => {
          if (error.status == 404) {
            alert("This project couldn't be loaded.");
            this.router.navigate(['/projects']);
          }
          else
            HandleError.handleError(error, this.router, this.authService)
        });
      else if (this.project)
        await this.getRootDiagram();
    }

    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  async getRootDiagram(){
    await this.http.get<PathNode>(this.baseUrl + 'api/Tasks/' + this.project?.caseInstanceId + '/Root', Token.getHeader()).toPromise().then(result => {
        this.root = result;
        this.selectedNode = this.root.self.instanceId;
      }).catch(error => {
        if (error.status == 404) {
          alert("This project couldn't be loaded.");
          this.router.navigate(['/projects']);
        }
        else
          HandleError.handleError(error, this.router, this.authService)
      });
  }

  ngAfterViewInit() {
  }

  async submitTasks() {
    if (await this.diagramComponent.submitTasks(this.project ? this.project?.id : "")) {
      await this.http.get<PathNode>(this.baseUrl + 'api/Tasks/' + this.project?.caseInstanceId + '/Path', Token.getHeader()).toPromise().then(result => {
        this.root = result;
        let node = this.root;
        while (node.children.length > 0)
          node = node.children[node.children.length - 1];
        this.selectedNode = node.self.instanceId;
      }).catch(async error => {
        await this.http.get<Project>(this.baseUrl + 'api/Projects/' + this.project?.id + '/DTO', Token.getHeader()).toPromise().then(result => {
          this.project = result;
          if (!this.project.isComplete)
            throw new Error(); 
          this.getRootDiagram();
        }).catch(error => {
          HandleError.handleError(error, this.router, this.authService)
          alert("This project couldn't be loaded.");
          this.router.navigate(['/projects']);
        });
      });
    }
  }

  changeDiagram(node: PathNode) {
    var instanceId = node.self.instanceId;
    if (this.selectedNode != instanceId) {
      this.selectedNode = instanceId;
    }
  }

  callDiagram(processInstanceId: string) {
    this.selectedNode = processInstanceId;
  }

  zoomOut() {
    this.http.get(this.baseUrl + 'api/Projects/Super/' + this.selectedNode, { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
      if (result != null)
        this.selectedNode = result;
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  downloadEvidence() {
    this.http.get(this.baseUrl + 'api/Projects/Evidence/' + this.project?.caseInstanceId, { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
      var tmp = document.createElement("a");
      tmp.href = "data:image/png;base64," + result;
      tmp.download = this.project?.make + " " + this.project?.model + " " + this.project?.year + ".pdf";
      tmp.click();
    })
  }

  deleteProject() {
    let confirmedDeletion = confirm("Deleting this project removes all the data stored in the system, closes the running instance in the Workflow Engine and deletes the Pinterest Board. Do you want to proceed?");
    
    if (confirmedDeletion) {
      this.http.delete(this.baseUrl + 'api/Projects/' + this.project?.id, Token.getHeader()).subscribe(result => {
        alert("Project deleted successfully.")
  
      }, error => HandleError.handleError(error, this.router, this.authService)
      , () => {
        this.router.navigate(['/projects']);
      });
    }
  }

  editProject(project: Project) {
    let navigationExtras: NavigationExtras = {
      state: {
        project: project
      }
    };
    this.router.navigate(['/projects/edit/' + project.id], navigationExtras);
  }

  handleImported(event: any) {

    const {
      type,
      error,
      warnings
    } = event;

    if (type === 'success') {
      console.log(`Rendered diagram (%s warnings)`, warnings.length);
    }

    if (type === 'error') {
      HandleError.handleError(error, this.router, this.authService);
      console.error('Failed to render diagram', error);
    }

    this.importError = error;
  }

  format(date?: string) {
    return Format.formatDate(date? date : "");
  }
}
