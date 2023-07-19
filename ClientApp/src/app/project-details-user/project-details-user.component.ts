import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { HandleError } from '../common/error';
import { Format } from '../common/format';
import { Token } from '../common/tokens';
import { Project, ProjectForm } from '../project';
import { CompletedHistoryTasks } from '../task';


@Component({
  selector: 'app-project-details-user',
  templateUrl: './project-details-user.component.html',
  styleUrls: ['./project-details-user.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class ProjectDetailsUserComponent implements OnInit {

  public columns = [
    {
      columnDef: 'activityName',
      header: 'Task',
      cell: (task: CompletedHistoryTasks) => `${task.activityName}`,
    },
    {
      columnDef: 'startTime',
      header: 'Start Date',
      cell: (task: CompletedHistoryTasks) => `${task.startTime}`,
    },
    {
      columnDef: 'completionTime',
      header: 'Completion Date',
      cell: (task: CompletedHistoryTasks) => `${task.completionTime}`,
    },
    {
      columnDef: 'photos',
      header: 'Evidence',
      cell: (task: CompletedHistoryTasks) => `${task.boardSectionUrl}`,
    }
  ];

  public displayedColumns = this.columns.map(c => c.columnDef);

  public project?: Project;
  public dataSource: CompletedHistoryTasks[] = [];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
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
    }

    this.http.get<CompletedHistoryTasks[]>(this.baseUrl + 'api/Tasks/' + this.project?.caseInstanceId + "/History", Token.getHeader()).subscribe(result => {
      this.dataSource = result;
      this.dataSource.sort((a, b) => a.submitionTime.localeCompare(b.submitionTime));
      this.dataSource.forEach(t => Format.formatTask(t))
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  openTab(url: string) {
    window.open(url, "_blank");
  }

  downloadEvidence() {
    try {
      // Create and display the spinner element
      alert("This operation may take a while. Please wait while the PDF is being generated.");
  
      var overlay = document.createElement("div");
      overlay.className = "overlay";
      document.body.appendChild(overlay);
  
      var spinner = document.createElement("div");
      spinner.className = "spinner";
      document.body.appendChild(spinner);
  
      // Show the overlay
      overlay.style.display = "block";
  
      document.body.style.cursor = "progress";
      this.http.get(this.baseUrl + 'api/Projects/Evidence/' + this.project?.caseInstanceId, { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
        var tmp = document.createElement("a");
        tmp.href = "data:image/png;base64," + result;
        tmp.download = this.project?.make + " " + this.project?.model + " " + this.project?.year + ".pdf";
        tmp.click();
  
        // Remove the spinner and overlay once the download is complete
        document.body.removeChild(spinner);
        document.body.removeChild(overlay);
      });
    } finally {
      document.body.style.cursor = "auto";
    }
  }

  format(date: string) {
    return Format.formatDate(date);
  }
}
