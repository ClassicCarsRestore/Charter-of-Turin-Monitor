import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";
import { ExclusiveNode } from "./exclusive-node";
import { GatewayNode } from "./gateway-node";
import { ParallelNode } from "./parallel-node";
import { SequenceFlowNode } from "./sequence-flow-node";

export class InclusiveNode extends GatewayNode {

  public canEnable(): DiagramNode[] {
    var canEnable: Array<DiagramNode> = new Array<DiagramNode>();
    var hasSubmittedPath: boolean = false;

    // check for submitted branch
    this.branches.forEach(br => {
      if (br.isSubmitted()) hasSubmittedPath = true;
    });

    // if the first node of a branch is already submitted, just follow those branches
    this.branches.forEach(br => {
      if (hasSubmittedPath) {
        if (br.isSubmitted()) {
          console.log("found a submitted branch inside: " + this.id);
          canEnable = canEnable.concat(br.canEnable());
        }
      } else {
        canEnable = canEnable.concat(br.canEnable());
      }
    });


    if (this.getGreenLight() && this.nextNode != null)
      canEnable = canEnable.concat(this.nextNode.canEnable());

    return canEnable;
  }

  public canBeValidated(): boolean {
    console.log("inside can be validated of: " + this.id);

    var isValid: boolean = true;

    var selectedBranchCount: number = 0;
    // check if the nodes selected can be submited by verifying that, if a node is selected before a 
    // gateway, at least on node is selected inside the gateway
    this.branches.forEach(br => { 
      if (br.getGreenLight() ) {
        selectedBranchCount++;
        isValid = isValid && br.canBeValidated(); 
      }
    });

    if (selectedBranchCount < 1 || !isValid) return false;

    if (this.nextNode != null && this.getGreenLight()) return this.nextNode.canBeValidated();

    return true;
  }

  public clone(): DiagramNode {
    var clonedBranches: Array<SequenceFlowNode> = new Array<SequenceFlowNode>(this.branches.length);
    this.branches.forEach(br => clonedBranches.push(br.clone()));

    if (this.nextNode == null)
      return new InclusiveNode(null, this.greenLight, clonedBranches, this.id);

    var nextNodeClone: DiagramNode = this.nextNode.clone();
    return new InclusiveNode(nextNodeClone, this.greenLight, clonedBranches, this.id);
  }

  public getGreenLight(): boolean {

    var activatedBranches: number = 0;

    this.branches.forEach(br => {
      if (br.getGreenLight()) activatedBranches++;
    });

    return this.completedBranches() == activatedBranches;
  }

  public getVariables(): Map<string, string> {
    var variables: Map<string, string> = new Map<string, string>();
    
    this.branches.forEach(br => { 
      if (br.getGreenLight() && !br.isSubmitted()) {
        variables.set(br.nextNodeId, br.id);

        if (br.nextNode != null) 
          br.nextNode.getVariables().forEach((v, k) => variables.set(k, v));

      } else {
        variables.set(br.nextNodeId, "");

      }
    });

    if (this.nextNode != null && this.getGreenLight()) {
      if (variables.size > 0) 
        this.nextNode.getVariables().forEach((v, k) => variables.set(k, v));
      else 
        variables = this.nextNode.getVariables();
      
    }

    return variables;
  }

  public static inferGatewayInstance(nextNode: DiagramNode| null, branches: Array<DiagramNode>, currentActivityIds: Array<string>): boolean {
    // if the next node is already submitted, the gateway has to be a SubmittedNode
    if (nextNode != null && nextNode.isSubmitted() ) {
      return false;
    }

    var numberOfOpenPaths: number = 0;
    var hasUnfinishedBranch: boolean = false;

    // one or more paths have to be completely submitted and none of the starting branch nodes can be in the 
    // currentActivityIds to be a SubmittedNode (return false case)
    branches.forEach(node => { 
      var currentNode: DiagramNode = node;

      // verify if the path is fully submitted
      if (currentNode.isSubmitted() ) {
        while (currentNode.nextNode != null) {
          currentNode = currentNode.nextNode;

          if (!(currentNode instanceof SequenceFlowNode) && !currentNode.isSubmitted() ) { 
            hasUnfinishedBranch = true;
          }

        }

      } else {
        numberOfOpenPaths++;

        // verify if the node is in the currentActivityIds
        if (node instanceof BasicNode && currentActivityIds.indexOf(node.id) != -1) {
          hasUnfinishedBranch = true;//return true;

        } else if (node instanceof GatewayNode) {
          currentActivityIds.forEach(id => {
            if(node.hasActivityId(id)) hasUnfinishedBranch = true;
          });

        }
      }
    });

    if (numberOfOpenPaths == branches.length)
      return true;
    
    return hasUnfinishedBranch;
  }

}
