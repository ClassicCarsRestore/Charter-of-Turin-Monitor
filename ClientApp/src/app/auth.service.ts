import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { Token } from './common/tokens';
import { Account } from './user';

@Injectable()
export class AuthService {

  role = new BehaviorSubject<string>("");

  get isLoggedIn() {
    return this.role.value !== "";
  }

  get loggedInRole() {
    return this.role.asObservable();
  }

  constructor(public client: HttpClient, @Inject('BASE_URL') public baseUrl: string, private router: Router) {
    this.checkSession();
  }

  checkSession() {
    this.client.get<Account>(this.baseUrl + 'api/Account/Self/', Token.getHeader()).subscribe(result => {
      this.role.next(result.role);
    }, error => {
      this.role.next("");
    });
  }

  logout() {
    localStorage.removeItem('ChainToken');
    this.role.next("");
    window.location.href = '/outpost.goauthentik.io/sign_out';
  }
}
