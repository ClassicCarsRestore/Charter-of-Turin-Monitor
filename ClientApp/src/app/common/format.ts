import { CompletedHistoryTasks } from "../task";

export class Format{
  static formatTask(task: CompletedHistoryTasks) {
    task.startTime = this.formatDate(task.startTime);
    task.completionTime = this.formatDate(task.completionTime);

    return task;
  }

  static formatDate(date: string) {
    let dateTime: string[] = date.split("T");

    //return dateTime[0] + " " + dateTime[1].substring(0, 8);
    return dateTime[0];
  }

  static formatDateSort(start: string, completion: string) {
    let startDateTime = start.split("T");
    let completionDateTime = completion.split("T");

    let startDate = startDateTime[0].split("-");
    let startTime = startDateTime[1].substring(0, 8).split(":");

    let completionDate = completionDateTime[0].split("-");
    let completionTime = completionDateTime[1].substring(0, 8).split(":");

    //return parseInt(startDate.join("") + startTime.join("") + completionDate.join("") + completionTime.join(""));
    return parseInt(startDate.join("") + completionDate.join(""));
  }
}
