import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, FormGroupDirective, NgForm, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatAccordion } from '@angular/material/expansion';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { SendCredentials } from '../camera-hub';
import { HandleError } from '../common/error';
import { Token } from '../common/tokens';
import { Project } from '../project';
import { Account } from '../user';

export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    return !!(control && control.parent && control.parent.invalid && control.parent.dirty && control.touched  && control.parent.hasError("notSame"));
  }
}

@Component({
  selector: 'app-admin-page',
  templateUrl: './admin-page.component.html',
  styleUrls: ['./admin-page.component.css']
})
export class AdminPageComponent implements OnInit {
  @ViewChild(MatAccordion) accordion?: MatAccordion;
  stepMinute: number = 15;

  hide = true;
  hideConfirm = true;
  minDate: Date;
  defaultDate: Date;
  users: Account[] = [];
  projects: Project[] = [];
  
  matcher = new MyErrorStateMatcher();

  checkPasswords: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    let pass = group.get('password')?.value;
    let confirmPass = group.get('confirmPassword')?.value;
    return pass === confirmPass ? null : { notSame: true }
  }

  newAccountForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    name: new FormControl('', [Validators.required]),
    //password: new FormControl('', [Validators.required, Validators.minLength(6)]),
    //confirmPassword: new FormControl(''),
    accountRole: new FormControl('', [Validators.required]),
  }, { validators: this.checkPasswords });

  delAccountForm = new FormGroup({
    account: new FormControl('', [Validators.required]),
  });

  sendCredentialsForm = new FormGroup({
    project: new FormControl('', [Validators.required]),
    startDateTime: new FormControl('', [Validators.required]),
    endDateTime: new FormControl('', [Validators.required]),
  });

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private authService: AuthService) {
    this.minDate = new Date();
    this.defaultDate = new Date();
    let minutes = this.minDate.getMinutes();
    minutes += (60 - minutes) % this.stepMinute;
    this.defaultDate.setMinutes(minutes);
    this.defaultDate.setSeconds(0);
    this.http.get<Project[]>(this.baseUrl + 'api/Projects', Token.getHeader()).subscribe(result => {
      this.projects = result;
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  async ngOnInit(): Promise<void> {
    let user: Account;
    await this.http.get<Account>(this.baseUrl + 'api/Account/Self', Token.getHeader()).toPromise().then(result => {
      user = result;
    }).catch(error => HandleError.handleError(error, this.router, this.authService));
    this.http.get<Account[]>(this.baseUrl + 'api/Account/', Token.getHeader()).subscribe(result => {
      this.users = result;
      this.users.splice(this.users.findIndex(u => u.email == user.email && u.role == user.role), 1);
    }, error => HandleError.handleError(error, this.router, this.authService));

  }

  createAccount(form: FormGroup) {
    let user = new Account(form.get("email")?.value, form.get("accountRole")?.value, form.get("name")?.value)
    this.http.post<Account>(this.baseUrl + 'api/Account/Create', user, Token.getHeader()).subscribe(result => {
      alert("Account created successfully.");
      this.accordion?.closeAll();
    }, error => {
      if (error.status == 409)
        alert("An account with this email already exists.")
      else
        HandleError.handleError(error, this.router, this.authService);
    })
  }

  delAccount(form: FormGroup) {
    let confirmedDeletion = confirm("Deleting this account removes all the data stored in the system. Do you want to proceed?");

    if (confirmedDeletion) {
      this.http.post(this.baseUrl + 'api/Account/Delete', form.get("account")?.value, Token.getHeader()).subscribe(result => {
        alert("Account deleted successfully.");
        this.accordion?.closeAll();
        this.users.splice(this.users.findIndex(u => u.email == form.get("account")?.value.email && u.role == form.get("account")?.value.role), 1);
      }, error => HandleError.handleError(error, this.router, this.authService))
    }
  }

  sendCredentials(form: FormGroup) {
    console.log(form.get("project")?.value.id)
    let send = new SendCredentials(form.get("project")?.value.id, form.get("startDateTime")?.value, form.get("endDateTime")?.value)
    this.http.post<SendCredentials>(this.baseUrl + 'api/CameraHub/Credentials', send, Token.getHeader()).subscribe(result => {
      alert("Credentials sent successfully.");
      this.accordion?.closeAll();
    }, error => HandleError.handleError(error, this.router, this.authService))
  }

  getErrorMessage() {
    if (this.newAccountForm.get("email")?.hasError('required')) {
      return 'You must enter an email';
    }

    return this.newAccountForm.get("email")?.hasError('email') ? 'Not a valid email' : '';
  }
}
