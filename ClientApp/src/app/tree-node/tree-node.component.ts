import { Component, EventEmitter, Input, Output } from '@angular/core'
import { PathNode } from '../process';

@Component({
  selector: 'app-tree-node',
  templateUrl: './tree-node.component.html',
  styleUrls: ['./tree-node.component.css']
})
export class TreeNodeComponent {
  @Output() private diagramChange: EventEmitter<PathNode> = new EventEmitter<PathNode>();
  @Input() node: any;
  @Input() selectedNode: any;

  changeDiagram(node: any) {
    this.diagramChange.emit(node);
  }
} 
