import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { HandleError } from '../common/error';
import { Token } from '../common/tokens';

@Component({
  selector: 'app-pinterest',
  templateUrl: './pinterest.component.html',
  styleUrls: ['./pinterest.component.css']
})
export class PinterestComponent implements OnInit {

  constructor(private client: HttpClient, @Inject('BASE_URL') private baseUrl: string, private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params.code) {
        this.client.get(this.baseUrl + 'api/Account/PinterestOauth/' + params.code, Token.getHeader()).subscribe(result => {
          this.router.navigate(['']);
        }); }
      else {
        this.client.get(this.baseUrl + 'api/Account/Pinterest/Check', { headers: Token.getHeader().headers, responseType: 'text' }).subscribe(result => {
          if (result == "")
            this.router.navigate(['']);
          else {
            document.location.href = result;
          }
        }, error => {
          console.log(error);
        })
      }
    });
  }

  ngOnInit(): void {
  }

}
