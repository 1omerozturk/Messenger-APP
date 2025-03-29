import axios from 'axios';

const API_URL = 'https://localhost:7001/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for handling errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  login: async (username: string, password: string) => {
    const response = await api.post('/user/login', { username, password });
    return response.data;
  },
  
  register: async (userData: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }) => {
    const response = await api.post('/user/register', userData);
    return response.data;
  },
};

export const userService = {
  getProfile: async () => {
    const response = await api.get('/user/profile');
    return response.data;
  },
  
  updateProfile: async (userData: {
    firstName: string;
    lastName: string;
    profilePicture?: string;
  }) => {
    const response = await api.put('/user/profile', userData);
    return response.data;
  },
  
  getContacts: async () => {
    const response = await api.get('/user/contacts');
    return response.data;
  },
  
  addContact: async (contactId: string) => {
    const response = await api.post('/user/contacts', { contactId });
    return response.data;
  },
  
  removeContact: async (contactId: string) => {
    const response = await api.delete('/user/contacts', { data: { contactId } });
    return response.data;
  },
};

export const messageService = {
  getConversations: async () => {
    const response = await api.get('/message/conversations');
    return response.data;
  },
  
  getConversation: async (userId: string, skip = 0, take = 50) => {
    const response = await api.get(`/message/conversation/${userId}`, {
      params: { skip, take },
    });
    return response.data;
  },
  
  getUnreadMessages: async () => {
    const response = await api.get('/message/unread');
    return response.data;
  },
  
  getUnreadCount: async () => {
    const response = await api.get('/message/unread/count');
    return response.data;
  },
  
  markAsRead: async (messageId: string) => {
    const response = await api.post(`/message/read/${messageId}`);
    return response.data;
  },
  
  markAllAsRead: async (userId: string) => {
    const response = await api.post(`/message/read/all/${userId}`);
    return response.data;
  },
}; 