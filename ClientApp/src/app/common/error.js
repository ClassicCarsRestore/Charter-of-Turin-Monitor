"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.HandleError = void 0;
var rxjs_1 = require("rxjs");
var HandleError = /** @class */ (function () {
    function HandleError() {
    }
    HandleError.handleError = function (error, router, authService) {
        if (error.status === 0) {
            // A client-side or network error occurred. Handle it accordingly.
            console.error('An error occurred:', error.error);
        }
        else {
            // The backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong.
            if (error.status == 401) {
                router.navigate(['/login']);
                authService.logout();
            }
            else if (error.status == 403) {
                router.navigate(['/']);
            }
            else if (error.status == 400) {
                alert("An error has occured.");
            }
            console.error("Backend returned code ".concat(error.status, ", body was: "), error.error);
        }
        // Return an observable with a user-facing error message.
        return (0, rxjs_1.throwError)(function () { return new Error('Something bad happened; please try again later.'); });
    };
    return HandleError;
}());
exports.HandleError = HandleError;
//# sourceMappingURL=error.js.map