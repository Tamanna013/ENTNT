import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Badge } from '../Badge';

describe('Badge', () => {
    it('renders text content correctly', () => {
        render(<Badge text="Status OK" />);
        expect(screen.getByText('Status OK')).toBeInTheDocument();
    });

    it('applies visually distinct class for each supported color variant', () => {
        const { rerender } = render(<Badge text="Badge" color="blue" />);
        expect(screen.getByText('Badge')).toHaveClass('text-blue-400');

        rerender(<Badge text="Badge" color="green" />);
        expect(screen.getByText('Badge')).toHaveClass('text-emerald-400');

        rerender(<Badge text="Badge" color="red" />);
        expect(screen.getByText('Badge')).toHaveClass('text-rose-400');
    });
});
