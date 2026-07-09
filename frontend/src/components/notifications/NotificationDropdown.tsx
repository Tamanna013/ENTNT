import React from 'react';
import { Link } from 'react-router-dom';
import { useNotificationsQuery, useMarkAllReadMutation, useUnreadCountQuery } from '../../hooks/useNotifications';
import { NotificationItem } from './NotificationItem';
import { Loader2, CheckCircle2 } from 'lucide-react';

interface NotificationDropdownProps {
  onClose: () => void;
}

export const NotificationDropdown: React.FC<NotificationDropdownProps> = ({ onClose }) => {
  const { data: unreadData } = useUnreadCountQuery();
  const hasUnread = unreadData && unreadData.count > 0;
  
  const { data, isLoading } = useNotificationsQuery({
    pageNumber: 1,
    pageSize: 5
  });
  
  const { mutate: markAllRead, isPending: isMarkingAll } = useMarkAllReadMutation();

  const handleMarkAllRead = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (hasUnread && !isMarkingAll) {
      markAllRead();
    }
  };

  return (
    <div className="absolute right-0 mt-2 w-80 sm:w-96 bg-white rounded-lg shadow-xl ring-1 ring-black ring-opacity-5 focus:outline-none z-50 overflow-hidden flex flex-col max-h-[32rem]">
      <div className="px-4 py-3 border-b border-gray-200 flex justify-between items-center bg-gray-50">
        <h3 className="text-sm font-semibold text-gray-900">Notifications</h3>
        {hasUnread && (
          <button
            onClick={handleMarkAllRead}
            disabled={isMarkingAll}
            className="text-xs font-medium text-blue-600 hover:text-blue-800 flex items-center"
          >
            {isMarkingAll ? <Loader2 className="w-3 h-3 animate-spin mr-1" /> : <CheckCircle2 className="w-3 h-3 mr-1" />}
            Mark all read
          </button>
        )}
      </div>
      
      <div className="flex-1 overflow-y-auto">
        {isLoading ? (
          <div className="flex justify-center items-center py-8">
            <Loader2 className="h-6 w-6 animate-spin text-text-muted" />
          </div>
        ) : data?.items.length === 0 ? (
          <div className="px-4 py-8 text-center text-sm text-gray-500">
            No notifications yet.
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {data?.items.map((notification) => (
              <NotificationItem 
                key={notification.id} 
                notification={notification} 
                onCloseDropdown={onClose} 
              />
            ))}
          </div>
        )}
      </div>
      
      <div className="px-4 py-3 border-t border-gray-200 bg-gray-50 text-center">
        <Link 
          to="/notifications" 
          onClick={onClose}
          className="text-sm font-medium text-blue-600 hover:text-blue-800"
        >
          View all notifications
        </Link>
      </div>
    </div>
  );
};
