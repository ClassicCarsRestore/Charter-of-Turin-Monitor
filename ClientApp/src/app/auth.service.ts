import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { Token } from './common/tokens';
import { Credentials, CredentialToken, CredentialsBC, CredentialBCToken } from './credentials';
import { Account } from './user';
import { envBC } from './envBC';

@Injectable()
export class AuthService {
  private role: BehaviorSubject<string> = new BehaviorSubject<string>("");

  get loggedInRoleObservable() {
    return this.role.asObservable();
  }

  get loggedInRole() {
    return this.role.value;
  }

  constructor(public client: HttpClient, @Inject('BASE_URL') public baseUrl: string, private router: Router) {
    let token = localStorage.getItem("RBToken") as string;
    if (token != null) {
      this.client.get<Account>(this.baseUrl + 'api/Account/Self/', Token.getHeader()).subscribe(result => {
            this.role.next(result.role);
        }, error => {
            this.logout();
        })
    }
  }

  login(cred: Credentials) {
    if (cred.email !== '' && cred.password !== '') {
      this.client.post<CredentialToken>(this.baseUrl + 'api/Account/Login', cred).subscribe(result => {
        let response = result;
        localStorage.setItem('RBToken', response.token);
        this.role.next(response.role);

        if (response.role == "admin") {
          this.router.navigate(['pinterest']);
          let userChain = new CredentialsBC(envBC.adminBCEmail, envBC.adminBCpassword, envBC.adminBCorg)
          this.client.post<CredentialBCToken>('http://194.210.120.34:8393/api/Users/Login', userChain).subscribe(result2 => {
            localStorage.setItem('ChainToken', result2.message.token);
          });
          
        }
        else
          this.router.navigate(['']);
      }, error => {
        document.getElementById("passwordEmpty")!.innerHTML = "The Username and Password do not match!";
        console.log(error);
      })
    }
  }

  logout() {
    localStorage.removeItem('RBToken');
    localStorage.removeItem('ChainToken');
    this.role.next("");
    this.router.navigate(['/login']);
  }
}
