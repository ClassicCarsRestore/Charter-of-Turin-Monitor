import { HttpHeaders } from "@angular/common/http";

export class Token {
  static getHeader() {
    let token = localStorage.getItem('RBToken');
    return { headers: new HttpHeaders().append('Authorization', "Bearer " + (token ? token : "")) };
  }

  static getHeaderBC() {
    let token = localStorage.getItem('ChainToken');
    return { headers: new HttpHeaders().append('Authorization', "Bearer " + (token ? token : "")) };
  }
}
