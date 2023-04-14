import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class ChatGPTService {

  public appId: string = 'MYB';
  public userId: string = '20071';
  public taxYear: number = 2023;

  constructor(private httpClient: HttpClient, private route: ActivatedRoute) { 
    this.route.queryParams.subscribe(params => {
      this.appId = params['appId'];
      this.userId = params['userId'];
      this.taxYear = params['taxYear'];
    });
  }

  sendToGPT(message: string): Observable<any> { 
    let response = this.httpClient.post(`https://chat-hrb.herokuapp.com/chat?appId=${this.appId}&userId=${this.userId}&taxYear=${this.taxYear}&input=` + message, null,  { responseType: 'text' });
    return response;
  }
}
