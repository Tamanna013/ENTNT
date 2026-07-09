import { UserDto as AuthUser } from './auth';

export interface User extends AuthUser {
  // Inherits id, firstName, lastName, email, phoneNumber, isActive, roles
  // We can extend it here if needed in the future
}

export interface CreateUserPayload {
  firstName: string;
  lastName: string;
  email: string;
  password?: string;
  phoneNumber?: string;
  roleNames: string[];
}

export interface UpdateUserPayload {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  roleNames: string[];
}

export interface AssignRolesPayload {
  roleNames: string[];
}
