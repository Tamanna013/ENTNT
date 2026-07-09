import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { authApi } from '../../api/authApi';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { FormError } from '../../components/ui/FormError';

const forgotSchema = z.object({
    email: z.string().min(1, "Email is required").email("Invalid email address"),
});

type ForgotPayload = z.infer<typeof forgotSchema>;

export const ForgotPasswordPage: React.FC = () => {
    const [serverError, setServerError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors }
    } = useForm<ForgotPayload>({
        resolver: zodResolver(forgotSchema)
    });

    const forgotMutation = useMutation({
        mutationFn: (data: ForgotPayload) => authApi.forgotPassword(data.email),
        onSuccess: () => {
            // ALWAYS show this generic message regardless of if email exists
            setSuccessMsg('If that email is registered, a reset link has been sent.');
            setServerError(null);
        },
        onError: (error: any) => {
            setSuccessMsg(null);
            setServerError(
                error.response?.data?.message || 'An unexpected error occurred.'
            );
        }
    });

    const onSubmit = (data: ForgotPayload) => {
        setServerError(null);
        setSuccessMsg(null);
        forgotMutation.mutate(data);
    };

    return (
        <div className="w-full max-w-md p-8 rounded-lg bg-surface shadow-xl border border-border">
            <div className="text-center mb-8">
                <h1 className="text-3xl font-bold text-text-primary tracking-tight">FleetMind AI</h1>
                <p className="text-text-muted mt-2">Reset your password</p>
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
                    {...register('email')}
                    error={errors.email?.message}
                />
                
                <Button type="submit" isLoading={forgotMutation.isPending} className="mt-2">
                    Send Reset Link
                </Button>
            </form>

            <div className="mt-6 text-center text-sm text-text-muted">
                Remember your password?{' '}
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
