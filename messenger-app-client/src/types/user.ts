export interface UserDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  profilePicture?: string;
  isOnline: boolean;
  lastSeen?: Date;
}

export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UpdateUserDto {
  firstName: string;
  lastName: string;
  profilePicture?: string;
}

export interface UserLoginDto {
  username: string;
  password: string;
}

export interface UserContactDto {
  userId: string;
  contactId: string;
} 