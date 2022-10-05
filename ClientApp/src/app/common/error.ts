import { HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";
import { throwError } from "rxjs";
import { AuthService } from "../auth.service";

export class HandleError{
  static handleError(error: HttpErrorResponse, router: Router, authService: AuthService) {
    if (error.status === 0) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error);
    } else {
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
        alert("An error has occured.")
      }
      console.error(
        `Backend returned code ${error.status}, body was: `, error.error);
    }
    // Return an observable with a user-facing error message.
    return throwError(() => new Error('Something bad happened; please try again later.'));
  }
}
