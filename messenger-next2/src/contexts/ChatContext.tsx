'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { MessageDto, ConversationDto } from '../types/message';
import { messageService } from '../services/api';
import { signalRService } from '../services/signalR';
import { useAuth } from './AuthContext';
import { userService } from '../services/api';

interface ChatContextType {
  conversations: ConversationDto[];
  currentConversation: ConversationDto | null;
  messages: MessageDto[];
  setCurrentConversation: (conversation: ConversationDto | null) => void;
  sendMessage: (content: string, type: string) => Promise<void>;
  loadMoreMessages: () => Promise<void>;
  markAsRead: (messageId: string) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  unreadCount: number;
  isLoading: boolean;
  error: string | null;
  allUsers: any[];
  loadAllUsers: () => Promise<void>;
  startNewConversation: (userId: string) => void;
  deleteConversation: (userId: string) => Promise<boolean>;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

export function ChatProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [currentConversation, setCurrentConversation] = useState<ConversationDto | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [skip, setSkip] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [allUsers, setAllUsers] = useState<any[]>([]);
  const take = 50;

  useEffect(() => {
    if (user) {
      loadConversations();
      loadUnreadCount();
      loadAllUsers();
    }
  }, [user]);

  useEffect(() => {
    if (currentConversation) {
      loadMessages();
    }
  }, [currentConversation]);

  useEffect(() => {
    if (!user) return;

    const unsubscribe = signalRService.onMessage((message) => {
      if (currentConversation && 
          (message.senderId === currentConversation.userId || 
           message.receiverId === currentConversation.userId)) {
        setMessages(prev => [...prev, message]);
        
        setConversations(prev => {
          const otherUserId = message.senderId === user.id ? message.receiverId : message.senderId;
          const updatedConversations = prev.map(conv => {
            if (conv.userId === otherUserId) {
              return {
                ...conv,
                lastMessage: message.content,
                lastMessageTime: message.createdAt
              };
            }
            return conv;
          });
          
          const conversationExists = prev.some(conv => conv.userId === otherUserId);
          
          if (!conversationExists) {
            const otherUser = allUsers.find(u => u.id === otherUserId);
            
            if (otherUser) {
              const newConversation: ConversationDto = {
                userId: otherUser.id,
                username: otherUser.username,
                firstName: otherUser.firstName,
                lastName: otherUser.lastName,
                profilePicture: otherUser.profilePicture,
                isOnline: otherUser.isOnline,
                lastSeen: otherUser.lastSeen,
                lastMessage: message.content,
                lastMessageTime: message.createdAt,
                unreadCount: message.senderId === user.id ? 0 : 1
              };
              
              return [newConversation, ...updatedConversations];
            }
          }
          
          return updatedConversations;
        });
        
      } else {
        setConversations(prev => {
          const otherUserId = message.senderId === user.id ? message.receiverId : message.senderId;
          
          const updatedConversations = prev.map(conv => {
            if (conv.userId === message.senderId && message.senderId !== user.id) {
              return {
                ...conv,
                lastMessage: message.content,
                lastMessageTime: message.createdAt,
                unreadCount: conv.unreadCount + 1
              };
            }
            return conv;
          });
          
          const conversationExists = prev.some(conv => conv.userId === otherUserId);
          
          if (!conversationExists) {
            const otherUser = allUsers.find(u => u.id === otherUserId);
            
            if (otherUser) {
              const newConversation: ConversationDto = {
                userId: otherUser.id,
                username: otherUser.username,
                firstName: otherUser.firstName,
                lastName: otherUser.lastName,
                profilePicture: otherUser.profilePicture,
                isOnline: otherUser.isOnline,
                lastSeen: otherUser.lastSeen,
                lastMessage: message.content,
                lastMessageTime: message.createdAt,
                unreadCount: message.senderId === user.id ? 0 : 1
              };
              
              return [newConversation, ...updatedConversations];
            } else {
              setTimeout(() => loadConversations(), 100);
              return updatedConversations;
            }
          }
          
          return updatedConversations;
        });
      }
      
      loadUnreadCount();
    });

    // Gönderilen mesaj dinleyicisi
    const unsubscribeSent = signalRService.onMessageSent((message) => {
      // Kendi gönderdiğimiz mesajı da mesajlar listesine ekleyelim
      if (currentConversation && message.receiverId === currentConversation.userId) {
        setMessages(prev => {
          // Eğer geçici mesajlar varsa, gerçek mesaj ile değiştir
          const filteredMessages = prev.filter(m => !m.id.startsWith('temp-') || m.content !== message.content);
          
          // Mesajları sırala (tarih sırasına göre)
          const updatedMessages = [...filteredMessages, message].sort((a, b) => {
            return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          });
          
          return updatedMessages;
        });
        
        // Aynı zamanda sohbet listesini de güncelleyelim
        setConversations(prev => {
          return prev.map(conv => {
            if (conv.userId === message.receiverId) {
              return {
                ...conv,
                lastMessage: message.content,
                lastMessageTime: message.createdAt
              };
            }
            return conv;
          });
        });
      }
    });

    const unsubscribeStatus = signalRService.onUserStatusChange((userId, isOnline) => {
      setConversations(prev => 
        prev.map(conv => 
          conv.userId === userId 
            ? { ...conv, isOnline, lastSeen: isOnline ? new Date() : conv.lastSeen }
            : conv
        )
      );
    });

    return () => {
      unsubscribe();
      unsubscribeSent();
      unsubscribeStatus();
    };
  }, [user, currentConversation, allUsers]);

  const loadConversations = async () => {
    if (!user) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await messageService.getConversations();
      
      // Gelen verileri tarih sırasına göre sırala (en yeni mesaj en üstte)
      const sortedData = data.sort((a: ConversationDto, b: ConversationDto) => {
        return new Date(b.lastMessageTime).getTime() - new Date(a.lastMessageTime).getTime();
      });
      
      // Aynı userId'ye sahip mükerrer kayıtları temizle
      const uniqueConversations = sortedData.reduce((acc: ConversationDto[], current: ConversationDto) => {
        const x = acc.find(item => item.userId === current.userId);
        if (!x) {
          return acc.concat([current]);
        } else {
          return acc;
        }
      }, []);
      
      setConversations(uniqueConversations);
    } catch (err) {
      console.error('Error loading conversations:', err);
      setError('Failed to load conversations');
    } finally {
      setIsLoading(false);
    }
  };

  const loadMessages = async () => {
    if (!currentConversation) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await messageService.getConversation(currentConversation.userId, 0, take);
      setMessages(data);
      setSkip(take);
    } catch (err) {
      console.error('Error loading messages:', err);
      setError('Failed to load messages');
    } finally {
      setIsLoading(false);
    }
  };

  const loadMoreMessages = async () => {
    if (!currentConversation) return;
    setIsLoading(true);
    setError(null);
    try {
      const data = await messageService.getConversation(currentConversation.userId, skip, take);
      setMessages(prev => [...data, ...prev]);
      setSkip(prev => prev + take);
    } catch (err) {
      console.error('Error loading more messages:', err);
      setError('Failed to load more messages');
    } finally {
      setIsLoading(false);
    }
  };

  const loadUnreadCount = async () => {
    if (!user) return;
    try {
      const count = await messageService.getUnreadCount();
      setUnreadCount(count);
    } catch (err) {
      console.error('Error loading unread count:', err);
    }
  };

  const loadAllUsers = async () => {
    if (!user) return;
    try {
      const users = await userService.getAllUsers();
      setAllUsers(users.filter((u: any) => u.id !== user.id));
    } catch (err) {
      console.error('Error loading all users:', err);
      setError('Failed to load users');
    }
  };

  const sendMessage = async (content: string, type: string = 'Text') => {
    if (!currentConversation || !user) return;
    
    try {
      // Geçici mesaj oluştur (UI'da hemen göstermek için)
      const tempId = `temp-${Date.now()}`;
      const tempMessage: MessageDto = {
        id: tempId,
        senderId: user.id,
        receiverId: currentConversation.userId,
        content: content,
        isRead: false,
        type: type as any,
        createdAt: new Date()
      };
      
      // Geçici mesajı ekranımızda göster
      setMessages(prev => [...prev, tempMessage]);
      
      // Gerçek mesajı gönder
      await signalRService.sendMessage(currentConversation.userId, content, type);
      
      // Not: Sunucu gerçek mesajı gönderdiğinde, onMessageSent ile yakalanacak
      // ve geçici mesaj yerine gerçek mesaj kullanılacak
    } catch (err) {
      console.error('Error sending message:', err);
      setError('Failed to send message');
      
      // Hata durumunda, geçici mesajı kaldır
      setMessages(prev => prev.filter(msg => !msg.id.startsWith('temp-')));
    }
  };

  const markAsRead = async (messageId: string) => {
    try {
      await messageService.markAsRead(messageId);
      setMessages(prev =>
        prev.map(msg =>
          msg.id === messageId
            ? { ...msg, isRead: true, readAt: new Date() }
            : msg
        )
      );
    } catch (err) {
      console.error('Error marking message as read:', err);
      setError('Failed to mark message as read');
    }
  };

  const markAllAsRead = async () => {
    if (!currentConversation) return;
    try {
      await messageService.markAllAsRead(currentConversation.userId);
      setMessages(prev =>
        prev.map(msg =>
          msg.senderId === currentConversation.userId && !msg.isRead
            ? { ...msg, isRead: true, readAt: new Date() }
            : msg
        )
      );
    } catch (err) {
      console.error('Error marking all messages as read:', err);
      setError('Failed to mark messages as read');
    }
  };

  const startNewConversation = (userId: string) => {
    const selectedUser = allUsers.find(u => u.id === userId);
    if (!selectedUser) return;
    
    // Önce mevcut konuşmaları kontrol et
    const existingConversation = conversations.find(c => c.userId === userId);
    
    if (existingConversation) {
      // Mevcut konuşmayı kullan
      setCurrentConversation(existingConversation);
    } else {
      // Yeni konuşma oluştur
      const newConversation: ConversationDto = {
        userId: selectedUser.id,
        username: selectedUser.username,
        firstName: selectedUser.firstName,
        lastName: selectedUser.lastName,
        profilePicture: selectedUser.profilePicture,
        isOnline: selectedUser.isOnline,
        lastSeen: selectedUser.lastSeen,
        lastMessage: '',
        lastMessageTime: new Date(),
        unreadCount: 0
      };
      
      // Mevcut konuşmaları kontrol et ve yeni konuşmayı ekle (aynı userId ile başka ekleme yapma)
      setConversations(prev => {
        // Var olan kontrol et (double-check)
        if (prev.some(c => c.userId === userId)) {
          return prev;
        }
        return [newConversation, ...prev];
      });
      
      // Yeni konuşmayı aktif et
      setCurrentConversation(newConversation);
      setMessages([]);
    }
  };

  const deleteConversation = async (userId: string): Promise<boolean> => {
    if (!user) return false;
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await messageService.deleteConversation(userId);
      
      if (response.success) {
        // Silinen sohbeti listeden kaldır
        setConversations(prev => prev.filter(c => c.userId !== userId));
        
        // Eğer şu an görüntülenen sohbet ise, temizle
        if (currentConversation?.userId === userId) {
          setCurrentConversation(null);
          setMessages([]);
        }
        
        return true;
      }
      
      return false;
    } catch (err) {
      console.error('Error deleting conversation:', err);
      setError('Failed to delete conversation');
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <ChatContext.Provider
      value={{
        conversations,
        currentConversation,
        messages,
        setCurrentConversation,
        sendMessage,
        loadMoreMessages,
        markAsRead,
        markAllAsRead,
        unreadCount,
        isLoading,
        error,
        allUsers,
        loadAllUsers,
        startNewConversation,
        deleteConversation
      }}
    >
      {children}
    </ChatContext.Provider>
  );
}

export function useChat() {
  const context = useContext(ChatContext);
  if (context === undefined) {
    throw new Error('useChat must be used within a ChatProvider');
  }
  return context;
} 