import {
  AfterContentInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  ViewChild,
  SimpleChanges,
  EventEmitter,
  ViewEncapsulation,
  Inject
} from '@angular/core';

import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { map, switchMap } from 'rxjs/operators';

/**
 * You may include a different variant of BpmnJS:
 *
 * bpmn-viewer  - displays BPMN diagrams without the ability
 *                to navigate them
 * bpmn-modeler - bootstraps a full-fledged BPMN editor
 */
// @ts-ignore
import * as BpmnJS from 'src/app/diagram/bpmn-navigated-viewer.development.js';

import { from, Observable, Subscriber, Subscription } from 'rxjs';
import { BasicNode } from '../basic-node';
import { DiagramNode } from '../diagram-node';
import { ExclusiveNode } from '../exclusive-node';
import { InclusiveNode } from '../inclusive-node';
import { ParallelNode } from '../parallel-node';
import { HistoryTasks } from '../diagram';
import { SubmittedNode } from '../submitted-node';
import { PredictedTasks, TasksToApprove, TaskToApprove, Task } from '../task';
import { SequenceFlowNode } from '../sequence-flow-node';
import { GatewayNode } from '../gateway-node';
import { ReceiveMessageNode } from '../receive-message-node';
import { SendMessageNode } from '../send-message-node';
import { ProcessNode } from '../process-node';
import { Token } from '../common/tokens';
import { HandleError } from '../common/error';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { DatePickerComponent } from '../date-picker/date-picker.component';

@Component({
  selector: 'app-diagram',
  templateUrl: './diagram.component.html',
  styleUrls: ['./diagram.component.css']
})
export class DiagramComponent implements AfterContentInit, OnChanges, OnDestroy {

  @ViewChild(DatePickerComponent, { static: false })
  private datePickerComponent!: DatePickerComponent;

  private bpmnJS: BpmnJS;
  @ViewChild('ref', { static: true }) private el: ElementRef|undefined = undefined;
  @Output() private importDone: EventEmitter<any> = new EventEmitter();
  @Output() private callDiagram: EventEmitter<any> = new EventEmitter();

  @Input() public processInstanceId: string = "";
  @Input() public caseInstanceId: string = "";

  private events = [
    'element.click'
  ];

  // the history of activity id's of the current diagram
  private taskHistoryIds: string[];
  //
  private sequenceFlowHistoryIds: string[];
  // the current task activity id available to click
  private currentTaskIds: string[];

  private predictedTasks: PredictedTasks[];

  // the node that represents the diagram
  private currentNode: ProcessNode| null = null;
  // nodes that can be selected
  private nodesEnableable: DiagramNode[];
  // nodes that can be unselected
  public nodesDisableable: DiagramNode[];

  private canvas: any;
  private elementRegistry: any;

  // the currently selected node on the 'date-picker', injected to the component with 
  // the @Inject parameter
  public selectedNode: BasicNode | null;
  public selectedTask: Task | null;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private authService: AuthService) {

    // Global variables init
    this.taskHistoryIds = [];
    this.sequenceFlowHistoryIds = [];
    this.currentTaskIds = [];

    this.predictedTasks = [];

    this.nodesEnableable = [];
    this.nodesDisableable = [];

    this.selectedNode = null;
    this.selectedTask = null;

    // bpmn.io variables
    this.bpmnJS = new BpmnJS();

    var eventBus = this.bpmnJS.get('eventBus');

    this.canvas = this.bpmnJS.get('canvas');
    this.elementRegistry = this.bpmnJS.get('elementRegistry');

    // Initialize the BPMN viewer and build the graph with current diagram information
    this.bpmnJS.on('import.done', ({ error }: any) => {
      if (!error) {
        this.bpmnJS.get('canvas').zoom('fit-viewport');
      }

      this.importDiagramHistory(this.canvas, this.elementRegistry);
    });

    this.bpmnJS.on('element.dblclick', async (e: any) => {
      if (e.element.type == "bpmn:CallActivity")
        this.http.get(this.baseUrl + 'api/Tasks/Called/' + this.processInstanceId + '/' + e.element.id, { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
          if (result != null)
            this.callDiagram.emit(result)
        }, error => HandleError.handleError(error, this.router, this.authService));
    });

    // open datetime picker if user right clicks on element
    this.bpmnJS.on('element.contextmenu', async (e: any) => {
      // prevent the default right click event
      e.originalEvent.preventDefault();
      e.originalEvent.stopPropagation();

      // check if the right click was on a disableable node
      var nodeForDisableFound: DiagramNode| undefined = this.nodesDisableable.find(n => n.id == e.element.id);

      if (e.element.type == "bpmn:CallActivity")
        this.http.get(this.baseUrl + 'api/Tasks/Called/' + this.processInstanceId + '/' + e.element.id, { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
          if (result != null)
            this.callDiagram.emit(result)
        }, error => HandleError.handleError(error, this.router, this.authService));
      else if (e.gfx.classList.contains("highlight-history") && e.element.type == "bpmn:UserTask") {
        var success;
        await this.http.get<Task>(this.baseUrl + 'api/Tasks/' + this.processInstanceId + "/" + e.element.id, Token.getHeader()).toPromise().then(result => {
          this.selectedTask = result;
          this.selectedNode = null;

          success = true;
        }).catch(error => success = false);
        if (success)
          return;
      }
      else if (nodeForDisableFound != undefined && nodeForDisableFound instanceof BasicNode ) {
        this.selectedNode = nodeForDisableFound;
        this.selectedTask = null;
        return;
      }
      this.selectedNode = null; // null so the 'date-picker' tab disappears
      this.selectedTask = null;
    });


    // interpret the click events
    this.events.forEach(event => {

      eventBus.on(event, (e: any) => {
        // e.element = the model element
        // e.gfx = the graphical element

        var nodeForEnableFound: DiagramNode| undefined = this.nodesEnableable.find(n => n.id == e.element.id);

        var nodeForDisableFound: DiagramNode| undefined = this.nodesDisableable.find(n => n.id == e.element.id);

        if (nodeForEnableFound != undefined) {

          if (!this.canvas.hasMarker(e.element.id, 'highlight') || !this.canvas.hasMarker(e.element.id, 'highlight-flow')) {
            
             // select the node and get the new array with nodes available to select
             nodeForEnableFound.enable();

            if (nodeForEnableFound instanceof SequenceFlowNode) 
              this.canvas.addMarker(e.element.id, 'highlight-flow');
            else {
              this.canvas.addMarker(e.element.id, 'highlight');
              // highlight the next SequenceFlow (that had been automatically enabled)
              this.canvas.addMarker(nodeForEnableFound.nextNode!.id, 'highlight-flow');
            }

            this.nodesEnableable = this.currentNode!.canEnable();

            this.nodesDisableable = this.currentNode!.canDisable();
          }
        }

        if (nodeForDisableFound != undefined) {

          if (this.canvas.hasMarker(e.element.id, 'highlight') || this.canvas.hasMarker(e.element.id, 'highlight-flow')) {
            if (nodeForEnableFound instanceof SequenceFlowNode) 
              this.canvas.removeMarker(e.element.id, 'highlight-flow');
            else
              this.canvas.removeMarker(e.element.id, 'highlight');
            
            // unselect the node and update the array with nodes available to select
            var nodesDisabled = nodeForDisableFound.disable();

            // trigger a function to cleanup colored nodes that have been removed
            this.disableColorCleanup(this.canvas, nodesDisabled);

            this.nodesEnableable = this.currentNode!.canEnable();

            this.nodesDisableable = this.currentNode!.canDisable();
          }
        }

        // add the cursor html change to the new enableable nodes
        if (nodeForEnableFound != undefined || nodeForDisableFound != undefined) {
          // filter the elements in the diagram to limit those who are clickable
          var tasksFound = this.elementRegistry.filter(function (el: any) {
            return (el.type == "bpmn:UserTask" || el.type == "bpmn:SendTask" || 
              el.type == "bpmn:ReceiveTask" || el.type == "bpmn:SequenceFlow")
          });
          // remove the 'pointer' property from all user tasks
          for (let i = 0; i < tasksFound.length; i++) {
            this.canvas.removeMarker(tasksFound[i].id, 'pointer');
            this.canvas.removeMarker(tasksFound[i].id, 'highlight-flow-hover');
          }

          // add the 'pointer' html property to the enableable nodes
          for (let i = 0; i < tasksFound.length; i++) {
            if (this.nodesEnableable.find(n => n.id == tasksFound[i].id) != undefined) {
              this.canvas.addMarker(tasksFound[i].id, 'pointer');
              this.canvas.addMarker(tasksFound[i].id, 'highlight-flow-hover');
            }
          }
          // add the 'pointer' html property to the disableable nodes
          for (let i = 0; i < tasksFound.length; i++) {
            if (this.nodesDisableable.find(n => n.id == tasksFound[i].id) != undefined) {
              this.canvas.addMarker(tasksFound[i].id, 'pointer');
              this.canvas.addMarker(tasksFound[i].id, 'highlight-flow-hover');
            }
          }
        }
      });
    });
  }

  ngAfterContentInit(): void {
    this.bpmnJS.attachTo(this.el!.nativeElement);
  }

  ngOnChanges(changes: SimpleChanges) {
    this.selectedNode = null;
    this.selectedTask = null;
    // re-import whenever the url changes
    if (changes.processInstanceId) {
      this.loadUrl(this.baseUrl + 'api/Projects/' + changes.processInstanceId.currentValue + '/Diagram');
    }
  }

  ngOnDestroy(): void {
    this.bpmnJS.destroy();
  }

  /**
   * Method called by the clicking of the button 'Submit tasks' in the parent component.
   * This method sends an object containing all the clicked diagram tasks to the server for them
   * to be approved in the Camunda Workflow Engine.
   */
  async submitTasks(projectId: string): Promise<boolean> {
    
    if (this.currentNode == null) return false;
    if (!this.currentNode.canBeValidated()) {
      alert("Please select a task inside the decision or remove the last selected task.");
      return false;
    }

    if (this.datePickerComponent?.error) {
      alert("Please make sure that the completion dates are after the start ones.");
      return false;
    }


    var nodesSelected: BasicNode[] = this.currentNode.getNodesForSubmission();

    var variablesToSend: Map<string, string> = this.currentNode.getVariables();

    var startEventTriggers: string[] = this.currentNode.getStartEventTriggers();

    // if the list to submit is empty do nothing
    if (nodesSelected.length == 0) return false;

    var tasks: Array<TaskToApprove> = new Array<TaskToApprove>();
    //var tasks: Map<string, string> = new Map<string, string>();
    //var activityIds: string[] = [];
    nodesSelected.forEach(node => {
      //tasks.set(node.id, node.completionTime.toISOString() );
      let task: TaskToApprove = new TaskToApprove(node.id, node.startTime!.toISOString(), node.completionTime!.toISOString(),
        node instanceof ReceiveMessageNode ? node.getMessageRefForSubmission() : "", node.commentReport, node.commentExtra, node.media, node.extraMedia);
      tasks.push(task);
    });
     
    var variablesArray = Array.from(variablesToSend.entries());
    //var tasksArray = Array.from(tasks.entries());

    var tasksToApprove: TasksToApprove = new TasksToApprove(tasks, variablesArray, startEventTriggers, this.processInstanceId);

    var success = false;
    // send the tasks for approval to the server
    await this.http.post(this.baseUrl + 'api/Tasks/' + projectId + '/Approve', tasksToApprove, Token.getHeader()).toPromise().then(result => {
      alert("Tasks approved successfully.");
      this.importDiagramHistory(this.canvas, this.elementRegistry);

      this.selectedNode = null;
      this.selectedTask = null;

      success = true;
    }).catch(error => { 
      alert("An error has occured with the task submission. Please refresh the page and try again.");
      console.error(error);
    });
    
    return success;
  }

  /**
   * Load diagram from URL and emit completion event
   */
  loadUrl(url: string): Subscription {

    return (
      this.http.get(url, { headers: Token.getHeader().headers, responseType: 'text' }).pipe(
        switchMap((xml: string) => this.importDiagram(xml)),
        map(result => result.warnings),
      ).subscribe(
        (warnings) => {
          this.importDone.emit({
            type: 'success',
            warnings
          });
        },
        (err) => {
          this.importDone.emit({
            type: 'error',
            error: err
          });
        }
      )
    );
  }

  /**
   * Auxiliar method to be called in a callback in the 'date-picker' child component.
   * This method is used only to close the 'date-picker' tab by clicking the 'x' button on the UI.
   */
  closeButton() {
    this.selectedNode = null;
    this.selectedTask = null;
  }

  /**
   * Creates a Promise to import the given XML into the current
   * BpmnJS instance, then returns it as an Observable.
   *
   * @see https://github.com/bpmn-io/bpmn-js-callbacks-to-promises#importxml
   */
  private importDiagram(xml: string): Observable<{ warnings: Array<any> }> {
    return from(this.bpmnJS.importXML(xml) as Promise<{ warnings: Array<any> }>);
  }

  /**
   * Auxiliary method to retrieve the node type of the parent of the requested node
   * 
   * @param node the unparsed node
   * @returns the string with the node type
   */
  private getParentNodeType(node: any): any {
    var nodeParent: string = node.parent;

    if (parent == undefined) {
      parent = node.businessObject.$parent;
    }

    return this.getNodeType(nodeParent);
  }

  /**
   * Auxiliary method that receives every found 'bpmn:StartEvent' nodes in the diagram and returns the main start event,
   * which is the one where the process starts.
   *  
   * @param startNodes an array of start nodes 
   * @returns the main start event
   */
  private getMainStartEventNode(startNodes: any[]): any {
    var startNode: any = null;

    startNodes.forEach(n => {
      if (this.getParentNodeType(n) == "bpmn:Process" || this.getParentNodeType(n) == "bpmn:Participant" ) 
        startNode = n;
    });

    return startNode;
  }

  /**
   * Auxiliary method to retrieve the Signal Event Name that triggers the Conditional Start Event execution.
   * 
   * @param node the unparsed node
   * @returns a string containing the Signal Event Name if the node is a Conditional Start Event, empty string otherwise
   */
  private getStartEventSignalRef(node: any): string {
    // get the 'eventDefinitions' child of the node
    var eventDefinitions: any = node.eventDefinitions;

    if (eventDefinitions == undefined) 
      eventDefinitions = node.businessObject.eventDefinitions;
    
    // if the node has no 'eventDefinitions' object, it isn't a Conditional Start Event and return empty string
    if (eventDefinitions == undefined)
      return "";

    return eventDefinitions[0].signalRef.name;
  }

  /**
   * Auxiliary method that receives an array containing all the Start Events and returns the Conditional Start Event's with the 
   * respective Signal Event Name's.
   * 
   * @param foundStartEvents an array with all the unparsed Start Event nodes
   * @returns two arrays: the first one containing the Conditional Start Nodes found; and the second containing the respective 
   * Signal Event Name's.
   */
  private getConditionalStartEventNodes(foundStartEvents: any[]): [any[], string[]] {
    var conditionalStartingNodes: any[] = [];
    var conditionalSignalNames: string[] = [];

    foundStartEvents.forEach(n => {
      var signalRefName: string = this.getStartEventSignalRef(n);

      if (signalRefName != "") {
        conditionalStartingNodes.push(n);
        conditionalSignalNames.push(signalRefName);
      }
    });

    return [conditionalStartingNodes, conditionalSignalNames];
  }

  /**
   * Method used to retrieve the diagram history for the current project and current diagram from Camunda Workflow Engine, and build 
   * the graph accordingly. Additionally, it colors the tasks which have already been submitted for better user understanding.
   * 
   * @param canvas the canvas of the bpmnjs viewer
   * @param elementRegistry the registry containing all the nodes, and enables their access
   */
  private importDiagramHistory(canvas: any, elementRegistry: any) {
    // get the tasks completed in the current diagram
    this.http.get<HistoryTasks>(this.baseUrl + 'api/Tasks/' + this.processInstanceId + '/Diagram/History', Token.getHeader()).subscribe(result => {

      this.taskHistoryIds = result.historyActivityIds;

      // remove the current task to be approved from the list and save it
      this.currentTaskIds = result.currentActivityIds;

      this.sequenceFlowHistoryIds = result.historySequenceFlowIds;

    }, error => HandleError.handleError(error, this.router, this.authService)
      , () => { // on complete this path is activated

        // get the tasks completed in the current diagram
        this.http.get<PredictedTasks[]>(this.baseUrl + 'api/Tasks/' + this.caseInstanceId + '/Diagram/Predictions', Token.getHeader()).subscribe(result => {
          this.predictedTasks = result;
        } , error => console.error(error)
        , () => {
          // get the start event of the diagram
          var foundStartEvents: any[] = elementRegistry.filter((el: any) => el.type == "bpmn:StartEvent");
          // get the main start event of the process
          var foundStart = this.getMainStartEventNode(foundStartEvents);
 
          var conditionalStartingNodes = this.getConditionalStartEventNodes(foundStartEvents);

          // parse the diagram
          this.currentNode = this.parseProcess(foundStart, conditionalStartingNodes[0], conditionalStartingNodes[1]);

          var nodesAbleToSelect: Array<DiagramNode> = this.currentNode.canEnable();
          nodesAbleToSelect.forEach(n => this.currentTaskIds.push(n.id));

          this.nodesEnableable = this.currentNode.canEnable();
          this.nodesDisableable = [];

          // filter the elements in the diagram to limit those who are clickable
          var tasksFound = elementRegistry.filter(function (el: any) {
            return (el.type == "bpmn:UserTask" || el.type == "bpmn:SendTask" || el.type == "bpmn:ReceiveTask")
          });
          // add the 'pointer' html property to the enableable nodes
          for (let i = 0; i < tasksFound.length; i++) {
            if (this.nodesEnableable.find(n => n.id == tasksFound[i].id) != undefined) 
              canvas.addMarker(tasksFound[i].id, 'pointer');
          }
        });
      });
  }

  /**
   * Method to build the graph recursively through the call of the parsing of the root (or first) node 
   * in the graph.
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @param isMainStartEvent boolean that identifies that the main start event of the diagram is being parsed
   * @returns the built 'node' called with the entire graph built
   */
  private parseNode(node: any, stoppingNode: any = null, isMainStartEvent: boolean = false, conditionalStartingNodes: any[] = []): DiagramNode| null {
    if (stoppingNode != null && node.id == stoppingNode.id) return null; 

    var nodeType: string = this.getNodeType(node);

    switch (nodeType) {
      case "bpmn:StartEvent":
        return this.parseNode(node.outgoing[0], stoppingNode);
      case "bpmn:SubProcess":
        return this.parseSubProcess(node, stoppingNode);
      case "bpmn:UserTask":
        return this.parseBasicTask(node, stoppingNode);
      case "bpmn:SendTask":
        return this.parseBasicTask(node, stoppingNode, true);
      case "bpmn:ReceiveTask":
        return this.parseBasicTask(node, stoppingNode, false, true);
      case "bpmn:ExclusiveGateway":
        return this.parseGateway(node, stoppingNode, "exclusive");
      case "bpmn:InclusiveGateway":
        return this.parseGateway(node, stoppingNode, "inclusive");
      case "bpmn:ParallelGateway":
        return this.parseGateway(node, stoppingNode, "parallel");
      case "bpmn:SequenceFlow":
        return this.parseSequenceFlow(node, stoppingNode);
      case "bpmn:ManualTask":
      case "bpmn:BusinessRuleTask":
      case "bpmn:CallActivity":
        return this.parseServerRequiredTask(node, stoppingNode);
      case "bpmn:EndEvent":
        return null;
      default:
        console.log("Node type not found.");
        return null;
    }
  }

  /**
   * Auxiliary method to retrive the next node in the case of a sequence flow. This is needed due to the 
   * existance of a 'businessObject' property in the outer nodes that is seemingly inexistant in the inner 
   * nodes, in which case to retrieve the next node the property 'targetRef' is needed to be called.
   * 
   * @param sequenceFlowNode the unparsed SequenceFlow
   * @returns the seuq
   */
  private getSequenceFlowOutgoing(sequenceFlowNode: any): any {
    if (sequenceFlowNode.businessObject != undefined) {
      return sequenceFlowNode.businessObject.targetRef;
    }
    return sequenceFlowNode.targetRef;
  }

  /**
   * Auxiliary method to retrieve the node type of a requested node.
   * 
   * @param node the unparsed node
   * @returns the string with the node type
   */
  private getNodeType(node: any): string {
    var nodeType: string = node.type;
    if (nodeType == undefined) {
      nodeType = node.$type;
    }

    return nodeType;
  }

  /**
   * Method to build a node of type ProcessNode which contains the entire process definition for the entire diagram.
   * 
   * @param startEventNode the unparsed main event node where the diagram starts
   * @param conditionalStartingEventNodes an array of the remaining unparsed secondary event nodes
   * @returns the parsed node as a ProcessNode
   */
  private parseProcess(startEventNode: any, conditionalStartingEventNodes: any[], 
    conditionalSignalNames: string[] ): ProcessNode {
    var nextNode: DiagramNode| null = null;
    var startNode: DiagramNode| null = this.parseNode(startEventNode);

    var conditionalStartingNodes: DiagramNode[] = [];
    conditionalStartingEventNodes.forEach(n => {
      // parse each conditional subprocess
      var parsedSequenceFlow: SequenceFlowNode = this.parseSequenceFlow(n.outgoing[0]);
      // enable the SequenceFlowNode which starts the subprocess to enable the click on the first UserTask
      parsedSequenceFlow.submitted = true;
      this.colourHistoryNode("flow", parsedSequenceFlow.id);
      conditionalStartingNodes.push(parsedSequenceFlow);
    });

    if (startNode == null) {
      throw new Error("Starting node is null");
    }

    return new ProcessNode(nextNode, false, "", startNode, conditionalStartingNodes, conditionalSignalNames);
  }

  /**
   * Auxiliary method to extract the start event node from a subProcess node.
   * 
   * @param subProcessNode the unparsed subProcess node
   * @returns the unparsed start event
   */
  private getSubProcessStart(subProcessNode: any): any {
    var children: any[] = subProcessNode.children;
    var foundStart: any = null;

    if (children == undefined) 
      children = subProcessNode.flowElements;

    for(let node of children) {
      if (this.getNodeType(node) == "bpmn:StartEvent") { 
        foundStart = node;
        break;
      }
    }

    return foundStart;
  }

  /**
   * Method to build a node of type ProcessNode (which in this case correlates to a node of type 'SubProcess' in 
   * BPMN), which encapsules the entire collection of nodes between the start and end event of the 'SubProcess' 
   * (including possible ProcessNode's).
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @returns the parsed node as a ProcessNode
   */
  private parseSubProcess(node: any, stoppingNode: any = null): DiagramNode {
    var unparsedStartNode: any = this.getSubProcessStart(node);

    var startNode: DiagramNode| null = this.parseNode(unparsedStartNode);
    var nextNode: DiagramNode| null = this.parseNode(node.outgoing[0], stoppingNode);

    if (startNode != null && ProcessNode.inferSubProcessInstance(startNode, nextNode) ) 
      return new ProcessNode(nextNode, false, node.id, startNode);

    return new SubmittedNode(nextNode, node.id);
  }

  /**
   * Auxiliary method to retrive the messageRef in the case of a 'bpmn:ReceiveTask'. This is needed due to the 
   * existance of a 'businessObject' property in the outer nodes that is seemingly inexistant in the inner 
   * nodes, in which case to retrieve the next node the property 'messageRef' is needed to be called.
   * 
   * @param node the unparsed node
   * @returns the string with the message referenced
   */
  private getMessageRef(node: any): string {
    if (node.businessObject != undefined) {
      return node.businessObject.messageRef.name;
    }
    return node.messageRef.name;
  }

  /**
   * Method to build a node of type BasicNode (which correlates to nodes of type 'UserTask', 'SendTask', 
   * and 'ReceiveTask' in BPMN). This are the types of Tasks that are 'clickable' in the user interface.
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @param isSendTask a boolean to distinguish a 'SendTask' parse
   * @param isReceiveTask a boolean to distinguish a 'ReceiveTask' parse
   * @returns the parsed node as a BasicNode (subtype of DiagramNode), ReceiveMessageNode  (subtype of 
   * BasicNode), or SendMessageNode (subtype of BasicNode)
   */
  private parseBasicTask(node: any, stoppingNode: any = null, isSendTask: boolean = false, 
    isReceiveTask: boolean = false): DiagramNode {
    var nextNode: DiagramNode| null = null;

    // if a 'bpmn:SendTask' is being processed, make sure to follow the 'bpmn:SequenceFlow' 
    // instead of the 'bpmn:MessageFlow'
    if (isSendTask) {
      if (this.getNodeType(node.outgoing[0]) == "bpmn:SequenceFlow") 
        nextNode = this.parseNode(node.outgoing[0], stoppingNode);
      else
        nextNode = this.parseNode(node.outgoing[1], stoppingNode);
      
    } else
      nextNode = this.parseNode(node.outgoing[0], stoppingNode);

    // if node is in historyNodes then return new SubmittedNode
    if (this.taskHistoryIds.indexOf(node.id) != -1) {
      
      // if the node is submitted the following sequence flow node should also be
      if (nextNode instanceof SequenceFlowNode)
        nextNode.submitted = true; 

      // color the affected nodes by making a submitted node
      if (nextNode != null)
        this.colourHistoryNode("basic", node.id, nextNode.id);
      return new SubmittedNode(nextNode, node.id);
    
    } 

    // see if it is one of the predicted tasks
    var prediction: PredictedTasks| undefined = this.predictedTasks.find(t => t.activityId == node.id);
    if (prediction != undefined) {
      var currentNode: BasicNode = new BasicNode(nextNode, false, node.id);
      currentNode.startTime = new Date(prediction.startTime);
      currentNode.completionTime = new Date(prediction.endTime);

      this.colourHistoryNode("prediction", node.id);

      return currentNode;
    }


    let elements = node.extensionElements;
    if (elements)
        for (let i = 0; i < elements.values.length; i++)
          if (elements.values[i].$type == "camunda:properties") {
            let properties = elements.values[i].$children
            for (let n = 0; n < properties.length; n++)
              if (properties[n].name == "Evidence" && properties[n].value == "True") {
                this.colourHistoryNode("evidence", node.id);
                break;
              }
            break;
          }

    if (isReceiveTask) 
      return new ReceiveMessageNode(nextNode, false, node.id, this.getMessageRef(node));
    else if (isSendTask)
      return new SendMessageNode(nextNode, false, node.id);
    else
      return new BasicNode(nextNode, false, node.id);
  }

  /**
   * Auxiliary method to retrieve the object containing the joining gateway of the requested type.
   * 
   * @param beginningGatewayNode
   * @param gatewayType
   */
  private getLastGateway(beginningGatewayNode: any, gatewayType: string): any {
    // check if the beginningGatewayNode is the ending node of a gateway
    if (beginningGatewayNode.incoming.length > 1 && beginningGatewayNode.outgoing.length == 1) {
      return beginningGatewayNode;
    }

    var node = beginningGatewayNode.outgoing[0];
    var nodeDepth: number = 1;

    while (nodeDepth > 0) {
      var nodeType: string = this.getNodeType(node);

      if (nodeType == gatewayType) {
        if (node.outgoing.length == 1) {
          nodeDepth--;
        } else {
          nodeDepth++;
        }
        // if the stopping condition is reached, don't override current found node
        if (nodeDepth != 0) node = node.outgoing[0];

      } else if (nodeType == "bpmn:SequenceFlow") {
        node = this.getSequenceFlowOutgoing(node);
      } else if (nodeType == "bpmn:EndEvent") { // create a imaginary end-gateway to build the gateway node
        return beginningGatewayNode;
      } else {
        node = node.outgoing[0];
      }
    }

    return node;
  }

  /**
   * Method to build a node of type GatewayNode, which encapsules the entire collection of nodes between 
   * the opening and the respective closing gateway (including possible GatewayNode's).
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @param gatewayType the type of gateway to create ("exclusive", "inclusive", and "parallel")
   * @returns the parsed node as a GatewayNode (subtype of DiagramNode)
   */
  private parseGateway(node: any, stoppingNode: any = null, gatewayType: string): DiagramNode| null {
    var endGateway = this.getLastGateway(node, this.getNodeType(node));

    var branches: Array<SequenceFlowNode> = new Array<SequenceFlowNode>();
    var nextNode: DiagramNode| null;

    if (node.id == endGateway.id) {
      node.outgoing.forEach((obj: any) => branches.push(this.parseSequenceFlow(obj) ) );

      nextNode = null;
    } else {
      node.outgoing.forEach((obj: any) => branches.push(this.parseSequenceFlow(obj, endGateway) ) );

      nextNode = this.parseNode(endGateway.outgoing[0], stoppingNode);
    }
    

    switch (gatewayType) {
      case "exclusive":
        if (ExclusiveNode.inferGatewayInstance(nextNode, branches)) {
          return new ExclusiveNode(nextNode, false, branches, node.id);
        } else {
          // if the node is submitted the following sequence flow node should also be
          if (nextNode instanceof SequenceFlowNode)
            nextNode.submitted = true;
          // color the affected nodes by making a submitted node
          if (nextNode != null)
            this.colourHistoryNode("gateway", node.id, nextNode.id);
          return new SubmittedNode(nextNode, node.id);
        }

        break;
      case "inclusive":
        if (InclusiveNode.inferGatewayInstance(nextNode, branches, this.currentTaskIds)) {
          return new InclusiveNode(nextNode, false, branches, node.id);
        } else {
          // if the node is submitted the following sequence flow node should also be
          if (nextNode instanceof SequenceFlowNode)
            nextNode.submitted = true;
          // color the affected nodes by making a submitted node
          if (nextNode != null)
            this.colourHistoryNode("gateway", node.id, nextNode.id);
          return new SubmittedNode(nextNode, node.id);
        }

        break;
      case "parallel":
        if (ParallelNode.inferGatewayInstance(nextNode, branches)) {
          return new ParallelNode(nextNode, false, branches, node.id);
        } else {
          // if the node is submitted the following sequence flow node should also be
          if (nextNode instanceof SequenceFlowNode)
            nextNode.submitted = true;
          // color the affected nodes by making a submitted node
          if (nextNode != null)
            this.colourHistoryNode("gateway", node.id, nextNode.id);
          return new SubmittedNode(nextNode, node.id);
        }

        break;
      default:
        console.log("Gateway type not found.");
        return null;
        break;
    }

  }

  /**
   * Method to build a node of type SequenceFlowNode (which correlates to a 'SequenceFlow' in BPMN)
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @returns the parsed node as a SequenceFlowNode (subtype of DiagramNode)
   */
  private parseSequenceFlow(node: any, stoppingNode: any = null): SequenceFlowNode {
    var nextObj: any = this.getSequenceFlowOutgoing(node);
    var nextNode: DiagramNode| null = this.parseNode(nextObj, stoppingNode);

    var builtNode: SequenceFlowNode;// = new SequenceFlowNode(nextNode, false, node.id, nextObj.id);

    if (nextNode != null) {

      builtNode = new SequenceFlowNode(nextNode, false, node.id, nextObj.id);

      if (this.sequenceFlowHistoryIds.indexOf(node.id) != -1) {
        builtNode.submitted = true;
        this.colourHistoryNode("flow", builtNode.id);
      
      // if the sequence flow is either before a submitted node or before a current node,
      // consider it submitted
      } else if (nextNode.isSubmitted() || this.currentTaskIds.indexOf(nextNode.id) != -1) {
        builtNode.submitted = true;
        this.colourHistoryNode("flow", builtNode.id);
      
      } else if (nextNode instanceof GatewayNode) {
        this.currentTaskIds.forEach(id => {
      
          if (nextNode!.hasActivityId(id)) { 
            builtNode.submitted = true;
            this.colourHistoryNode("flow", builtNode.id);
          }
        });
      }

    } else {

      if (stoppingNode != null && nextObj.id == stoppingNode.id) {
        builtNode = new SequenceFlowNode(nextNode, false, node.id, nextObj.id);

        if (this.sequenceFlowHistoryIds.indexOf(node.id) != -1) {
          builtNode.submitted = true;
          this.colourHistoryNode("flow", builtNode.id);
        
        }

      } else {
        builtNode = new SequenceFlowNode(nextNode, false, node.id, nextObj.id);
      }

    }

    return builtNode;
  }

  /**
   * Method to build a special type of node of type SubmittedNode or 'null'. This type of node has some 
   * requirements that can only be satisfied by the server. Therefore, this node should be interpreted as 
   * either a stopping point where the user must submit everything immediatly before it to be able to 
   * proceed, or simply part of the history of the diagram. This method is called for ManualTask's, 
   * BusinessRuleTask's, or CallActivity's.
   * 
   * @param node the unparsed node
   * @param stoppingNode the node that serves as criteria to stop the parsing
   * @returns the parsed node as a SubmittedNode (subtype of DiagramNode), or null if it hasn't occurred yet
   */
  private parseServerRequiredTask(node: any, stoppingNode: any = null): DiagramNode| null {
    var nextNode: DiagramNode| null = this.parseNode(node.outgoing[0], stoppingNode);
    // if node is in historyNodes then return new SubmittedNode
    if (this.taskHistoryIds.indexOf(node.id) != -1 && nextNode != null) {
      this.colourHistoryNode("basic", node.id, nextNode.id);
      return new SubmittedNode(nextNode, node.id);
    }
    return null;
  }

  /**
   * Method to remove the coloring of the tasks that have been unselected by an action on previous 
   * nodes (i.e. unselecting a node before another should automatically unselect the next one, if it was selected)
   * 
   * @param canvas 
   * @param nodesToUncolor
   */
  private disableColorCleanup(canvas: any, nodesToUncolor: DiagramNode[]): void {
    nodesToUncolor.forEach(n => {
      canvas.removeMarker(n.id, 'highlight');
      canvas.removeMarker(n.id, 'highlight-flow');
    });
  }

  /**
   * Method to color the submitted nodes and their surrounding affected nodes (SequenceFlowNode's).
   * 
   * @param nodeType the type of node to color ("flow", "basic", or "gateway")
   * @param nodeId the id of the node to color
   * @param nextNodeId the id of the next node (can be ommited in case of being null, after SequenceFlowNode)
   */
  private colourHistoryNode(nodeType: string, nodeId: string, nextNodeId: string = "") {
    switch (nodeType) {
      case "flow": // color only itself
        if (!this.canvas.hasMarker(nodeId, 'highlight-flow-history') )
          this.canvas.addMarker(nodeId, 'highlight-flow-history');

        break;
      case "basic": // color itself and the following sequence flow
        if (!this.canvas.hasMarker(nodeId, 'highlight-history') )
          this.canvas.addMarker(nodeId, 'highlight-history');

        if (!this.canvas.hasMarker(nextNodeId, 'highlight-flow-history') )
          this.canvas.addMarker(nextNodeId, 'highlight-flow-history');

        break;
      case "gateway": // color only the following sequence flow
        if (!this.canvas.hasMarker(nextNodeId, 'highlight-flow-history') )
          this.canvas.addMarker(nextNodeId, 'highlight-flow-history');
        
        break;
      case "prediction":
        if (!this.canvas.hasMarker(nodeId, 'highlight-prediction') )
          this.canvas.addMarker(nodeId, 'highlight-prediction');
        break;
      case "evidence":
        if (!this.canvas.hasMarker(nodeId, 'highlight-evidence') )
          this.canvas.addMarker(nodeId, 'highlight-evidence');
        break;
      default:
        console.log("Node type not found.");
        break;
    }
  }

}
