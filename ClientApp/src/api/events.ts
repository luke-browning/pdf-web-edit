import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService {

  private hubConnection: signalR.HubConnection
  public events: BehaviorSubject<string> = new BehaviorSubject<string>("")

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/events')
      .build();
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))
    this.hubConnection.on('fileschangedevent', (data) => {
        this.events.next("fileschangedevent")
      });
  }
}
