import {
  Component,
  ElementRef,
  ViewChild,
  Inject,
  OnInit
} from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { map, switchMap } from 'rxjs/operators';

// @ts-ignore
import * as BpmnJS from 'src/app/diagram/bpmn-navigated-viewer.development.js';

import { from, Observable, Subscription } from 'rxjs';
import { Token } from '../common/tokens';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { PathNode } from '../process';

@Component({
  selector: 'app-diagram-base',
  templateUrl: './diagram-base.component.html',
  styleUrls: ['./diagram-base.component.css']
})
export class DiagramBaseComponent implements OnInit {
  private bpmnJS: BpmnJS;
  @ViewChild('ref', { static: true }) private el: ElementRef | undefined = undefined;

  public root!: PathNode;
  public openDefinition!: PathNode;

  private events = [
    'element.click'
  ];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService)
  {
    // bpmn.io variables
    this.bpmnJS = new BpmnJS();

    // Initialize the BPMN viewer and build the graph with current diagram information
    this.bpmnJS.on('import.done', ({ error }: any) => {
      if (!error) {
        this.bpmnJS.get('canvas').zoom('fit-viewport');
      }
    });

    this.bpmnJS.on('element.contextmenu', (e: any) => this.navigateSubProcess(e, this))
    this.bpmnJS.on('element.click', (e: any) => this.navigateSubProcess(e, this))
  }

  async ngOnInit(): Promise<void> {
    let definitionId: string | null = null;
    this.activatedRoute.paramMap.subscribe(params => definitionId = params.get('definitionId'));
    if (definitionId != null)
      await this.http.get<PathNode>(this.baseUrl + 'api/Projects/Definition/' + definitionId, Token.getHeader()).toPromise().then(result => {
        if (result != null) {
          this.root = result;
          let node = result;
          while (node.children.length > 0)
            node = node.children[0];
          this.openDefinition = node;
          this.loadUrl(this.baseUrl + 'api/Projects/Diagram/' + this.openDefinition.self.definitionId);
        }
      });

    if (!this.root)
      this.http.get<PathNode>(this.baseUrl + 'api/Projects/Root', Token.getHeader()).subscribe(result => {
        this.root = result;
        this.openDefinition = result;
        this.loadUrl(this.baseUrl + 'api/Projects/Diagram/' + result.self.definitionId);
      });
  }

  navigateSubProcess(e: any, t: this) {
    e.originalEvent.preventDefault();
    e.originalEvent.stopPropagation();
    if (e.element.type == "bpmn:CallActivity") {
      t.http.get<PathNode>(t.baseUrl + 'api/Projects/Node/' + t.openDefinition.self.definitionId + "/" + e.element.id, Token.getHeader()).subscribe(result => {
        t.openDefinition.children.push(result);
        t.openDefinition = result;
        t.loadUrl(t.baseUrl + 'api/Projects/Diagram/' + result.self.definitionId);
      });
    }
  }

  loadUrl(url: string): Subscription {
    return (
      this.http.get(url, { headers: Token.getHeader().headers, responseType: 'text' }).pipe(
        switchMap((xml: string) => this.importDiagram(xml)),
        map(result => result.warnings),
      ).subscribe()
    );
  }

  private importDiagram(xml: string): Observable<{ warnings: Array<any> }> {
    return from(this.bpmnJS.importXML(xml) as Promise<{ warnings: Array<any> }>);
  }

  ngOnDestroy(): void {
    this.bpmnJS.destroy();
  }

  ngAfterContentInit(): void {
    this.bpmnJS.attachTo(this.el!.nativeElement);
  }

  changeDiagram(node: PathNode) {
    if (node != this.openDefinition) {
      this.openDefinition = node;
      this.openDefinition.children = new Array();
      this.loadUrl(this.baseUrl + 'api/Projects/Diagram/' + this.openDefinition.self.definitionId);
    }
  }

  zoomOut() {
    var node = this.root;
    while (node.children[0].children.length > 0) {
      node = node.children[0];
    }
    this.changeDiagram(node);
  }
}
