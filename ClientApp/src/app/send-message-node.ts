import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";

export class SendMessageNode extends BasicNode {

  constructor(nextNode: DiagramNode| null, greenLight: boolean, activityId: string) {
    super(nextNode, greenLight, activityId);
  }

  public canBeValidated(): boolean {
    if (this.greenLight == true && this.nextNode != null) 
        return this.nextNode.canBeValidated();
    // similarly to a GatewayNode, the previous 'BasicNode' can't be selected without the following 
    // 'MessageNode' being selected because of the needed variables
    else 
      return false;
  }

  public getNodesForSubmission(): BasicNode[] {
    var nodesToSubmit: Array<BasicNode> = new Array<BasicNode>();
    if (this.greenLight == true && this.nextNode != null)
      nodesToSubmit = nodesToSubmit.concat(this.nextNode.getNodesForSubmission());

    return nodesToSubmit;
  }

  public getVariables(): Map<string, string> {
    var variables: Map<string, string> = new Map<string, string>();
    // if this node is selected add the path variables needed for approving a 'ReceivingTask' in 
    // Camunda Workflow Engine
    if (this.greenLight && this.previousNode != null)
      variables.set(this.previousNode.id, this.id);

    if (this.nextNode != null && this.getGreenLight())
      this.nextNode.getVariables().forEach((v, k) => variables.set(k, v));
    
    return variables;
  }
}