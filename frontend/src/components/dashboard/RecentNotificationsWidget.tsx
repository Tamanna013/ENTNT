import React from 'react';
import { Link } from 'react-router-dom';
import { useNotificationsQuery } from '../../hooks/useNotifications';
import { NotificationItem } from '../notifications/NotificationItem';
import { Bell, Loader2 } from 'lucide-react';

export const RecentNotificationsWidget: React.FC = () => {
    const { data, isLoading } = useNotificationsQuery({
        pageNumber: 1,
        pageSize: 5
    });

    return (
        <div className="bg-surface border border-border rounded-lg shadow-sm flex flex-col h-full">
            <div className="p-4 border-b border-border flex justify-between items-center bg-slate-900/50 rounded-t-lg">
                <h3 className="text-lg font-medium text-text-primary flex items-center">
                    <Bell className="w-5 h-5 mr-2 text-blue-400" />
                    Recent Notifications
                </h3>
            </div>
            
            <div className="flex-1 p-0 overflow-y-auto min-h-[300px]">
                {isLoading ? (
                    <div className="flex justify-center items-center h-full py-12">
                        <Loader2 className="h-8 w-8 animate-spin text-text-muted" />
                    </div>
                ) : data?.items.length === 0 ? (
                    <div className="flex justify-center items-center h-full py-12 text-text-muted">
                        No notifications yet.
                    </div>
                ) : (
                    <div className="divide-y divide-border flex flex-col">
                        {data?.items.map((notification) => (
                            <div key={notification.id} className="relative bg-surface">
                                {/* Wrap NotificationItem for dark mode compatibility if needed, 
                                    though NotificationItem uses its own light classes. 
                                    Let's override styles or just use it as is. */}
                                <NotificationItem notification={notification} />
                            </div>
                        ))}
                    </div>
                )}
            </div>
            
            <div className="p-3 border-t border-border bg-slate-900/50 text-center rounded-b-lg mt-auto">
                <Link to="/notifications" className="text-sm font-medium text-blue-400 hover:text-blue-300">
                    View all notifications
                </Link>
            </div>
        </div>
    );
};
