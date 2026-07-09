import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Modal } from '../Modal';

describe('Modal', () => {
    it('renders children and title when isOpen is true, and nothing when false', () => {
        const { rerender } = render(
            <Modal isOpen={true} onClose={() => {}} title="Test Modal">
                <div>Modal Content</div>
            </Modal>
        );
        expect(screen.getByText('Test Modal')).toBeInTheDocument();
        expect(screen.getByText('Modal Content')).toBeInTheDocument();

        rerender(
            <Modal isOpen={false} onClose={() => {}} title="Test Modal">
                <div>Modal Content</div>
            </Modal>
        );
        expect(screen.queryByText('Test Modal')).not.toBeInTheDocument();
        expect(screen.queryByText('Modal Content')).not.toBeInTheDocument();
    });

    it('clicking backdrop calls onClose', async () => {
        const onClose = vi.fn();
        const user = userEvent.setup();
        render(
            <Modal isOpen={true} onClose={onClose} title="Test Modal">
                <div>Content</div>
            </Modal>
        );
        
        // Find the backdrop (first div child inside Modal wrapper that has click event)
        // Since Modal wraps backdrop in fixed inset-0 flex, we can query by backdrop-blur-sm
        const backdrop = document.querySelector('.backdrop-blur-sm');
        expect(backdrop).toBeInTheDocument();
        await user.click(backdrop as Element);
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('pressing Escape calls onClose', async () => {
        const onClose = vi.fn();
        const user = userEvent.setup();
        render(
            <Modal isOpen={true} onClose={onClose} title="Test Modal">
                <div>Content</div>
            </Modal>
        );
        
        await user.keyboard('{Escape}');
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('focus trap works correctly', async () => {
        const user = userEvent.setup();
        render(
            <Modal isOpen={true} onClose={() => {}} title="Test Modal">
                <button>First Button</button>
                <button>Second Button</button>
            </Modal>
        );

        // Wait for Modal's setTimeout focusing logic to run
        await new Promise(resolve => setTimeout(resolve, 50));

        const closeBtn = screen.getByLabelText('Close modal');
        const firstBtn = screen.getByText('First Button');
        const secondBtn = screen.getByText('Second Button');

        // Initially closeBtn gets focus as it's the first focusable element
        expect(document.activeElement).toBe(closeBtn);

        // Move to first button
        await user.tab();
        expect(document.activeElement).toBe(firstBtn);
        
        // Move to second button
        await user.tab();
        expect(document.activeElement).toBe(secondBtn);

        // Tab from last element (secondBtn) should wrap to first element (closeBtn)
        await user.tab();
        expect(document.activeElement).toBe(closeBtn);

        // Shift+Tab from first element (closeBtn) should wrap to last element (secondBtn)
        await user.tab({ shift: true });
        expect(document.activeElement).toBe(secondBtn);
    });
});
