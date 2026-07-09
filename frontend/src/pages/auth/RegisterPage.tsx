import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { authApi } from '../../api/authApi';
import { useAuthStore } from '../../store/authStore';
import { registerSchema } from '../../lib/validation';
import { RegisterPayload } from '../../types/auth';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { FormError } from '../../components/ui/FormError';

export const RegisterPage: React.FC = () => {
    const navigate = useNavigate();
    
    const setAuth = useAuthStore((state) => state.setAuth);
    const [serverError, setServerError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors }
    } = useForm<RegisterPayload>({
        resolver: zodResolver(registerSchema)
    });

    const registerMutation = useMutation({
        mutationFn: (data: RegisterPayload) => authApi.register(data),
        onSuccess: (data) => {
            setAuth(data.accessToken, data.user);
            navigate('/dashboard');
        },
        onError: (error: any) => {
            setSuccessMsg(null);
            setServerError(
                error.response?.data?.message || 'An unexpected error occurred during registration.'
            );
        }
    });

    const onSubmit = (data: RegisterPayload) => {
        setServerError(null);
        // Optional values from form might be empty string instead of undefined
        const payload = {
            ...data,
            phoneNumber: data.phoneNumber || undefined
        };
        registerMutation.mutate(payload);
    };

    return (
        <div className="w-full max-w-md p-8 rounded-lg bg-surface shadow-xl border border-border">
            <div className="text-center mb-8">
                <h1 className="text-3xl font-bold text-text-primary tracking-tight">FleetMind AI</h1>
                <p className="text-text-muted mt-2">Create a new account</p>
            </div>

            {successMsg && (
                <div className="mb-4 p-3 rounded-md bg-green-900/50 border border-green-500/50 text-green-200 text-sm text-center">
                    {successMsg}
                </div>
            )}

            <FormError message={serverError} />

            <form onSubmit={handleSubmit(onSubmit)}>
                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label="First Name"
                        {...register('firstName')}
                        error={errors.firstName?.message}
                    />
                    <Input
                        label="Last Name"
                        {...register('lastName')}
                        error={errors.lastName?.message}
                    />
                </div>

                <Input
                    label="Email Address"
                    type="email"
                    {...register('email')}
                    error={errors.email?.message}
                />
                
                <Input
                    label="Password"
                    type="password"
                    {...register('password')}
                    error={errors.password?.message}
                />

                <Input
                    label="Phone Number (Optional)"
                    type="tel"
                    {...register('phoneNumber')}
                    error={errors.phoneNumber?.message}
                />

                <Button type="submit" isLoading={registerMutation.isPending} className="mt-4">
                    Register
                </Button>
            </form>

            <div className="mt-6 text-center text-sm text-text-muted">
                Already have an account?{' '}
                <Link 
                    to="/login"
                    className="text-indigo-400 hover:text-indigo-300 font-medium transition-colors"
                >
                    Sign In
                </Link>
            </div>
        </div>
    );
};
