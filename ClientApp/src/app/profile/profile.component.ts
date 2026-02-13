import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { MatAccordion } from '@angular/material/expansion';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { HandleError } from '../common/error';
import { Token } from '../common/tokens';
import { Details } from '../user';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  @ViewChild(MatAccordion) accordion?: MatAccordion;

  address?: string;
  phone?: string;
  name?: string;

  editDetailsForm = new FormGroup({
    address: new FormControl(),
    phone: new FormControl(),
    name: new FormControl()
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

  editDetails(form: FormGroup) {
    let detailsform = new Details(form.get("address")?.value, form.get("phone")?.value, form.get("name")?.value)
    this.http.post(this.baseUrl + 'api/Account/Details', detailsform, Token.getHeader()).subscribe(result => {
      alert("Details changed successfully.");
      this.accordion?.closeAll();
    }, error => HandleError.handleError(error, this.router, this.authService));
  }
}
