export class DiagramXML {
  constructor(
    public id: string,
    public bpmn20Xml: string
  ) { }
}

export class HistoryTasks {
  constructor(
    public currentActivityIds: string[],
    public historyActivityIds: string[],
    public historySequenceFlowIds: string[]
  ) { }
}