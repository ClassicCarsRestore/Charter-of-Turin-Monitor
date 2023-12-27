import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Project, ProjectForm } from '../project';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { AuthService } from '../auth.service';
import { Account } from '../user';
import { BcClassicForm } from '../bcTask';

@Component({
  selector: 'app-project-form',
  templateUrl: './project-form.component.html',
  styleUrls: ['./project-form.component.css']
})
export class ProjectFormComponent implements OnInit {

  public photo?: string[];
  public project!: Project;
  public users: Account[] = [];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private authService: AuthService) {
    this.http.get<Account[]>(this.baseUrl + 'api/Account/', Token.getHeader()).subscribe(result => {
      this.users = result.filter(u => u.role == "owner");
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  processFile(files: FileList | null) {
    const media: string[] = [];
    if (files != null && files.length > 0){
      const reader = new FileReader();

      reader.readAsDataURL(files[0]);
      reader.onload = function () {
        if (reader.result)
          media.push(reader.result.toString());
      };
    }
    this.photo = media;
  }

  onContinue() {

    var hasErrors: boolean = false;

    var make = (<HTMLInputElement>document.getElementById("make")).value;

    // disallow submitting without writting the vehicle's make
    if (make.trim() == "") {
      hasErrors = true;
      document.getElementById("makeEmpty")!.innerHTML = "Vehicle's make can't be empty"; 
    } else 
      document.getElementById("makeEmpty")!.innerHTML = "";

    var model = (<HTMLInputElement>document.getElementById("model")).value;

    // disallow submitting without writting the vehicle's model
    if (model.trim() == "") {
      hasErrors = true;
      document.getElementById("modelEmpty")!.innerHTML = "Vehicle's model can't be empty"; 
    } else 
      document.getElementById("modelEmpty")!.innerHTML = "";

    var year = Number((<HTMLInputElement>document.getElementById("year")).value);

    // disallow submitting without writting a valid manifacture year
    if (isNaN(year) || year <= 0) {
      hasErrors = true;
      document.getElementById("yearEmpty")!.innerHTML = "Vehicle's year of manifacture must be a positive number";
    } else
      document.getElementById("yearEmpty")!.innerHTML = "";

    var licencePlate = (<HTMLInputElement>document.getElementById("licencePlate")).value;
    
    // disallow submitting without writting a license plate
    if (licencePlate.trim() == "") {
      hasErrors = true;
      document.getElementById("licencePlateEmpty")!.innerHTML = "Vehicle's license plate can't be empty"; 
    } else 
      document.getElementById("licencePlateEmpty")!.innerHTML = "";

    var country = (<HTMLInputElement>document.getElementById("country")).value;
    
    // disallow submitting without writting a country
    if (country.trim() == "") {
      hasErrors = true;
      document.getElementById("countryEmpty")!.innerHTML = "Vehicle's country can't be empty"; 
    } else 
      document.getElementById("countryEmpty")!.innerHTML = "";

    var chassisNo = (<HTMLInputElement>document.getElementById("chassisNo")).value;
    
    // disallow submitting without writting a chassis number
    if (chassisNo.trim() == "") {
      hasErrors = true;
      document.getElementById("chassisNoEmpty")!.innerHTML = "Vehicle's chassis number can't be empty"; 
    } else 
      document.getElementById("chassisNoEmpty")!.innerHTML = "";

    var engineNo = (<HTMLInputElement>document.getElementById("engineNo")).value;
    
    // disallow submitting without writting the engine details
    if (engineNo.trim() == "") {
      hasErrors = true;
      document.getElementById("engineNoEmpty")!.innerHTML = "Vehicle's engine details can't be empty"; 
    } else 
      document.getElementById("engineNoEmpty")!.innerHTML = "";
    
    var ownerEmail = (<HTMLInputElement>document.getElementById("ownerEmail")).value;

    // disallow submitting without selecting an owner
    if (ownerEmail.trim() == "") {
      hasErrors = true;
      document.getElementById("ownerEmailEmpty")!.innerHTML = "An owner's email needs to be selected";
    } else
      document.getElementById("ownerEmailEmpty")!.innerHTML = "";

    // disallow submitting without selecting a photo
    if (!this.photo || this.photo.length == 0) {
      hasErrors = true;
      document.getElementById("photoEmpty")!.innerHTML = "Photo of the car needs to be selected";
    } else 
      document.getElementById("photoEmpty")!.innerHTML = "";

    if (!hasErrors) {
      var clientExpectation = (<HTMLInputElement>document.getElementById("clientExpectation")).value;
      var originalMaterials = (<HTMLInputElement>document.getElementById("originalMaterials")).checked;
      var carDocuments = (<HTMLInputElement>document.getElementById("carDocuments")).checked;

      // get the current Date object
      let currentDate: Date = new Date();
      // fix the problem caused by summer time
      currentDate.setTime( currentDate.getTime() - new Date().getTimezoneOffset() * 60 * 1000 );

      // build an object with the project name and creation datetime
      let proj = new ProjectForm(make, model, year, licencePlate, country, chassisNo, engineNo, ownerEmail, clientExpectation, this.photo![0], originalMaterials, carDocuments, currentDate.toISOString());

      this.http.post<Project>(this.baseUrl + 'api/Projects', proj, Token.getHeader()).subscribe(result => {
        this.project = result;
        window.open(result.pinterestBoardUrl, "_blank");
        //Blockchain
        this.http.get('https://gui.classicschain.com:8393/api/Classics/Get/' + chassisNo, Token.getHeaderBC()).subscribe(result2 => {
          console.log(result2);
        }, error => {
          //If the classic with the given chassisNo is not found, then create it
          if (error && error.error && error.error.message === "404 - The classic "+chassisNo+" was Not Found") {
            let classic = new BcClassicForm(make, model, year, licencePlate, country, chassisNo, engineNo, ownerEmail);
            this.http.post('https://gui.classicschain.com:8393/api/Classics/Create', classic, Token.getHeaderBC()).subscribe(result3 => {
              console.log(result3);
            });
          }
        });
      }, error => HandleError.handleError(error, this.router, this.authService)); 
    }
  }

  onSubmit() {
    var pinterestUrl = (<HTMLInputElement>document.getElementById("pinterestUrl")).value;

    // disallow submitting without writting the pinterest Url
    if (pinterestUrl.trim() == "")
      document.getElementById("pinterestUrlEmpty")!.innerHTML = "Pinterest board access url can't be empty";
    else if(this.project){
      this.project.pinterestBoardAccessUrl = pinterestUrl;
      this.project.photo = "";
      this.http.put(this.baseUrl + 'api/Projects/' + this.project.id, this.project, Token.getHeader()).subscribe(result => {
        this.router.navigate(['/projects']);
      }, error => HandleError.handleError(error, this.router, this.authService));
    }
  }
  
  ngOnInit() {
  }

}
