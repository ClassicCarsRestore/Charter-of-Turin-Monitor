import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";

export class ReceiveMessageNode extends BasicNode {
  // the message reference used to trigger the event 'ReceiveMessage' in the bpmn
  public messageRef: string;

  constructor(nextNode: DiagramNode| null, greenLight: boolean, activityId: string, messageRef: string) {
    super(nextNode, greenLight, activityId);
    this.messageRef = messageRef;
  }

  /*
  public canBeValidated(): boolean {
    if (this.greenLight == true && this.nextNode != null) 
        return this.nextNode.canBeValidated();
    // similarly to a GatewayNode, the previous 'BasicNode' can't be selected without the following 
    // 'MessageNode' being selected because of the needed variables
    else 
      return false;
  }
  */

  /**
   * Method that returns the 'messageRef' associated with a BasicNode. In case of a BasicNode made 
   * from a 'bpmn:UserTask' or 'bpmn:SendTask', it is returned an empty string as it isn't needed
   * for Task approval in the Camunda Workflow Engine; inversly, in the case of a BasicNode made
   * from a 'bpmn:ReceiveTask' we need the 'Message Referenced' to trigger the diagram progression
   * 
   * @returns the value of 'messageRef' or ""
   */
  public getMessageRefForSubmission(): string {
    return this.messageRef;
  }
}