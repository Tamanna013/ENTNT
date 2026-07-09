import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Notification } from '../../types/notification';
import { useMarkReadMutation } from '../../hooks/useNotifications';
import { Info, AlertTriangle, AlertCircle, Clock, ShieldAlert } from 'lucide-react';
import { formatRelativeTime } from '../../lib/formatRelativeTime';

interface NotificationItemProps {
  notification: Notification;
  onCloseDropdown?: () => void;
}

export const NotificationItem: React.FC<NotificationItemProps> = ({ notification, onCloseDropdown }) => {
  const navigate = useNavigate();
  const { mutate: markRead } = useMarkReadMutation();

  const handleClick = () => {
    if (!notification.isRead) {
      markRead(notification.id);
    }
    
    if (onCloseDropdown) {
      onCloseDropdown();
    }
    
    if (notification.relatedEntityName && notification.relatedEntityId) {
      switch (notification.relatedEntityName) {
        case 'MaintenanceRecord':
          // Optionally route to maintenance detail if there was one, or ships detail
          navigate('/maintenance');
          break;
        case 'Voyage':
          navigate('/voyages');
          break;
        case 'FuelLog':
          navigate('/fuel');
          break;
        case 'CrewCertification':
          navigate('/crew');
          break;
        default:
          break;
      }
    }
  };

  const getStyleParams = (type: string) => {
    switch (type) {
      case 'MaintenanceOverdue':
        return { color: 'text-red-600', bg: 'bg-red-50', border: 'border-red-500', icon: <AlertCircle className="w-5 h-5 text-red-600" /> };
      case 'VoyageDelayed':
        return { color: 'text-amber-600', bg: 'bg-amber-50', border: 'border-amber-500', icon: <Clock className="w-5 h-5 text-amber-600" /> };
      case 'CertificationExpiring':
        return { color: 'text-yellow-600', bg: 'bg-yellow-50', border: 'border-yellow-400', icon: <ShieldAlert className="w-5 h-5 text-yellow-600" /> };
      case 'FuelAnomaly':
        return { color: 'text-orange-600', bg: 'bg-orange-50', border: 'border-orange-500', icon: <AlertTriangle className="w-5 h-5 text-orange-600" /> };
      default:
        return { color: 'text-gray-600', bg: 'bg-gray-50', border: 'border-gray-400', icon: <Info className="w-5 h-5 text-gray-600" /> };
    }
  };

  const styles = getStyleParams(notification.type);

  return (
    <div 
      onClick={handleClick}
      className={`p-4 border-l-4 cursor-pointer hover:bg-gray-50 transition-colors ${!notification.isRead ? 'bg-white' : 'bg-gray-100 opacity-75'} ${styles.border} border-b border-b-gray-200`}
    >
      <div className="flex items-start">
        <div className="flex-shrink-0 mt-0.5">
          {styles.icon}
        </div>
        <div className="ml-3 w-full">
          <p className={`text-sm ${!notification.isRead ? 'font-bold text-gray-900' : 'font-medium text-gray-700'}`}>
            {notification.title}
          </p>
          <p className="mt-1 text-sm text-gray-600 line-clamp-2">
            {notification.message}
          </p>
          <div className="flex items-center gap-1 mt-1.5 text-xs text-gray-500">
          <Clock className="w-3 h-3" />
          <span>{formatRelativeTime(notification.createdAt)}</span>
        </div>
        </div>
        {!notification.isRead && (
          <div className="flex-shrink-0 ml-2">
            <span className="inline-block w-2 h-2 rounded-full bg-blue-600"></span>
          </div>
        )}
      </div>
    </div>
  );
};
