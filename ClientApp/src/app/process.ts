export interface PathProcess {
  name: string;
  definitionId: string;
  instanceId: string;
}

export interface PathNode {
  self: PathProcess
  children: PathNode[]
}
