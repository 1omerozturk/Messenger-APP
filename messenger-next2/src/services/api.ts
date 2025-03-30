import axios, { AxiosError } from 'axios';

interface ErrorResponse {
  message?: string;
  errors?: string[];
}

const API_URL = 'http://localhost:5223';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true
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
  async (error: AxiosError<ErrorResponse>) => {
    const originalRequest = error.config;

    // Handle CORS errors
    if (error.response?.status === 0) {
      console.error('CORS error:', error);
      return Promise.reject(new Error('Network error. Please check your connection.'));
    }

    // Handle 401 errors (unauthorized)
    if (error.response?.status === 401 && originalRequest) {
      try {
        const token = localStorage.getItem('token');
        if (!token) {
          throw new Error('No token available');
        }

        // Try to refresh the token
        const response = await authService.refreshToken(token);
        if (response?.token) {
          localStorage.setItem('token', response.token);
          originalRequest.headers.Authorization = `Bearer ${response.token}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        console.error('Error refreshing token:', refreshError);
        // If refresh fails, redirect to login
        localStorage.removeItem('token');
        window.location.href = '/login';
        return Promise.reject(new Error('Session expired. Please login again.'));
      }
    }

    // Handle other errors
    if (error.response?.data) {
      const errorMessage = error.response.data.message || 
                          error.response.data.errors?.join(', ') || 
                          'An error occurred';
      return Promise.reject(new Error(errorMessage));
    }

    return Promise.reject(error);
  }
);

export const authService = {
  login: async (username: string, password: string) => {
    const response = await api.post('/api/user/login', { username, password });
    return response.data;
  },
  
  register: async (userData: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    profilePicture?: string;
  }) => {
    const response = await api.post('/api/user/register', userData);
    return response.data;
  },

  getProfile: async () => {
    const response = await api.get('/api/user/profile');
    return response.data;
  },

  refreshToken: async (oldToken: string) => {
    try {
      const response = await api.post('/api/user/refresh-token', { token: oldToken });
      return response.data;
    } catch (error) {
      console.error('Error refreshing token:', error);
      return null;
    }
  },

  uploadProfilePicture: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await axios.post(`${API_URL}/api/user/upload-profile-picture`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  updateProfilePicture: async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);

    const token = localStorage.getItem('token');
    const response = await axios.post(`${API_URL}/api/user/profile-picture`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
        'Authorization': token ? `Bearer ${token}` : '',
      },
    });
    return response.data;
  }
};

export const userService = {
  getProfile: async () => {
    const response = await api.get('/api/user/profile');
    return response.data;
  },
  
  updateProfile: async (userData: {
    firstName: string;
    lastName: string;
    profilePicture?: string;
  }) => {
    const response = await api.put('/api/user/profile', userData);
    return response.data;
  },
  
  getContacts: async () => {
    const response = await api.get('/api/user/contacts');
    return response.data;
  },
  
  addContact: async (contactId: string) => {
    const response = await api.post('/api/user/contacts', { contactId });
    return response.data;
  },
  
  removeContact: async (contactId: string) => {
    const response = await api.delete('/api/user/contacts', { data: { contactId } });
    return response.data;
  },
  
  getAllUsers: async () => {
    const response = await api.get('/api/user');
    return response.data;
  },
};

export const messageService = {
  getConversations: async () => {
    const response = await api.get('/api/message/conversations');
    return response.data;
  },
  
  getConversation: async (userId: string, skip = 0, take = 50) => {
    const response = await api.get(`/api/message/conversation/${userId}`, {
      params: { skip, take },
    });
    return response.data;
  },
  
  getUnreadMessages: async () => {
    const response = await api.get('/api/message/unread');
    return response.data;
  },
  
  markAsRead: async (messageId: string) => {
    const response = await api.put(`/api/message/${messageId}/read`);
    return response.data;
  },
  
  markAllAsRead: async (userId: string) => {
    const response = await api.post(`/api/message/read/all/${userId}`);
    return response.data;
  },
  
  getUnreadCount: async () => {
    const response = await api.get('/api/message/unread/count');
    return response.data;
  },
  
  deleteConversation: async (userId: string) => {
    const response = await api.delete(`/api/message/conversation/${userId}`);
    return response.data;
  },
}; 