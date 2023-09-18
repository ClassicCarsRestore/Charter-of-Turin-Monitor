export interface Task {
  id: string;
  processInstanceId: string;
  startTime: string;
  completionTime: string;
  boardSectionUrl: string;
  commentReport: string;
  commentExtra: string;
  blockChainId: string;
}

export class TasksToApprove {
  public tasks: TaskToApprove[];
  public variables: string[][];
  public startEventTriggers: string[];
  public processInstanceId: string;

  constructor(tasks: TaskToApprove[], variables: string[][], startEventTriggers: string[], processInstanceId: string) {
    this.tasks = tasks;
    this.variables = variables;
    this.startEventTriggers = startEventTriggers;
    this.processInstanceId = processInstanceId;
  }
}

export class TaskToApprove {
  public id: string;
  public startTime: string;
  public completionTime: string;
  public message: string;
  public commentReport: string;
  public commentExtra: string;
  public media: string[];
  public extraMedia: string[];

  constructor(id: string, startTime: string, completionTime: string, message: string, commentReport: string, commentExtra: string, media: string[], extraMedia: string[]) {
    this.id = id;
    this.startTime = startTime;
    this.completionTime = completionTime;
    this.message = message;
    this.commentReport = commentReport;
    this.commentExtra = commentExtra;
    this.media = media;
    this.extraMedia = extraMedia;
  }
}

export class TaskUpdate {
  public startDate: string;
  public completionDate: string;
  public commentReport: string;
  public commentExtra: string;
  public media: string[];
  public extraMedia: string[];
  public blockChainId: string;

  constructor(startDate: string, completionDate: string, commentReport: string, commentExtra: string, media: string[], extraMedia: string[], blockChainId: string) {
    this.startDate = startDate;
    this.completionDate = completionDate;
    this.commentReport = commentReport;
    this.commentExtra = commentExtra;
    this.media = media;
    this.extraMedia = extraMedia;
    this.blockChainId = blockChainId;
  }
}

export interface PredictedTasks {
  activityId: string;
  startTime: string;
  endTime: string;
}

export interface CompletedHistoryTasks {
  activityName: string;
  startTime: string;
  completionTime: string;
  submitionTime: string;
  boardSectionUrl: string;
}

export interface PathTask {
  activityName: string;
  processDefinitionId: string;
  processInstanceId: string;
  startTime: string;
}

export interface CreateTaskBCResponse {
  success: string;
  message: string;
  stepId: string;
}
