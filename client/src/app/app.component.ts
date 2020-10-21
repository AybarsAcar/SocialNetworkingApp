import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})

// passing variables
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;

  // dependency injection
  constructor(
    private accountService: AccountService,
    private presence: PresenceService
  ) {}

  //lifecycle methods
  ngOnInit() {
    this.setCurrentUser();
  }

  // persisting the User Data
  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));

    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
  }
}
