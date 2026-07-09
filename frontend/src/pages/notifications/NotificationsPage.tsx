import React, { useState } from 'react';
import { useNotificationsQuery, useMarkAllReadMutation, useUnreadCountQuery } from '../../hooks/useNotifications';
import { NotificationItem } from '../../components/notifications/NotificationItem';
import { Pagination } from '../../components/ui/Pagination';
import { NotificationQuery } from '../../types/notification';
import { NOTIFICATION_TYPES } from '../../lib/constants';
import { Loader2, CheckCircle2 } from 'lucide-react';
import { Button } from '../../components/ui/Button';

export const NotificationsPage: React.FC = () => {
  const [query, setQuery] = useState<NotificationQuery>({
    pageNumber: 1,
    pageSize: 10,
    unreadOnly: false,
    type: ''
  });

  const { data, isLoading } = useNotificationsQuery(query);
  const { data: unreadData } = useUnreadCountQuery();
  const { mutate: markAllRead, isPending: isMarkingAll } = useMarkAllReadMutation();

  const hasUnread = unreadData && unreadData.count > 0;

  const handleMarkAllRead = () => {
    if (hasUnread && !isMarkingAll) {
      markAllRead();
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-semibold text-gray-900">Notifications</h1>
        {hasUnread && (
          <Button 
            onClick={handleMarkAllRead} 
            disabled={isMarkingAll}
            className="flex items-center"
          >
            {isMarkingAll ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <CheckCircle2 className="w-4 h-4 mr-2" />}
            Mark all read
          </Button>
        )}
      </div>

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-4 border-b border-gray-200 flex flex-col sm:flex-row gap-4">
          <div className="flex items-center">
            <input
              id="unreadOnly"
              type="checkbox"
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              checked={query.unreadOnly || false}
              onChange={(e) => setQuery({ ...query, unreadOnly: e.target.checked, pageNumber: 1 })}
            />
            <label htmlFor="unreadOnly" className="ml-2 block text-sm text-gray-900">
              Show unread only
            </label>
          </div>
          
          <div className="w-full sm:w-64">
            <select
              className="block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md"
              value={query.type || ''}
              onChange={(e) => setQuery({ ...query, type: e.target.value, pageNumber: 1 })}
            >
              <option value="">All Notification Types</option>
              {NOTIFICATION_TYPES.map(type => (
                <option key={type} value={type}>{type.replace(/([A-Z])/g, ' $1').trim()}</option>
              ))}
            </select>
          </div>
        </div>

        {isLoading ? (
          <div className="flex justify-center items-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-text-muted" />
          </div>
        ) : data?.items.length === 0 ? (
          <div className="p-12 text-center text-gray-500">
            No notifications found matching your criteria.
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {data?.items.map(notification => (
              <NotificationItem key={notification.id} notification={notification} />
            ))}
          </div>
        )}

        {data && data.totalPages > 1 && (
          <div className="p-4 border-t border-gray-200">
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={data.totalPages}
              onPageChange={(page) => setQuery({ ...query, pageNumber: page })}
            />
          </div>
        )}
      </div>
    </div>
  );
};
