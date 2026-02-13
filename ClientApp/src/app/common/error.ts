import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { AuthService } from '../auth.service';

export class HandleError {
  static handleError(error: HttpErrorResponse, router: Router, authService: AuthService) {
    if (error.status === 0) {
      console.error('An error occurred:', error.error);
    } else {
      if (error.status == 401) {
        window.location.href = '/outpost.goauthentik.io/start';
      }
      else if (error.status == 403) {
        router.navigate(['/']);
      }
      else if (error.status == 409) {
        alert("The account you are trying to create already exists.");
      }
      else if (error.status == 500) {
        alert("Unexpected server error. Contact the admin.");
      }
      console.error(
        `Backend returned code ${error.status}, body was: `, error.error);
    }
    return throwError(() => new Error('Something bad happened; please try again later.'));
  }
}
