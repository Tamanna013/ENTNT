import { z } from 'zod';

export const loginSchema = z.object({
    email: z.string().min(1, "Email is required").email("Invalid email address"),
    password: z.string().min(1, "Password is required"),
});

export const registerSchema = z.object({
    firstName: z.string().min(1, "First name is required").max(100, "Max 100 characters"),
    lastName: z.string().min(1, "Last name is required").max(100, "Max 100 characters"),
    email: z.string().min(1, "Email is required").email("Invalid email address"),
    password: z.string()
        .min(8, "Password must be at least 8 characters")
        .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
        .regex(/[0-9]/, "Password must contain at least one digit"),
    phoneNumber: z.string().optional(),
});
