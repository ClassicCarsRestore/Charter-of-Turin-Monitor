import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";
import { GatewayNode } from "./gateway-node";
import { SequenceFlowNode } from "./sequence-flow-node";

export class ParallelNode extends GatewayNode {

  public canEnable(): DiagramNode[] {
    var canEnable: Array<DiagramNode> = new Array<DiagramNode>();
    this.branches.forEach(br => canEnable = canEnable.concat(br.canEnable()));

    if (this.getGreenLight() && this.nextNode != null)
      canEnable = canEnable.concat(this.nextNode.canEnable());

    return canEnable;
  }

  public canBeValidated(): boolean {
    var selectedBranchCount: number = 0;
    // check if the nodes selected can be submited by verifying that, if a node is selected before a 
    // gateway, at least on node is selected inside the gateway
    this.branches.forEach(br => { 
      if (br.getGreenLight() && br.canBeValidated()) selectedBranchCount++;
    });

    if (selectedBranchCount != this.branches.length) return false;

    if (this.nextNode != null && this.getGreenLight()) return this.nextNode.canBeValidated();

    return true;
  }

  public clone(): DiagramNode {
    var clonedBranches: Array<SequenceFlowNode> = new Array<SequenceFlowNode>(this.branches.length);
    this.branches.forEach(br => clonedBranches.push(br.clone()));

    if (this.nextNode == null)
      return new ParallelNode(null, this.greenLight, clonedBranches, this.id);

    var nextNodeClone: DiagramNode = this.nextNode.clone();
    return new ParallelNode(nextNodeClone, this.greenLight, clonedBranches, this.id);
  }

  public getGreenLight(): boolean {
    return this.completedBranches() == this.branches.length;
  }

  public getVariables(): Map<string, string> {
    if (this.greenLight == true && this.nextNode != null)
      return this.nextNode.getVariables();

    return new Map<string, string>();
  }

  public static inferGatewayInstance(nextNode: DiagramNode| null, branches: Array<DiagramNode>): boolean {
    // if the next node is already submitted, the gateway has to be a SubmittedNode
    if (nextNode != null && nextNode.isSubmitted() ) {
      return false;
    }

    var hasUnfinishedBranch: boolean = false;
    
    // one path has to be completely submitted to be a SubmittedNode
    branches.forEach(node => { 
      var currentNode: DiagramNode = node;

      while (currentNode.isSubmitted() && currentNode.nextNode != null) {
        currentNode = currentNode.nextNode;
      }

      if (currentNode instanceof SequenceFlowNode) { // always should be
        // last node before ending gateway with a basic/gateway node before
        if (currentNode.previousNode != null && !currentNode.previousNode.isSubmitted() && currentNode.nextNode == null) {
          hasUnfinishedBranch = true;

        // single sequence flow in a branch
        } else if (currentNode.previousNode == null && !currentNode.getGreenLight() && currentNode.nextNode == null) {
          hasUnfinishedBranch = true;
        }
      }
    });

    return hasUnfinishedBranch;
  }

}
