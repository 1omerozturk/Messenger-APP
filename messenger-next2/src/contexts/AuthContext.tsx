'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { UserDto } from '../types/user';
import { authService } from '../services/api';
import { signalRService } from '../services/signalR';

interface AuthContextType {
  user: UserDto | null;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  register: (userData: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem('token');
      if (storedToken) {
        setToken(storedToken);
        try {
          const userData = await authService.getProfile();
          setUser(userData);
          await signalRService.startConnection(storedToken);
        } catch (error) {
          console.error('Error fetching user profile:', error);
          // Try to refresh token
          try {
            const response = await authService.refreshToken(storedToken);
            if (response?.token) {
              setToken(response.token);
              localStorage.setItem('token', response.token);
              const userData = await authService.getProfile();
              setUser(userData);
              await signalRService.startConnection(response.token);
            } else {
              handleLogout();
            }
          } catch (refreshError) {
            console.error('Error refreshing token:', refreshError);
            handleLogout();
          }
        }
      }
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  useEffect(() => {
    if (!user) return;

    const unsubscribe = signalRService.onUserStatusChange((userId, isOnline) => {
      if (user.id === userId) {
        setUser(prev => prev ? { ...prev, isOnline } : null);
      }
    });

    return () => {
      unsubscribe();
    };
  }, [user]);

  const login = async (username: string, password: string) => {
    const response = await authService.login(username, password);
    setUser(response.user);
    setToken(response.token);
    localStorage.setItem('token', response.token);
    await signalRService.startConnection(response.token);
  };

  const register = async (userData: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }) => {
    const response = await authService.register(userData);
    setUser(response.user);
    setToken(response.token);
    localStorage.setItem('token', response.token);
    await signalRService.startConnection(response.token);
  };

  const handleLogout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('token');
    signalRService.stopConnection();
  };

  const logout = () => {
    handleLogout();
  };

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        login,
        register,
        logout,
        isAuthenticated: !!token,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
} 