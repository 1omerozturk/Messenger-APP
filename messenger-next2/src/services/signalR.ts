import * as signalR from '@microsoft/signalr';
import { MessageDto } from '../types/message';

class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private messageHandlers: ((message: MessageDto) => void)[] = [];
  private typingHandlers: ((userId: string) => void)[] = [];
  private stoppedTypingHandlers: ((userId: string) => void)[] = [];
  private messageReadHandlers: ((messageId: string) => void)[] = [];
  private userStatusHandlers: ((userId: string, isOnline: boolean) => void)[] = [];
  private connectionErrorHandlers: ((error: Error) => void)[] = [];
  private messageSentHandlers: ((message: MessageDto) => void)[] = [];
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectTimeout = 5000; // 5 seconds
  private connectionStarting = false;

  public async startConnection(token: string) {
    if (this.connectionStarting) return;
    this.connectionStarting = true;
    
    try {
      if (this.hubConnection) {
        await this.hubConnection.stop();
      }

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5223/chatHub', {
          accessTokenFactory: () => token,
          withCredentials: true
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals in milliseconds
        .build();

      this.setupHandlers();
      this.setupConnectionEvents();
      await this.hubConnection.start();
      console.log('SignalR Connected successfully');
      this.reconnectAttempts = 0;
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionErrorHandlers.forEach(handler => handler(error as Error));
      
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        this.reconnectAttempts++;
        setTimeout(() => {
          this.connectionStarting = false;
          this.startConnection(token);
        }, this.reconnectTimeout);
      }
    } finally {
      this.connectionStarting = false;
    }
  }

  private setupConnectionEvents() {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
      this.connectionErrorHandlers.forEach(handler => handler(error));
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.reconnectAttempts = 0;
    });

    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.connectionErrorHandlers.forEach(handler => handler(error));
      
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        this.reconnectAttempts++;
        setTimeout(() => {
          this.startConnection(this.hubConnection?.connectionId || '');
        }, this.reconnectTimeout);
      }
    });
  }

  private setupHandlers() {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveMessage', (message: MessageDto) => {
      this.messageHandlers.forEach(handler => handler(message));
    });

    this.hubConnection.on('MessageSent', (message: MessageDto) => {
      this.messageSentHandlers.forEach(handler => handler(message));
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

    this.hubConnection.on('UserStatusChanged', (userId: string, isOnline: boolean) => {
      this.userStatusHandlers.forEach(handler => handler(userId, isOnline));
    });
  }

  public onConnectionError(handler: (error: Error) => void) {
    this.connectionErrorHandlers.push(handler);
    return () => {
      this.connectionErrorHandlers = this.connectionErrorHandlers.filter(h => h !== handler);
    };
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

  public onUserStatusChange(handler: (userId: string, isOnline: boolean) => void) {
    this.userStatusHandlers.push(handler);
    return () => {
      this.userStatusHandlers = this.userStatusHandlers.filter(h => h !== handler);
    };
  }

  public onMessageSent(handler: (message: MessageDto) => void) {
    this.messageSentHandlers.push(handler);
    return () => {
      this.messageSentHandlers = this.messageSentHandlers.filter(h => h !== handler);
    };
  }

  public async sendMessage(receiverId: string, content: string, type: string = 'Text') {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error('Cannot send message: SignalR not connected');
      return;
    }
    
    try {
      await this.hubConnection.invoke('SendMessage', { receiverId, content, type });
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  }

  public async startTyping(receiverId: string) {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('Typing', receiverId);
  }

  public async stopTyping(receiverId: string) {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('StopTyping', receiverId);
  }

  public async markMessageAsRead(messageId: string) {
    if (!this.hubConnection) return;
    await this.hubConnection.invoke('MarkMessageAsRead', messageId);
  }

  public async stopConnection() {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }
}

export const signalRService = new SignalRService(); 