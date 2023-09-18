export class BCTask {
    constructor(
        public assignee: string,
        public created: string,
        public executionId: string,
        public id: string,
        public name: string,
        public parentTaskId: string,
        public processDefinitionId: string,
        public processInstanceId: string,
        public suspended: boolean,
        public taskDefinitionKey: string,
    ) {}
}

export class TaskBCGet {
    constructor(
        public id: string,
        public title: string,
        public description: string,
        public photosIds: string[],
        public madeBy: string,
        public when: string,
    ) {}
}

export class BcClassicForm {
    constructor(
        public make: string,
        public model: string,
        public year: number,
        public licencePlate: string,
        public country: string,
        public chassisNo: string,
        public engineNo: string,
        public ownerEmail: string,
    ) {}
}