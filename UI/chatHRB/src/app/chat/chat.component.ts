import { Component } from '@angular/core';
import { ChatGPTService } from '../services/chat-gpt.service';

@Component({
  templateUrl: './chat.component.html',
  selector: 'app-chat',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent {

  constructor(private chatGPT: ChatGPTService) {}

  chatIsOpen = false;
  waitingOnGPT = false;
  messages: any[] = [
    {
      text: 'Hello, how can I help you today!',
      date: new Date(),
      reply: false,
      user: {
        name: 'Max Refund',
        avatar: 'assets/MaxRefund.png',
      },
    },
  ];

  async sendMessage(event: any) {
    this.messages.push({
      text: event.message,
      date: new Date(),
      reply: true,
      user: {
        name: 'You',
        avatar: 'https://techcrunch.com/wp-content/uploads/2015/08/safe_image.gif',
      },
    });
    this.messages.push({
      text: '...',
      date: new Date(),
      reply: false,
      user: {
        name: 'Max Refund',
        avatar: 'assets/MaxRefund.png',
      },
    });
    this.chatGPT.sendToGPT(event.message).subscribe(data => {
      this.messages.pop();
      this.messages.push({
        text: data,
        date: new Date(),
        reply: false,
        user: {
          name: 'Max Refund',
          avatar: 'assets/MaxRefund.png',
        },
      });
      this.waitingOnGPT = false;
    });
  }

  appendBubble()
  {
    const messagesDiv = document.querySelector('.messages');
    const chatBubbleHtml = '<div class="chat-bubble"><p>Typing...</p></div>';
    messagesDiv?.insertAdjacentHTML('beforeend', chatBubbleHtml);
  }

  toggleChat() {
      this.chatIsOpen = !this.chatIsOpen;
  }
}