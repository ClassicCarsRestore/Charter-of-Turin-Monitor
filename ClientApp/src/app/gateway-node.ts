import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";
import { SequenceFlowNode } from "./sequence-flow-node";

export abstract class GatewayNode extends DiagramNode {
  // array with the first node of each branch of the gateway
  public branches: Array<SequenceFlowNode>;

  constructor(nextNode: DiagramNode| null, greenLight: boolean, branches: Array<SequenceFlowNode>, gatewayId: string) {
    super(nextNode, gatewayId, greenLight);

    this.branches = new Array<SequenceFlowNode>();
    branches.forEach(br => this.addBranch(br));
  }

  /**
   * Auxiliary method that adds a branch to the 'branches' array.
   * 
   * @param branch the 'SequenceFlowNode' that represents the branch 
   */
  private addBranch(branch: SequenceFlowNode): void {
    this.branches.push(branch);

    var currentNode: DiagramNode| null = branch;
    while (currentNode != null) {
      currentNode.setParentGatewayNode(this);
      currentNode = currentNode.nextNode;
    }
  }

  public canDisable(): DiagramNode[] {
    var canDisable: Array<DiagramNode> = new Array<DiagramNode>();
    this.branches.forEach(br => canDisable = canDisable.concat(br.canDisable()) );

    if (this.nextNode != null)
      canDisable = canDisable.concat(this.nextNode.canDisable());

    return canDisable;
  }

  public getNodesForSubmission(): BasicNode[] {
    var nodesToSubmit: Array<BasicNode> = new Array<BasicNode>();
    this.branches.forEach(br => nodesToSubmit = nodesToSubmit.concat(br.getNodesForSubmission()) );

    if (this.nextNode != null)
      nodesToSubmit = nodesToSubmit.concat(this.nextNode.getNodesForSubmission());

    return nodesToSubmit;
  }

  public enable(): void {
    throw new Error("Method not implemented.");
  }

  public disable(): Array<DiagramNode> {
    var nodesDisabled: Array<DiagramNode> = new Array<DiagramNode>();
    this.branches.forEach(br => nodesDisabled = nodesDisabled.concat(br.disable()) );

    if (this.nextNode != null)
      nodesDisabled = nodesDisabled.concat(this.nextNode.disable());

    return nodesDisabled;
  }

  /**
   * Method that counts the number of branches that are fully submitted/complete.
   * 
   * @returns the count of fully submitted/complete branches
   */
  public completedBranches(): number {
    var completedBranches: number = 0;

    this.branches.forEach(br => {
      var currentNode: DiagramNode = br;

      while (currentNode.getGreenLight() && currentNode.nextNode != null) {
        currentNode = currentNode.nextNode;
      }

      if (currentNode instanceof SequenceFlowNode) { // always should be
        // last node before ending gateway with a basic node before
        if (currentNode.previousNode != null && currentNode.previousNode.getGreenLight() && currentNode.nextNode == null) {
          completedBranches++;

        // single sequence flow in a branch
        } else if (currentNode.previousNode == null && currentNode.getGreenLight() && currentNode.nextNode == null) {
          completedBranches++;
        }
      }
      
    });

    return completedBranches;
  }

  public isSubmitted(): boolean {
    return false;
  }

  public hasActivityId(activityId: string): boolean {
    var activityIdFound: boolean = false;
    // search all the branches for the activity id
    this.branches.forEach(br => {
      var node: DiagramNode| null = br;
      while (node != null ) {
        activityIdFound = activityIdFound || node.hasActivityId(activityId);

        // if the node is found stop the iteration
        if (node.getGreenLight() || node.isSubmitted())
          node = node.nextNode;
        else
          node = null;
      }
    });

    return activityIdFound;
  }

  public getPreviousCompletionTime(): Date {
    let currentDate: Date = new Date();
    // fix the problem caused by summer time
    currentDate.setTime( currentDate.getTime() - new Date().getTimezoneOffset() * 60 * 1000 );

    let completionDate: Date = currentDate;

    this.branches.forEach(br => {
      var currentNode: DiagramNode = br;
      // select the last selected node of the branch
      while (currentNode.getGreenLight() && currentNode.nextNode != null) {
        currentNode = currentNode.nextNode;
      }
      // save the latest completion date
      if (completionDate == null || completionDate < currentNode.getPreviousCompletionTime() ) 
        completionDate = currentNode.getPreviousCompletionTime();
      
    });
    
    return completionDate;
  }
}
