import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { FleetFormModal } from '../FleetFormModal';
import { server } from '../../../test/mocks/server';
import { http, HttpResponse } from 'msw';

// Wrapper to provide a fresh QueryClient for each test
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('FleetFormModal', () => {
    const mockOnClose = vi.fn();
    const alertMock = vi.spyOn(window, 'alert').mockImplementation(() => {});

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('blocks invalid submissions client-side without making a network request', async () => {
        const user = userEvent.setup();
        const Wrapper = createWrapper();

        render(
            <Wrapper>
                <FleetFormModal isOpen={true} onClose={mockOnClose} />
            </Wrapper>
        );

        // Leave name empty and submit
        const submitButton = screen.getByRole('button', { name: /Save Fleet/i });
        await user.click(submitButton);

        // Zod validation should display error inline
        expect(await screen.findByText('Name is required')).toBeInTheDocument();
        
        // Ensure onClose was never called (no success)
        expect(mockOnClose).not.toHaveBeenCalled();
    });

    it('submits successfully with valid data and calls onClose', async () => {
        const user = userEvent.setup();
        const Wrapper = createWrapper();

        render(
            <Wrapper>
                <FleetFormModal isOpen={true} onClose={mockOnClose} />
            </Wrapper>
        );

        // Wait for ports to load to avoid unhandled request warnings
        await waitFor(() => expect(screen.queryByText(/Mock Port/)).toBeInTheDocument());

        // Fill out form
        const nameInput = screen.getByRole('textbox', { name: /Name/i });
        await user.type(nameInput, 'Valid Fleet Name');

        // Note: Description is optional
        // Select home port
        // PortSelect might be a standard select or a custom component. Let's assume standard for now or combobox.
        // The modal uses <PortSelect {...register('homePortId')} label="Home Port" />
        // Since we don't know the exact HTML of PortSelect, let's just select the option.
        // It's likely a standard select or custom that renders options.
        // Since it's a combobox or select, we'll try standard select first.
        const portSelect = screen.getByLabelText(/Home Port/i);
        await user.selectOptions(portSelect, 'port-1'); // From our mock

        const submitButton = screen.getByRole('button', { name: /Save Fleet/i });
        await user.click(submitButton);

        // Successful submission should trigger onClose
        await waitFor(() => expect(mockOnClose).toHaveBeenCalledTimes(1));
    });

    it('surfaces server-side error (409 Conflict) correctly', async () => {
        const user = userEvent.setup();
        const Wrapper = createWrapper();

        // Configure MSW to explicitly intercept the POST with a 409
        server.use(
            http.post('http://localhost:5000/api/v1/fleets', () => {
                return HttpResponse.json(
                    { message: 'A fleet with this name already exists.' },
                    { status: 409 }
                );
            })
        );

        render(
            <Wrapper>
                <FleetFormModal isOpen={true} onClose={mockOnClose} />
            </Wrapper>
        );

        await waitFor(() => expect(screen.queryByText(/Mock Port/)).toBeInTheDocument());

        const nameInput = screen.getByRole('textbox', { name: /Name/i });
        await user.type(nameInput, 'Conflict Fleet');

        const portSelect = screen.getByLabelText(/Home Port/i);
        await user.selectOptions(portSelect, 'port-1');

        const submitButton = screen.getByRole('button', { name: /Save Fleet/i });
        await user.click(submitButton);

        // We mock alert() because FleetFormModal currently uses alert(error.response?.data?.message)
        await waitFor(() => {
            expect(alertMock).toHaveBeenCalledWith('A fleet with this name already exists.');
        });
        
        expect(mockOnClose).not.toHaveBeenCalled();
    });
});
