import { DiagramNode } from "./diagram-node";
import { GatewayNode } from "./gateway-node";

export class BasicNode extends DiagramNode {
  // the date and time defined in which the task started
  public startTime: Date| null;
  // the date and time defined in which the task was completed
  public completionTime: Date | null;

  public media: string[];
  public extraMedia: string[];
  public commentReport = "";
  public commentExtra = "";

  constructor(nextNode: DiagramNode| null, greenLight: boolean, activityId: string) {
    super(nextNode, activityId, greenLight);
    this.startTime = null;
    this.completionTime = null;
    this.media = [];
    this.extraMedia = [];
    this.startTime = new Date();
    this.completionTime = new Date();
  }

  public canEnable(): DiagramNode[] {
    if (this.greenLight == false)
      return [this];
    if (this.nextNode != null)
      return this.nextNode.canEnable();

    return new Array<DiagramNode>();
  }

  public canDisable(): DiagramNode[] {
    var canDisable: Array<DiagramNode> = new Array<DiagramNode>();
    if (this.greenLight == true)
      canDisable.push(this);
      if (this.nextNode != null)
        canDisable = canDisable.concat(this.nextNode.canDisable());

    return canDisable;
  }

  public canBeValidated(): boolean {
    if (this.greenLight == true && this.nextNode != null)
      return this.nextNode.canBeValidated();

    return true;
  }

  /**
   * Method to retrieve all the BasicNodes that were selected in the user interface to be submitted.
   * 
   * @returns an array of BasicNode's to be submitted
   */
  public getNodesForSubmission(): BasicNode[] {
    var nodesToSubmit: Array<BasicNode> = new Array<BasicNode>();
    if (this.greenLight == true)
      nodesToSubmit.push(this);
      if (this.nextNode != null)
        nodesToSubmit = nodesToSubmit.concat(this.nextNode.getNodesForSubmission());

    return nodesToSubmit;
  }

  public enable(): void {
    this.greenLight = true;
    this.fillTimeVariables();
    // automatically enable the next sequence flow
    this.nextNode?.enable();
  }

  /**
   * Auxiliary method to fill automatically the fields of 'startTime' and 'completionTime' based 
   * on the previous node times. If the previous nodes don't have times filled, use the current
   * Date and Time.
   */
  private fillTimeVariables(): void {
    if (this.startTime == null) {
      let previousCompletionTime: Date = this.getPreviousCompletionTime();

      if (previousCompletionTime == null) 
        this.startTime = new Date();
      
      else {
        this.startTime = new Date(previousCompletionTime.getTime());
        this.startTime.setMinutes(this.startTime.getMinutes() + 1);
      }
    }

    if (this.completionTime == null) {
      this.completionTime = new Date(this.startTime.getTime() );
      this.completionTime.setMinutes(this.completionTime.getMinutes() + 1);
    }
  }

  public disable(): Array<DiagramNode> {
    var nodesDisabled: Array<DiagramNode> = new Array<DiagramNode>();

    if (this.greenLight == true) {
      nodesDisabled = [this];
      this.greenLight = false;
    }

    if (this.nextNode != null) {
      nodesDisabled = nodesDisabled.concat(this.nextNode.disable());
    }
    else {
      var currentParentGatewayNode: GatewayNode| null = this.parentGatewayNode;
      // disable the next nodes if the disabling of the current node has changed the green light
      // of the parent gateway node
      while (currentParentGatewayNode != null && !currentParentGatewayNode.getGreenLight()) {
        if (currentParentGatewayNode.nextNode != null)
          nodesDisabled = nodesDisabled.concat(currentParentGatewayNode.nextNode.disable());

        currentParentGatewayNode = currentParentGatewayNode.parentGatewayNode;
      }
    }

    return nodesDisabled;
  }

  public clone(): BasicNode {
    if (this.nextNode == null)
      return new BasicNode(null, this.greenLight, this.id);

    var nextNodeClone: DiagramNode = this.nextNode.clone();
    return new BasicNode(nextNodeClone, this.greenLight, this.id);
  }

  public getGreenLight(): boolean {
    return this.greenLight;
  }

  public isSubmitted(): boolean {
    return false;
  }

  public getVariables(): Map<string, string> {
    if (this.greenLight == true && this.nextNode != null)
      return this.nextNode.getVariables();

    return new Map<string, string>();
  }

  public hasActivityId(activityId: string): boolean {
    return activityId == this.id;
  }

  public getPreviousCompletionTime(): Date {
    if (this.previousNode != null) 
      return this.previousNode.getPreviousCompletionTime();
    
    return new Date();
  }

}
