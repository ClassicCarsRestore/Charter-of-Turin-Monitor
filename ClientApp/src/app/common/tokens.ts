import { HttpHeaders } from "@angular/common/http";

export class Token {
  static getHeader() {
    return { headers: new HttpHeaders(), withCredentials: true };
  }

  static getHeaderBC() {
    let token = localStorage.getItem('ChainToken');
    return { headers: new HttpHeaders().append('Authorization', "Bearer " + (token ? token : "")) };
  }
}
