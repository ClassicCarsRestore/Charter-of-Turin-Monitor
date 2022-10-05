import {
  Component,
  Inject,
  OnInit
} from '@angular/core';

import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  public loggedInRole$: Observable<string>;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
    this.loggedInRole$ = this.authService.loggedInRoleObservable;
    if (this.authService.loggedInRole != "")
      this.router.navigate(['/projects/']);
    else
      this.router.navigate(['/login/']);
  }

  ngOnInit(): void {
  }
}
