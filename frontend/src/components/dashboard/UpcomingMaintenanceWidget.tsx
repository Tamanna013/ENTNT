import React, { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useMaintenanceQuery } from '../../hooks/useMaintenance';
import { MaintenanceRecord } from '../../types/maintenance';
import { Badge } from '../ui/Badge';
import { Clock, Loader2 } from 'lucide-react';

export const UpcomingMaintenanceWidget: React.FC = () => {
    // 14 days from today
    const fourteenDaysOut = new Date();
    fourteenDaysOut.setDate(fourteenDaysOut.getDate() + 14);
    
    // (a) Upcoming within 14 days
    const { data: upcomingData, isLoading: isLoadingUpcoming } = useMaintenanceQuery({
        pageNumber: 1,
        pageSize: 50,
        dueBefore: fourteenDaysOut.toISOString(),
        sortBy: "scheduledDate"
    });

    // (b) ALL currently overdue records, regardless of date
    const { data: overdueData, isLoading: isLoadingOverdue } = useMaintenanceQuery({
        pageNumber: 1,
        pageSize: 50,
        status: "Overdue"
    });

    const isLoading = isLoadingUpcoming || isLoadingOverdue;

    const mergedRecords = useMemo(() => {
        const records = new Map<string, MaintenanceRecord>();

        // Add overdue records first
        if (overdueData?.items) {
            overdueData.items.forEach(item => {
                records.set(item.id, item);
            });
        }

        // Add upcoming records (if not already present)
        if (upcomingData?.items) {
            upcomingData.items.forEach(item => {
                if (!records.has(item.id)) {
                    records.set(item.id, item);
                }
            });
        }

        // Convert back to array
        const mergedArray = Array.from(records.values());

        // Sort: Overdue first, then by scheduledDate ascending
        mergedArray.sort((a, b) => {
            if (a.status === 'Overdue' && b.status !== 'Overdue') return -1;
            if (a.status !== 'Overdue' && b.status === 'Overdue') return 1;
            
            // If statuses are the same (or neither is Overdue), sort by date
            const dateA = new Date(a.scheduledDate).getTime();
            const dateB = new Date(b.scheduledDate).getTime();
            return dateA - dateB;
        });

        // Limit to top 10
        return mergedArray.slice(0, 10);
    }, [upcomingData, overdueData]);

    const getStatusBadgeVariant = (status: string) => {
        switch (status) {
            case 'Overdue': return 'red';
            case 'Scheduled': return 'blue';
            case 'InProgress': return 'amber';
            case 'Completed': return 'green';
            case 'Cancelled': return 'gray';
            default: return 'gray';
        }
    };

    return (
        <div className="bg-surface border border-border rounded-lg shadow-sm flex flex-col h-full">
            <div className="p-4 border-b border-border flex justify-between items-center bg-slate-900/50 rounded-t-lg">
                <h3 className="text-lg font-medium text-text-primary flex items-center">
                    <Clock className="w-5 h-5 mr-2 text-blue-400" />
                    Upcoming & Overdue Maintenance
                </h3>
            </div>
            
            <div className="flex-1 p-0 overflow-y-auto min-h-[300px]">
                {isLoading ? (
                    <div className="flex justify-center items-center h-full py-12">
                        <Loader2 className="h-8 w-8 animate-spin text-text-muted" />
                    </div>
                ) : mergedRecords.length === 0 ? (
                    <div className="flex justify-center items-center h-full py-12 text-text-muted">
                        No upcoming or overdue maintenance.
                    </div>
                ) : (
                    <div className="divide-y divide-border">
                        {mergedRecords.map((record) => (
                            <div key={record.id} className="p-4 hover:bg-surface-hover transition-colors">
                                <div className="flex justify-between items-start">
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2 mb-1">
                                            <span className="font-semibold text-text-primary">
                                                {record.shipName || 'Unknown Ship'}
                                            </span>
                                            <Badge 
                                                text={record.status} 
                                                color={getStatusBadgeVariant(record.status) as any} 
                                            />
                                        </div>
                                        <p className="text-sm text-text-muted line-clamp-1">
                                            {record.description}
                                        </p>
                                    </div>
                                    <div className="text-right ml-4">
                                        <span className="text-sm font-medium text-text-primary">
                                            {new Date(record.scheduledDate).toLocaleDateString()}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
            
            <div className="p-3 border-t border-border bg-slate-900/50 text-center rounded-b-lg mt-auto">
                <Link to="/maintenance" className="text-sm font-medium text-blue-400 hover:text-blue-300">
                    View all maintenance
                </Link>
            </div>
        </div>
    );
};
