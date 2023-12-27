import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Inject, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { Token } from '../common/tokens';
import { Task, TaskUpdate } from '../task';
import { TaskBCGet } from '../bcTask';

@Component({
  selector: 'app-task-details',
  templateUrl: './task-details.component.html',
  styleUrls: ['./task-details.component.css']
})
export class TaskDetailsComponent implements OnInit {
  // node of which the date in the date picker relates to
  @Input() public selectedTask!: Task;
  // trigger to show the dateTime form on the parent component
  @Output() private closeTaskDetailTrigger: EventEmitter<any> = new EventEmitter();

  private startDatePicker: HTMLInputElement | undefined = undefined;
  private completedDatePicker: HTMLInputElement | undefined = undefined;

  public error: boolean;
  public media: string[];
  public extraMedia: string[];
  public commentReport: string;
  public commentExtra: string;
  public startDate: Date;
  public completionDate: Date;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
    this.media = [];
    this.extraMedia = [];
    this.commentReport = "";
    this.commentExtra = "";
    this.error = false;
    this.startDate = new Date();
    this.completionDate = new Date();
  }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges) {
    let change = changes['selectedTask'];
    if (!change.previousValue || (!change.isFirstChange() && change.previousValue.id != this.selectedTask.id)) {
      this.startDatePicker = document.getElementById("start-date") as HTMLInputElement;
      this.startDatePicker.value = this.selectedTask.startTime.split('T')[0];
      this.startDate = new Date(this.startDatePicker.value);

      this.completedDatePicker = document.getElementById("completion-date") as HTMLInputElement;
      this.completedDatePicker.value = this.selectedTask.completionTime.split('T')[0];
      this.completionDate = new Date(this.completedDatePicker.value);

      if (this.startDatePicker.value > this.completedDatePicker.value)
        this.error = true;
      else
        this.error = false;

      this.commentReport = this.selectedTask.commentReport;
      let commentReport = document.getElementById("comment-report-text") as HTMLInputElement;
      commentReport.value = this.commentReport;

      this.commentExtra = this.selectedTask.commentExtra;
      let commentExtra = document.getElementById("comment-extra-text") as HTMLInputElement;
      commentExtra.value = this.commentExtra;

      this.media = [];
      this.extraMedia = [];
      document.getElementById("media-add-text")!.innerHTML = "";
      document.getElementById("media-add-extra-text")!.innerHTML = "";
    }
  }

  closeButton() {
    this.closeTaskDetailTrigger.emit({});
  }

  openTab(url: string) {
    window.open(url, "_blank");
  }


  processFile(files: FileList | null) {
    const media = this.media;
    var n = this.media.length;
    if (files != null)
      for (var i = 0; i < files.length; i++) {
        if (!files[i].type.includes("video") && !files[i].type.includes("image"))
          continue;
        n++;
        const reader = new FileReader();

        reader.readAsDataURL(files[i]);
        reader.onload = function () {
          if (reader.result)
            media.push(reader.result.toString());
        };
      }
    this.media = media;
    if (n > 0)
      document.getElementById("media-add-text")!.innerHTML = n + " media file" + (n == 1 ? "" : "s") + " selected.";
    else
      document.getElementById("media-add-text")!.innerHTML = "";
  }

  deleteFiles() {
    this.media = [];
    document.getElementById("media-add-text")!.innerHTML = "";
  }

  processExtraFile(files: FileList | null) {
    const media = this.extraMedia;
    var n = this.extraMedia.length;
    if (files != null)
      for (var i = 0; i < files.length; i++) {
        if (!files[i].type.includes("video") && !files[i].type.includes("image"))
          continue;
        n++;
        const reader = new FileReader();

        reader.readAsDataURL(files[i]);
        reader.onload = function () {
          if (reader.result)
            media.push(reader.result.toString());
        };
      }
    this.extraMedia = media;
    if (n > 0)
      document.getElementById("media-add-extra-text")!.innerHTML = n + " media file" + (n == 1 ? "" : "s") + " selected.";
    else
      document.getElementById("media-add-extra-text")!.innerHTML = "";
  }

  deleteExtraFiles() {
    this.extraMedia = [];
    document.getElementById("media-add-extra-text")!.innerHTML = "";
  }

  saveChanges() {
    this.startDate = new Date(this.startDatePicker!.value);
    this.completionDate = new Date(this.completedDatePicker!.value);

    if (this.startDatePicker!.value > this.completedDatePicker!.value)
      this.error = true;
    else
      this.error = false;
  }

  commentReportChange() {
    let t = document.getElementById("comment-report-text") as HTMLInputElement;
    this.commentReport = t.value;
  }

  commentExtraChange() {
    let t = document.getElementById("comment-extra-text") as HTMLInputElement;
    this.commentExtra = t.value;
  }

  submitUpdates() {
    var updates = new TaskUpdate(this.startDate.toISOString(), this.completionDate.toISOString(), this.commentReport, this.commentExtra, this.media, this.extraMedia, this.selectedTask.blockChainId);
    this.http.post(this.baseUrl + 'api/Tasks/' + this.selectedTask?.processInstanceId + "/" + this.selectedTask?.id + '/Update', updates, Token.getHeader()).subscribe(async result => {
      //Blockchain
      console.log("Taskkk debug");
      console.log(this.selectedTask);
      console.log("process instance");
      console.log(this.selectedTask?.processInstanceId)
      console.log("blockchain id");
      console.log(this.selectedTask?.blockChainId);
      const parts: string[] = this.selectedTask.blockChainId.split("_");
      const chassisNo: string = parts[0];
      const formData = new FormData();
      formData.append('stepId', String(this.selectedTask.blockChainId));
      let task;
      await this.http.get<TaskBCGet>('https://gui.classicschain.com:8393/api/Restorations/Get/' + chassisNo +'/'+this.selectedTask.blockChainId, Token.getHeaderBC()).subscribe(async result2 => {
        task = result2;
        formData.append('newTitle', task.title);
        formData.append('newDescription', this.commentReport);
        for(let file of this.media) {
          let imageFile = this.base64ToFile(file, "filename");
          formData.append('file', imageFile);
        }
        await this.http.put('https://gui.classicschain.com:8393/api/Restorations/UpdateAndPhotos/' + chassisNo, formData, Token.getHeaderBC()).subscribe();
      });
      //Blockchain - End
      this.closeButton();
      alert("Task updated successfully.");
    }, (error => {
      alert("An error has occured with the task update. Please refresh the page and try again.");
      console.error(error);
    }));
  }

  private base64ToFile(base64String: string, filename:string) {
    let arr = base64String.split(',');
    let match = arr[0].match(/:(.*?);/);
    
    if (!match) {
        throw new Error('Invalid base64 string format');
    }

    let mimeType = match[1];
    let bstr = atob(arr[1]);
    let n = bstr.length;
    let u8arr = new Uint8Array(n);
    
    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }
    
    return new File([u8arr], filename, { type: mimeType });
  }
}
