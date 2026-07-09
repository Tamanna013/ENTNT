import React, { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { authApi } from '../../api/authApi';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { FormError } from '../../components/ui/FormError';

const resetSchema = z.object({
    newPassword: z.string()
        .min(8, "Password must be at least 8 characters")
        .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
        .regex(/[0-9]/, "Password must contain at least one digit"),
});

type ResetPayload = z.infer<typeof resetSchema>;

export const ResetPasswordPage: React.FC = () => {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const [serverError, setServerError] = useState<string | null>(null);
    const [successMsg, setSuccessMsg] = useState<string | null>(null);

    useEffect(() => {
        if (!token) {
            setServerError("No reset token found in the URL.");
        }
    }, [token]);

    const {
        register,
        handleSubmit,
        formState: { errors }
    } = useForm<ResetPayload>({
        resolver: zodResolver(resetSchema)
    });

    const resetMutation = useMutation({
        mutationFn: (data: ResetPayload) => authApi.resetPassword(token!, data.newPassword),
        onSuccess: () => {
            setSuccessMsg('Password has been successfully reset. You can now log in.');
            setServerError(null);
        },
        onError: (error: any) => {
            setSuccessMsg(null);
            setServerError(
                error.response?.data?.message || 'An unexpected error occurred during password reset.'
            );
        }
    });

    const onSubmit = (data: ResetPayload) => {
        if (!token) return;
        setServerError(null);
        setSuccessMsg(null);
        resetMutation.mutate(data);
    };

    return (
        <div className="w-full max-w-md p-8 rounded-lg bg-surface shadow-xl border border-border">
            <div className="text-center mb-8">
                <h1 className="text-3xl font-bold text-text-primary tracking-tight">FleetMind AI</h1>
                <p className="text-text-muted mt-2">Enter your new password</p>
            </div>

            {successMsg && (
                <div className="mb-4 p-3 rounded-md bg-green-900/50 border border-green-500/50 text-green-200 text-sm text-center">
                    {successMsg}
                </div>
            )}

            <FormError message={serverError} />

            <form onSubmit={handleSubmit(onSubmit)}>
                <Input
                    label="New Password"
                    type="password"
                    {...register('newPassword')}
                    error={errors.newPassword?.message}
                    disabled={!token || !!successMsg}
                />
                
                <Button 
                    type="submit" 
                    isLoading={resetMutation.isPending} 
                    className="mt-2"
                    disabled={!token || !!successMsg}
                >
                    Reset Password
                </Button>
            </form>

            <div className="mt-6 text-center text-sm text-text-muted">
                <Link 
                    to="/login"
                    className="text-indigo-400 hover:text-indigo-300 font-medium transition-colors"
                >
                    Back to Sign In
                </Link>
            </div>
        </div>
    );
};
