import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  isExpanded = false;
  loggedInRole$: Observable<string>;

  constructor(public authService: AuthService) {
    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  ngOnInit() {
    this.loggedInRole$ = this.authService.loggedInRoleObservable;
  }

  logout() {
    this.authService.logout();
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
