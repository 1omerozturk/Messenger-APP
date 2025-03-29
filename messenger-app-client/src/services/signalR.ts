import * as signalR from '@microsoft/signalr';
import { MessageDto } from '../types/message';

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private messageHandlers: ((message: MessageDto) => void)[] = [];
  private typingHandlers: ((userId: string) => void)[] = [];
  private stoppedTypingHandlers: ((userId: string) => void)[] = [];
  private messageReadHandlers: ((messageId: string) => void)[] = [];

  public async startConnection(token: string) {
    if (this.hubConnection) {
      await this.hubConnection.stop();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7001/chatHub', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.setupHandlers();
    await this.hubConnection.start();
  }

  private setupHandlers() {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveMessage', (message: MessageDto) => {
      this.messageHandlers.forEach(handler => handler(message));
    });

    this.hubConnection.on('UserTyping', (userId: string) => {
      this.typingHandlers.forEach(handler => handler(userId));
    });

    this.hubConnection.on('UserStoppedTyping', (userId: string) => {
      this.stoppedTypingHandlers.forEach(handler => handler(userId));
    });

    this.hubConnection.on('MessageRead', (messageId: string) => {
      this.messageReadHandlers.forEach(handler => handler(messageId));
    });
  }

  public async sendMessage(receiverId: string, content: string, type: string = 'Text') {
    if (!this.hubConnection) return;

    await this.hubConnection.invoke('SendMessage', {
      receiverId,
      content,
      type,
    });
  }

  public async markMessageAsRead(messageId: string) {
    if (!this.hubConnection) return;

    await this.hubConnection.invoke('MarkMessageAsRead', messageId);
  }

  public async startTyping(receiverId: string) {
    if (!this.hubConnection) return;

    await this.hubConnection.invoke('Typing', receiverId);
  }

  public async stopTyping(receiverId: string) {
    if (!this.hubConnection) return;

    await this.hubConnection.invoke('StopTyping', receiverId);
  }

  public onMessage(handler: (message: MessageDto) => void) {
    this.messageHandlers.push(handler);
    return () => {
      this.messageHandlers = this.messageHandlers.filter(h => h !== handler);
    };
  }

  public onTyping(handler: (userId: string) => void) {
    this.typingHandlers.push(handler);
    return () => {
      this.typingHandlers = this.typingHandlers.filter(h => h !== handler);
    };
  }

  public onStoppedTyping(handler: (userId: string) => void) {
    this.stoppedTypingHandlers.push(handler);
    return () => {
      this.stoppedTypingHandlers = this.stoppedTypingHandlers.filter(h => h !== handler);
    };
  }

  public onMessageRead(handler: (messageId: string) => void) {
    this.messageReadHandlers.push(handler);
    return () => {
      this.messageReadHandlers = this.messageReadHandlers.filter(h => h !== handler);
    };
  }

  public async stopConnection() {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }
}

export const signalRService = new SignalRService(); 