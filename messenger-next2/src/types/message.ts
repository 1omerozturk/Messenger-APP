export enum MessageType {
  Text = 'Text',
  Image = 'Image',
  File = 'File',
  Voice = 'Voice',
  Video = 'Video',
}

export interface MessageDto {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  isRead: boolean;
  readAt?: Date;
  type: MessageType;
  attachmentUrl?: string;
  createdAt: Date;
}

export interface CreateMessageDto {
  receiverId: string;
  content: string;
  type: MessageType;
  attachmentUrl?: string;
}

export interface ConversationDto {
  userId: string;
  username: string;
  firstName: string;
  lastName: string;
  profilePicture?: string;
  lastMessage: string;
  lastMessageTime: Date;
  unreadCount: number;
  isOnline: boolean;
  lastSeen?: Date;
} 