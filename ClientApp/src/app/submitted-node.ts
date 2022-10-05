import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";

export class SubmittedNode extends DiagramNode {

    constructor(nextNode: DiagramNode| null, id: string) {
        super(nextNode, id, true);
    }

    public canEnable(): DiagramNode[] {
        if (this.nextNode == null) return []; 

        return this.nextNode.canEnable();
    }
    
    public canDisable(): DiagramNode[] {
        if (this.nextNode == null) return []; 

        return this.nextNode.canDisable();
    }

    public canBeValidated(): boolean {
        if (this.nextNode == null) return true; 

        return this.nextNode.canBeValidated();
    }

    public getNodesForSubmission(): BasicNode[] {
        if (this.nextNode == null) return []; 

        return this.nextNode.getNodesForSubmission();
    }

    public enable(): void {
        throw new Error("Method not implemented.");
    }

    public disable(): BasicNode[] {
        throw new Error("Method not implemented.");
    }

    public clone(): DiagramNode {
        throw new Error("Method not implemented.");
    }

    public getGreenLight(): boolean {
        return true;
    }

    public isSubmitted(): boolean {
        return true;
    }

    public getVariables(): Map<string, string> {
        if (this.nextNode != null)
          return this.nextNode.getVariables();
        
        return new Map<string, string>();
    }

    public hasActivityId(activityId: string): boolean {
        return false;
    }

    public getPreviousCompletionTime(): Date {
        return new Date();
    }
  
}
  