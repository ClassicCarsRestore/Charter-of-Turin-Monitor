import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { HandleError } from '../common/error';
import { Token } from '../common/tokens';
import { Project, ProjectForm } from '../project';
import { Account } from '../user';

@Component({
  selector: 'app-edit-project',
  templateUrl: './edit-project.component.html',
  styleUrls: ['./edit-project.component.css']
})
export class EditProjectComponent implements OnInit {

  public users: Account[] = [];
  public photo?: string[];
  public project?: Project;

  constructor(private client: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
  }

  async ngOnInit() {
    // get the selected Project info
    if (this.router != null && this.router.getCurrentNavigation()?.extras.state)
      this.project = this.router.getCurrentNavigation()!.extras.state!.project;
    else { // retrieve the project by id from url

      let projectId: string = "";

      // get id from url
      this.activatedRoute.paramMap.subscribe(params => {
        var tmpProjectId: string | null = params.get('projectId');
        if (tmpProjectId != null)
          projectId = tmpProjectId;
      });

      await this.client.get<Project>(this.baseUrl + 'api/Projects/' + projectId + '/DTO', Token.getHeader()).toPromise().then(result => {
        this.project = result;
      }).catch(error => {
        HandleError.handleError(error, this.router, this.authService)
        alert("This project couldn't be loaded.");
        this.router.navigate(['/projects']);
      });
    }
    this.client.get<Account[]>(this.baseUrl + 'api/Account/', Token.getHeader()).subscribe(async result => {
      this.users = result.filter(u => u.role == "owner");
      await new Promise(r => setTimeout(r, 100));
      (<HTMLInputElement>document.getElementById("ownerEmail")).value = "" + this.project?.ownerEmail;
    }, error => HandleError.handleError(error, this.router, this.authService));
  }

  processFile(files: FileList | null) {
    const media: string[] = [];
    if (files != null && files.length > 0) {
      const reader = new FileReader();

      reader.readAsDataURL(files[0]);
      reader.onload = function () {
        if (reader.result)
          media.push(reader.result.toString());
      };
    }
    this.photo = media;
  }

  onSubmit() {

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

    var pinterestUrl = (<HTMLInputElement>document.getElementById("pinterestUrl")).value;

    // disallow submitting without writting the pinterest Url
    if (pinterestUrl.trim() == "") {
      hasErrors = true;
      document.getElementById("pinterestUrlEmpty")!.innerHTML = "Pinterest board access url can't be empty";
    } else
      document.getElementById("pinterestUrlEmpty")!.innerHTML = "";

    var paintRecordNumber = (<HTMLInputElement>document.getElementById("paintRecordNumber")).value;

    // disallow submitting without writting the paintRecordNumber
    if (paintRecordNumber.trim() == "") {
      hasErrors = true;
      document.getElementById("paintRecordNumberEmpty")!.innerHTML = "Paint record number can't be empty";
    } else
      document.getElementById("paintRecordNumberEmpty")!.innerHTML = "";

    var paintDesignation = (<HTMLInputElement>document.getElementById("paintDesignation")).value;

    // disallow submitting without writting the paintDesignation
    if (paintDesignation.trim() == "") {
      hasErrors = true;
      document.getElementById("paintDesignationEmpty")!.innerHTML = "Paint designation can't be empty";
    } else
      document.getElementById("paintDesignationEmpty")!.innerHTML = "";

    var paintTechnique = (<HTMLInputElement>document.getElementById("paintTechnique")).value;

    // disallow submitting without writting the paintTechnique
    if (paintTechnique.trim() == "") {
      hasErrors = true;
      document.getElementById("paintTechniqueEmpty")!.innerHTML = "Paint technique can't be empty";
    } else
      document.getElementById("paintTechniqueEmpty")!.innerHTML = ""

    var paintOriginalYear = (<HTMLInputElement>document.getElementById("paintOriginalYear")).value;

    // disallow submitting without writting the paintOriginalYear
    if (paintOriginalYear.trim() == "") {
      hasErrors = true;
      document.getElementById("paintOriginalYearEmpty")!.innerHTML = "The original year of the paint can't be empty";
    } else
      document.getElementById("paintOriginalYearEmpty")!.innerHTML = ""

    var paintDate = (<HTMLInputElement>document.getElementById("paintDate")).value;

    // disallow submitting without writting the paintDate
    if (paintDate.trim() == "") {
      hasErrors = true;
      document.getElementById("paintDateEmpty")!.innerHTML = "The date of the paint can't be empty";
    } else
      document.getElementById("paintDateEmpty")!.innerHTML = ""

    if (!hasErrors && this.project) {
      let photo = "";
      if (this.photo)
        photo = this.photo[0];
      let proj = new Project(this.project?.id, make, model, year, licencePlate, country, chassisNo, engineNo, ownerEmail, this.project.startDate, this.project.endDate, this.project.isComplete, this.project.caseInstanceId, this.project.nextTaskName, photo, this.project.pinterestBoardUrl, pinterestUrl, paintRecordNumber, paintDesignation, paintTechnique, paintOriginalYear, paintDate);
      this.client.put(this.baseUrl + 'api/Projects/' + proj?.id, proj, Token.getHeader()).subscribe(result => {
        this.router.navigate(['/projects/details/' + proj?.id]);
      }, error => HandleError.handleError(error, this.router, this.authService));
    }
  }
}
