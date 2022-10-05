import { Project } from "../project";

export class Extra {
  static search(p: Project, searchWord: string[]) {
    for (var w of searchWord) {
      if (!(p.make).toLowerCase().includes(w) && !p.model.toLowerCase().includes(w) && !p.licencePlate.toLowerCase().includes(w) && !('' + p.year).toLowerCase().includes(w) && !(p.isComplete ? "Finished" : "Active").toLowerCase().includes(w))
        return false;
    }
    return true;
  }
}
