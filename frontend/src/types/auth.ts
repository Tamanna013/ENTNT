export interface UserDto {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string | null;
    isActive: boolean;
    roles: string[];
    createdAt: string;
}

export interface AuthResponse {
    accessToken: string;
    expiresAt: string;
    user: UserDto;
}

export interface RegisterPayload {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phoneNumber?: string;
}

export interface LoginPayload {
    email: string;
    password: string;
}
