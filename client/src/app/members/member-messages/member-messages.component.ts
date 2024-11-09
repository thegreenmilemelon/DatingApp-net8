import {
  AfterViewChecked,
  Component,
  ViewChild,
  inject,
  input,
} from '@angular/core';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';
import { MessageService } from '../../_services/message.service';
@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css',
})
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer?: any;
  messageService = inject(MessageService);
  username = input.required<string>();
  // messages = input.required<Message[]>();
  messageContent = '';
  // updateMessages = output<Message>();

  sendMessage() {
    this.messageService
      .sendMessage(this.username(), this.messageContent)
      .then(() => {
        this.messageForm?.reset();
        this.scrollToBottom();
      });
  }
  ngAfterViewChecked(): void {
    //loading scrolling behavior after component change check cycle
    this.scrollToBottom();
  }
  //added
  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTop =
        this.scrollContainer.nativeElement.scrollHeight;
    }
  }
  // ngOnInit(): void {
  //   this.loadMessages();
  // }

  // loadMessages(){
  //   this.messageService.getMessageThread(this.username()).subscribe({
  //     next: messages => this.messages = messages
  //   })
  // }
}
