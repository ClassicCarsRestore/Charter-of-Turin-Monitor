import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Inject, Input, OnInit, Output, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { BasicNode } from '../basic-node';

@Component({
  selector: 'app-date-picker',
  templateUrl: './date-picker.component.html',
  styleUrls: ['./date-picker.component.css']
})
export class DatePickerComponent implements OnInit {
  // node of which the date in the date picker relates to
  @Input() public selectedNode: BasicNode|null = null;
  // trigger to show the dateTime form on the parent component
  @Output() private closeDatePickerTrigger: EventEmitter<any> = new EventEmitter();
  
  private startDatePicker: HTMLInputElement|undefined = undefined;
  private completedDatePicker: HTMLInputElement|undefined = undefined;

  public error: boolean;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router) {
    this.error = false;
  }

  ngOnChanges(changes: SimpleChanges) {
    this.startDatePicker = document.getElementById("task-start-date") as HTMLInputElement;
    this.completedDatePicker = document.getElementById("task-completion-date") as HTMLInputElement;

    // if no node is selected, change the current value to null
    if (changes.selectedNode.currentValue == null) {
      this.startDatePicker.value = "";
      this.completedDatePicker.value = "";
      return;
    }
    
    let newSelectedNode: BasicNode = changes.selectedNode.currentValue;
    if (newSelectedNode != null) {
      this.startDatePicker.value = newSelectedNode.startTime!.toISOString().split('T')[0];
      this.completedDatePicker.value = newSelectedNode.completionTime!.toISOString().split('T')[0];

      let commentReport = document.getElementById("commentReport") as HTMLInputElement;
      commentReport.value = newSelectedNode.commentReport;

      let commentExtra = document.getElementById("commentExtra") as HTMLInputElement;
      commentExtra.value = newSelectedNode.commentExtra;

      if (newSelectedNode.media.length > 0)
        document.getElementById("media-text")!.innerHTML = newSelectedNode.media.length + " media file" + (newSelectedNode.media.length == 1 ? "" : "s") + " selected.";
      else
        document.getElementById("media-text")!.innerHTML = "";
      if (newSelectedNode.extraMedia.length > 0)
        document.getElementById("media-extra-text")!.innerHTML = newSelectedNode.extraMedia.length + " media file" + (newSelectedNode.extraMedia.length == 1 ? "" : "s") + " selected.";
      else
        document.getElementById("media-extra-text")!.innerHTML = "";

      if (this.startDatePicker.value > this.completedDatePicker.value)
        this.error = true;
      else
        this.error = false;
    }
  }

  ngOnInit() {
  }

  processFile(files: FileList | null) {
    const media = this.selectedNode!.media;
    var n = this.selectedNode!.media.length;
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
    this.selectedNode!.media = media;
    if (n > 0)
      document.getElementById("media-text")!.innerHTML = n + " media file" + (n == 1 ? "" : "s") + " selected.";
    else
      document.getElementById("media-text")!.innerHTML = "";
  }

  deleteFiles() {
    this.selectedNode!.media = [];
    document.getElementById("media-text")!.innerHTML = "";
  }

  processExtraFile(files: FileList | null) {
    const media = this.selectedNode!.extraMedia;
    var n = this.selectedNode!.extraMedia.length;
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
    this.selectedNode!.extraMedia = media;
    if (n > 0)
      document.getElementById("media-extra-text")!.innerHTML = n + " media file" + (n == 1 ? "" : "s") + " selected.";
    else
      document.getElementById("media-extra-text")!.innerHTML = "";
  }

  deleteExtraFiles() {
    this.selectedNode!.extraMedia = [];
    document.getElementById("media-extra-text")!.innerHTML = "";
  }

  saveChanges() {
    this.selectedNode!.startTime = new Date(this.startDatePicker!.value);
    this.selectedNode!.completionTime = new Date(this.completedDatePicker!.value);

    if (this.startDatePicker!.value > this.completedDatePicker!.value)
      this.error = true;
    else
      this.error = false;
  }

  commentReportChange() {
    let t = document.getElementById("commentReport") as HTMLInputElement;
    this.selectedNode!.commentReport = t.value;
  }

  commentExtraChange() {
    let t = document.getElementById("commentExtra") as HTMLInputElement;
    this.selectedNode!.commentExtra = t.value;
  }

  closeButton() {
    this.closeDatePickerTrigger.emit({});
  }
}
