import { BasicNode } from "./basic-node";
import { DiagramNode } from "./diagram-node";
import { GatewayNode } from "./gateway-node";

export class SequenceFlowNode extends DiagramNode {
    // the id that identifies the node connected to the end (gateway or task)
    public nextNodeId: string;
    // property that indicated whether the current node was already submitted in the system
    public submitted: boolean;

    constructor(nextNode: DiagramNode| null, greenLight: boolean, sequenceFlowId: string, nextNodeId: string) {
        super(nextNode, sequenceFlowId, greenLight);
        this.nextNodeId = nextNodeId;
        this.submitted = false;
    }

    public canEnable(): DiagramNode[] {
        if (this.submitted) {
            if (this.nextNode == null) return []; 

            return this.nextNode.canEnable();
        }

        if (this.greenLight == false)
            return [this];
        if (this.nextNode != null)
            return this.nextNode.canEnable();

        return new Array<DiagramNode>();
    }

    public canDisable(): DiagramNode[] {
        if (this.submitted) {
            if (this.nextNode == null) return []; 

            return this.nextNode.canDisable();
        }

        var canDisable: Array<DiagramNode> = new Array<DiagramNode>();
        if (this.greenLight == true)
            canDisable.push(this);
        if (this.nextNode != null)
            canDisable = canDisable.concat(this.nextNode.canDisable());

        return canDisable;
    }

    public canBeValidated(): boolean {
        if (this.nextNode == null) return true; 

        return this.nextNode.canBeValidated();
    }

    public getNodesForSubmission(): BasicNode[] {
        var nodesToSubmit: Array<BasicNode> = new Array<BasicNode>();
        if ( (this.submitted || this.greenLight == true) && this.nextNode != null)
            nodesToSubmit = nodesToSubmit.concat(this.nextNode.getNodesForSubmission());

        return nodesToSubmit;
    }

    public enable(): void {
        this.greenLight = true;
    }

    public disable(): DiagramNode[] {
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

    public clone(): SequenceFlowNode {
        if (this.nextNode == null)
            return new SequenceFlowNode(null, this.greenLight, this.id, this.nextNodeId);

        var nextNodeClone: DiagramNode = this.nextNode.clone();
        return new SequenceFlowNode(nextNodeClone, this.greenLight, this.id, this.nextNodeId);
    }

    public getGreenLight(): boolean {
        if (this.submitted)
            return true;

        return this.greenLight;
    }

    public isSubmitted(): boolean {
        return this.submitted;
    }

    public getVariables(): Map<string, string> {
        if ((this.greenLight == true || this.submitted) && this.nextNode != null)
            return this.nextNode.getVariables();

        return new Map<string, string>();
    }

    public hasActivityId(activityId: string): boolean {
        return false;
    }

    public getPreviousCompletionTime(): Date {
        if (this.previousNode != null) {
            if (this.previousNode instanceof BasicNode)
                return this.previousNode.completionTime!;
                
            else if (this.previousNode instanceof GatewayNode)
                return this.previousNode.getPreviousCompletionTime();
        }
        
        let currentDate: Date = new Date() ;
        // fix the problem caused by summer time
        currentDate.setTime( currentDate.getTime() - new Date().getTimezoneOffset() * 60 * 1000 );

        return currentDate;
    }

}
