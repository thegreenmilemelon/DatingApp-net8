import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { MemberCardComponent } from '../member-card/member-card.component';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css',
})
export class MemberListComponent implements OnInit {
  memberService = inject(MembersService);
  //pageNumber = 1;
  //pageSize = 5;
  // private accountService = inject(AccountService);
  // userPArams = new UserParams(this.accountService.currentUser());

  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  ngOnInit(): void {
    // if(this.memberService.members().length == 0) this.loadMembers();
    if (!this.memberService.paginatedResult()) this.loadMembers();
  }

  loadMembers() {
    //this.memberService.getMembers(this.pageNumber, this.pageSize);
    //this.memberService.getMembers(this.userPArams);
    this.memberService.getMembers();
  }

  resetFilters() {
    //this.userParams = new UserParams(this.accountService.currentUser());
    this.memberService.resetUserParams();
    this.loadMembers();
  }
  pageChanged(event: any) {
    //if(this.userParams.pageNumber != event.page)
    if (this.memberService.userParams().pageNumber != event.page) {
      this.memberService.userParams().pageNumber = event.page;
      this.loadMembers();
    }
  }
}
