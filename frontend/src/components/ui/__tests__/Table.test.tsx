import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Table, Column } from '../Table';
import * as useMediaQueryModule from '../../../hooks/useMediaQuery';

// Mock the useMediaQuery hook
vi.mock('../../../hooks/useMediaQuery', () => ({
  useMediaQuery: vi.fn()
}));

const mockData = [
    { id: 1, name: 'Expected Value 1', role: 'Admin' },
    { id: 2, name: 'Expected Value 2', role: 'User' }
];

const columns: Column<typeof mockData[0]>[] = [
    { key: 'name', header: 'Name' },
    { key: 'role', header: 'Role' }
];

describe('Table', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders actual table elements at DESKTOP viewport', () => {
        // Desktop (not mobile)
        vi.mocked(useMediaQueryModule.useMediaQuery).mockReturnValue(false);

        render(<Table columns={columns} data={mockData} />);

        // Verify structure
        expect(document.querySelector('table')).toBeInTheDocument();
        expect(document.querySelectorAll('th').length).toBe(2);
        expect(document.querySelectorAll('tr').length).toBe(3); // 1 header row + 2 data rows
        
        // Verify data
        expect(screen.getByText('Expected Value 1')).toBeInTheDocument();
    });

    it('renders card-stack fallback layout at MOBILE viewport', () => {
        // Mobile
        vi.mocked(useMediaQueryModule.useMediaQuery).mockReturnValue(true);

        render(<Table columns={columns} data={mockData} />);

        // Verify structure: no table elements should be present
        expect(document.querySelector('table')).not.toBeInTheDocument();
        expect(document.querySelector('tr')).not.toBeInTheDocument();
        
        // Verify data is still present via text queries
        expect(screen.getByText('Expected Value 1')).toBeInTheDocument();
        expect(screen.getByText('Expected Value 2')).toBeInTheDocument();
    });
});
