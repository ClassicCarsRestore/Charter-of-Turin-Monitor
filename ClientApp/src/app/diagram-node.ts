import { BasicNode } from "./basic-node";
import { GatewayNode } from "./gateway-node";

export abstract class DiagramNode {
  // identifies if a node is validly selected
  protected greenLight: boolean;
  // next node connected
  public nextNode: DiagramNode| null;
  // previous node connected
  public previousNode: DiagramNode| null;
  // the id that identifies the node (activityId, gatewayId, or sequenceFlowId)
  public id: string;
  // identifies the parent of the current node (if it exists)
  public parentGatewayNode: GatewayNode| null;

  constructor(nextNode: DiagramNode| null, id: string, greenLight: boolean) {
    this.nextNode = nextNode;
    this.previousNode = null;

    if (this.nextNode)
      this.nextNode.previousNode = this;
    
    this.id = id;
    this.greenLight = greenLight;
    this.parentGatewayNode = null;
  }

  public setParentGatewayNode(gatewayNode: GatewayNode): void {
    this.parentGatewayNode = gatewayNode;
  }

  /**
   * Method that returns an array containing all the nodes that can be enabled (or clicked) in the graph.
   * 
   * @returns an array of enableable BasicNode's
   */
  public abstract canEnable(): Array<DiagramNode>;

  /**
   * Method that returns an array containing all the nodes that were previously enabled in the session (except 
   * the nodes already marked and submitted), making it possible to disable them before submission. 
   * 
   * @returns an array of disableable BasicNode's
   */
  public abstract canDisable(): Array<DiagramNode>;

  /**
   * Method that evaluates the selection based on it's validity to submission in the Camunda Workflow Engine.
   * 
   * @returns a boolean that evaluates if the selection is valid for submission
   */
  public abstract canBeValidated(): boolean;

  /**
   * Method that returns an array containing all the basic nodes that were previously enabled in the session (except 
   * the nodes already marked and submitted), to send to be approved by Camunda. 
   * 
   * @returns an array of submittable BasicNode's
   */
  public abstract getNodesForSubmission(): Array<BasicNode>;

  /**
   * Method that activates (greenLight = true) a node of type BasicNode or SequenceFlowNode. In other nodes the method 
   * isn't supposed to be called returning the "Method not implemented." error.
   * 
   * @throws "Method not implemented.", in case of a GatewayNode
   */
  public abstract enable(): void;

  /**
   * Method that deactivates a node and all the posterior nodes that previously required that activation.
   * 
   * @returns an array with all the DiagramNode consequently disabled by the deactivation of the current node
   */
  public abstract disable(): Array<DiagramNode>;

  public abstract clone(): DiagramNode;

  public abstract getGreenLight(): boolean;

  /**
   * Method that returns whether the current node was already submitted (part of the history).
   * 
   * @returns a boolean that represents whether the current node was submitted
   */
  public abstract isSubmitted(): boolean;

  /**
   * Method that retrieves all the path variables that are used to choose the flow/s of the gateway/s of the current 
   * node selection that is awaiting approval, and the corresponding variable name to be submitted with those path variables.
   * The variable names and values have the following formatting:
   * - Variable names are composed by the gatewayId + the index of the branch; (i.e. a gateway with id "SomeGateway" and a 
   * branch index of 2 would be "SomeGateway2")
   * - Variable values are simply substring with 16 characters and substituted " " for "_" of the path names; 
   * 
   * @returns a map of key values, where the key is the variable name and the value is the variable value
   */
  public abstract getVariables(): Map<string, string>;

  /**
   * Method that evaluates whether the parameter 'activityId' corresponds to the property with the same node (in the case of a 
   * BasicNode), or if the node has a node in it's branch with that 'activityId' (in case of a GatewayNode).
   * 
   * @param activityId the property of the node to search for 
   * @returns true if the node has the requested activityId, false otherwise
   */
  public abstract hasActivityId(activityId: string): boolean;

  /**
   * Method that returns the last BasicNode's 'completionTime' value. In case of a GatewayNode, return the last BasicNode's 
   * 'completionTime', or the highest value amongst the last BasicNodes found.
   * 
   * @returns the previous BasicNode 'completionTime'
   */
  public abstract getPreviousCompletionTime(): Date;

  //public abstract where(predicate: (n: DiagramNode) => boolean): DiagramNode[];
}
