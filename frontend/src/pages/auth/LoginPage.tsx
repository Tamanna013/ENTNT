import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { authApi } from '../../api/authApi';
import { useAuthStore } from '../../store/authStore';
import { loginSchema } from '../../lib/validation';
import { LoginPayload } from '../../types/auth';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { FormError } from '../../components/ui/FormError';

export const LoginPage: React.FC = () => {
    const navigate = useNavigate();
    
    const setAuth = useAuthStore((state) => state.setAuth);
    const [serverError, setServerError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors }
    } = useForm<LoginPayload>({
        resolver: zodResolver(loginSchema)
    });

    const loginMutation = useMutation({
        mutationFn: (data: LoginPayload) => authApi.login(data),
        onSuccess: (data) => {
            setAuth(data.accessToken, data.user);
            navigate('/dashboard');
        },
        onError: (error: any) => {
            setSuccessMsg(null);
            setServerError(
                error.response?.data?.message || 'An unexpected error occurred during login.'
            );
        }
    });

    const onSubmit = (data: LoginPayload) => {
        setServerError(null);
        loginMutation.mutate(data);
    };

    return (
        <div className="w-full max-w-md p-8 rounded-lg bg-surface shadow-xl border border-border">
            <div className="text-center mb-8">
                <h1 className="text-3xl font-bold text-text-primary tracking-tight">FleetMind AI</h1>
                <p className="text-text-muted mt-2">Sign in to your account</p>
            </div>

            {successMsg && (
                <div className="mb-4 p-3 rounded-md bg-green-900/50 border border-green-500/50 text-green-200 text-sm text-center">
                    {successMsg}
                </div>
            )}

            <FormError message={serverError} />

            <form onSubmit={handleSubmit(onSubmit)}>
                <Input
                    label="Email Address"
                    type="email"
                    autoComplete="email"
                    {...register('email')}
                    error={errors.email?.message}
                />
                
                <Input
                    label="Password"
                    type="password"
                    autoComplete="current-password"
                    {...register('password')}
                    error={errors.password?.message}
                />

                <div className="flex justify-end mb-6">
                    <Link 
                        to="/forgot-password"
                        className="text-sm text-indigo-400 hover:text-indigo-300 transition-colors"
                    >
                        Forgot password?
                    </Link>
                </div>

                <Button type="submit" isLoading={loginMutation.isPending}>
                    Sign In
                </Button>
            </form>

            <div className="mt-6 text-center text-sm text-text-muted">
                Don't have an account?{' '}
                <Link 
                    to="/register"
                    className="text-indigo-400 hover:text-indigo-300 font-medium transition-colors"
                >
                    Register
                </Link>
            </div>
        </div>
    );
};
