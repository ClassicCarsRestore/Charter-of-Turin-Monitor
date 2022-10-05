"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Format = void 0;
var Format = /** @class */ (function () {
    function Format() {
    }
    Format.formatTask = function (task) {
        task.startTime = this.formatDate(task.startTime);
        task.completionTime = this.formatDate(task.completionTime);
        return task;
    };
    Format.formatDate = function (date) {
        var dateTime = date.split("T");
        //return dateTime[0] + " " + dateTime[1].substring(0, 8);
        return dateTime[0];
    };
    Format.formatDateSort = function (start, completion) {
        var startDateTime = start.split("T");
        var completionDateTime = completion.split("T");
        var startDate = startDateTime[0].split("-");
        var startTime = startDateTime[1].substring(0, 8).split(":");
        var completionDate = completionDateTime[0].split("-");
        var completionTime = completionDateTime[1].substring(0, 8).split(":");
        //return parseInt(startDate.join("") + startTime.join("") + completionDate.join("") + completionTime.join(""));
        return parseInt(startDate.join("") + completionDate.join(""));
    };
    return Format;
}());
exports.Format = Format;
//# sourceMappingURL=format.js.map