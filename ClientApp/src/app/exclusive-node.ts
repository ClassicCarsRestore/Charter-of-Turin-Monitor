import { DiagramNode } from "./diagram-node";
import { GatewayNode } from "./gateway-node";
import { SequenceFlowNode } from "./sequence-flow-node";

export class ExclusiveNode extends GatewayNode {

  public canEnable(): DiagramNode[] {
    var selectedNode: DiagramNode| null = null as DiagramNode| null;
    // do a first iteration to see if there is already as selected path and prevent the others from 
    // entering the canEnable array
    this.branches.forEach((br: SequenceFlowNode) => {
      if (selectedNode == null && br.getGreenLight() == true) selectedNode = br;
    });

    var canEnable: Array<DiagramNode> = new Array<DiagramNode>();
    
    if (selectedNode != null) 
      canEnable = canEnable.concat(selectedNode.canEnable());
    else 
      this.branches.forEach(br => canEnable = canEnable.concat(br.canEnable()) );

    if (this.getGreenLight() && this.nextNode != null)
      canEnable = canEnable.concat(this.nextNode.canEnable());

    return canEnable;
  }

  public canBeValidated(): boolean {
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

    if (selectedBranchCount != 1 || !isValid) return false;
    if (this.nextNode != null && this.getGreenLight()) return this.nextNode.canBeValidated();

    return true;
  }

  public clone(): DiagramNode {
    var clonedBranches: Array<SequenceFlowNode> = new Array<SequenceFlowNode>(this.branches.length);
    this.branches.forEach(br => clonedBranches.push(br.clone()));

    if (this.nextNode == null)
      return new ExclusiveNode(null, this.greenLight, clonedBranches, this.id);

    var nextNodeClone: DiagramNode = this.nextNode.clone();
    return new ExclusiveNode(nextNodeClone, this.greenLight, clonedBranches, this.id);
  }

  public getGreenLight(): boolean {
    return this.completedBranches() == 1;
  }

  public getVariables(): Map<string, string> {
    var variables: Map<string, string> = new Map<string, string>();

    var submittedPath: DiagramNode| null = null as DiagramNode| null;
    // do a first iteration to see if there is already as selected path and prevent the others from 
    // entering the canEnable array
    this.branches.forEach(br => {
      if (submittedPath == null && br.isSubmitted() ) submittedPath = br;
    });

    if (submittedPath != null) {
      if (submittedPath.nextNode != null) 
        submittedPath.nextNode.getVariables().forEach((v, k) => variables.set(k, v));
    
    } else {
      this.branches.forEach(br => { 
        if (br.getGreenLight()) {
          variables.set(br.nextNodeId, br.id);

          if (br.nextNode != null) 
            br.nextNode.getVariables().forEach((v, k) => variables.set(k, v));
            
        } else {
          variables.set(br.nextNodeId, "");
        }
        
      });
    }

    if (this.nextNode != null && this.getGreenLight())
      this.nextNode.getVariables().forEach((v, k) => variables.set(k, v));
    
    return variables;
  }

  /**
   * TODO
   * @param nextNode 
   * @param branches 
   * @returns 
   */
  public static inferGatewayInstance(nextNode: DiagramNode| null, branches: Array<DiagramNode>): boolean {
    // if the next node is already submitted, the gateway has to be a SubmittedNode
    if (nextNode != null && nextNode.isSubmitted() ) {
      return false;
    }

    var noSubmittedPath: boolean = true;

    // one path has to be completely submitted to be a SubmittedNode
    branches.forEach(node => { 
      var currentNode: DiagramNode = node;

      while (currentNode.isSubmitted() && currentNode.nextNode != null) {
        currentNode = currentNode.nextNode;
      }

      if (currentNode instanceof SequenceFlowNode) { // always should be
        // last node before ending gateway with a basic/gateway node before
        if (currentNode.previousNode != null && currentNode.previousNode.isSubmitted() && currentNode.nextNode == null) {
          noSubmittedPath = false;

        // single sequence flow in a branch
        } else if (currentNode.previousNode == null && currentNode.getGreenLight() && currentNode.nextNode == null) {
          noSubmittedPath = false;
        }
      }

    });

    return noSubmittedPath;
  }

}
