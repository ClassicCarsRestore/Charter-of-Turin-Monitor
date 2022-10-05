export class Project {
  constructor(
    public id: string,
    public make: string,
    public model: string,
    public year: number,
    public licencePlate: string,
    public country: string,
    public chassisNo: string,
    public engineNo: string,
    public ownerEmail: string,
    public startDate: string,
    public endDate: string,
    public isComplete: boolean,
    public caseInstanceId: string,
    public nextTaskName: string[],
    public photo: string,
    public pinterestBoardUrl: string,
    public pinterestBoardAccessUrl: string
  ) { }
}


export class ProjectForm {
  constructor(
    public make: string,
    public model: string,
    public year: number,
    public licencePlate: string,
    public country: string,
    public chassisNo: string,
    public engineNo: string,
    public ownerEmail: string,
    public clientExpectation: string,
    public photo: string,
    public originalMaterials: boolean,
    public carDocuments: boolean,
    public startDate: string
  ) { }
}

