import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, FormGroupDirective, NgForm, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatAccordion } from '@angular/material/expansion';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { HandleError } from '../common/error';
import { Token } from '../common/tokens';
import { ChangePasswordForm, Details } from '../user';

export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    return !!(control && control.parent && control.parent.invalid && control.parent.dirty && control.touched && control.parent.hasError("notSame"));
  }
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  @ViewChild(MatAccordion) accordion?: MatAccordion;
  oldHide = true;
  hide = true;
  hideConfirm = true;

  address?: string;
  phone?: string;
  name?: string;

  matcher = new MyErrorStateMatcher();

  checkPasswords: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    let pass = group.get('password')?.value;
    let confirmPass = group.get('confirmPassword')?.value;
    return pass === confirmPass ? null : { notSame: true }
  }
  changePasswordForm = new FormGroup({
    oldPassword: new FormControl('', [Validators.required, Validators.minLength(6)]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)]),
    confirmPassword: new FormControl(''),
  }, { validators: this.checkPasswords });

  editDetailsForm = new FormGroup({
    address: new FormControl(),
    phone: new FormControl(),
    name: new FormControl([Validators.required])
  })

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private authService: AuthService) {
  }

  async ngOnInit(): Promise<void> {
    await this.http.get<Details>(this.baseUrl + 'api/Account/Details', Token.getHeader()).toPromise().then(result => {
      this.address = result.address;
      this.phone = result.phone;
      this.name = result.name;
    }).catch(error => HandleError.handleError(error, this.router, this.authService));
    this.editDetailsForm = new FormGroup({
      address: new FormControl(this.address),
      phone: new FormControl(this.phone),
      name: new FormControl(this.name)
    })
  }

  changePassword(form: FormGroup) {
    let passwordForm = new ChangePasswordForm(form.get("oldPassword")?.value, form.get("password")?.value)
    this.http.post(this.baseUrl + 'api/Account/Password', passwordForm, Token.getHeader()).subscribe(result => {
      alert("Password changed successfully.");
      this.accordion?.closeAll();
    }, error => {
      if (error.status == 400)
        alert("Incorrect Password.");
      else
        HandleError.handleError(error, this.router, this.authService);
    });
  }

  editDetails(form: FormGroup) {
    let detailsform = new Details(form.get("address")?.value, form.get("phone")?.value, form.get("name")?.value)
    this.http.post(this.baseUrl + 'api/Account/Details', detailsform, Token.getHeader()).subscribe(result => {
      alert("Details changed successfully.");
      this.accordion?.closeAll();
    }, error => HandleError.handleError(error, this.router, this.authService));
  }
}
