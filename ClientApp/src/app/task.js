"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TaskUpdate = exports.TaskToApprove = exports.TasksToApprove = void 0;
var TasksToApprove = /** @class */ (function () {
    function TasksToApprove(tasks, variables, startEventTriggers, processInstanceId) {
        this.tasks = tasks;
        this.variables = variables;
        this.startEventTriggers = startEventTriggers;
        this.processInstanceId = processInstanceId;
    }
    return TasksToApprove;
}());
exports.TasksToApprove = TasksToApprove;
var TaskToApprove = /** @class */ (function () {
    function TaskToApprove(id, startTime, completionTime, message, commentReport, commentExtra, media, extraMedia) {
        this.id = id;
        this.startTime = startTime;
        this.completionTime = completionTime;
        this.message = message;
        this.commentReport = commentReport;
        this.commentExtra = commentExtra;
        this.media = media;
        this.extraMedia = extraMedia;
    }
    return TaskToApprove;
}());
exports.TaskToApprove = TaskToApprove;
var TaskUpdate = /** @class */ (function () {
    function TaskUpdate(startDate, completionDate, commentReport, commentExtra, media, extraMedia) {
        this.startDate = startDate;
        this.completionDate = completionDate;
        this.commentReport = commentReport;
        this.commentExtra = commentExtra;
        this.media = media;
        this.extraMedia = extraMedia;
    }
    return TaskUpdate;
}());
exports.TaskUpdate = TaskUpdate;
//# sourceMappingURL=task.js.map