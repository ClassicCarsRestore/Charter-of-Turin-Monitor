import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { Credentials } from '../credentials';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  constructor(public client: HttpClient, @Inject('BASE_URL') public baseUrl: string, public router: Router, private authService: AuthService) {}

  login(event: Event, username: string, password: string) {
    event.preventDefault();

    var hasErrors: boolean = false;

    // disallow submitting without writting a username
    if (username == "") {
      hasErrors = true;
      document.getElementById("usernameEmpty")!.innerHTML = "Invalid Username!";
    } else
      document.getElementById("usernameEmpty")!.innerHTML = "";

    // disallow submitting without writting a password
    if (password == "") {
      hasErrors = true;
      document.getElementById("passwordEmpty")!.innerHTML = "Invalid Password!";
    } else
      document.getElementById("passwordEmpty")!.innerHTML = "";


    if (!hasErrors) {
      this.authService.login(new Credentials(username, password));
    }
  }

  ngOnInit(): void {
  }

  //signup(event: Event) {
  //  event.preventDefault();
  //  this.router.navigate(['signup']);
  //}
}
