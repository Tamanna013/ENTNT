import React from 'react';
import { useMySettingsQuery, useUpdateMySettingsMutation } from '../../hooks/useUserProfile';
import { Bell, ShieldAlert, Clock, Ship, FileText, AlertTriangle } from 'lucide-react';

const NOTIFICATION_TYPES = [
  { key: 'MaintenanceOverdue', label: 'Overdue Maintenance Alerts', icon: <Clock size={18} className="text-amber-500" /> },
  { key: 'VoyageDelayed', label: 'Voyage Delay Alerts', icon: <Ship size={18} className="text-blue-500" /> },
  { key: 'CertificationExpiring', label: 'Certification Expiry Alerts', icon: <FileText size={18} className="text-orange-500" /> },
  { key: 'FuelAnomaly', label: 'Fuel Cost Anomaly Alerts', icon: <AlertTriangle size={18} className="text-red-500" /> },
  { key: 'IncidentReported', label: 'Incident Report Alerts', icon: <ShieldAlert size={18} className="text-red-600" /> },
  { key: 'General', label: 'General Notifications', icon: <Bell size={18} className="text-text-muted" /> }
];

export const NotificationPreferencesForm: React.FC = () => {
  const { data: settings, isLoading } = useMySettingsQuery();
  const updateSettings = useUpdateMySettingsMutation();

  if (isLoading || !settings) {
    return <div className="animate-pulse space-y-4">
      {[1, 2, 3, 4, 5].map(i => (
        <div key={i} className="h-12 bg-surface-hover rounded-md w-full"></div>
      ))}
    </div>;
  }

  const preferences = settings.notificationPreferences || {};

  const handleToggle = (key: string, currentValue: boolean) => {
    updateSettings.mutate({
      theme: settings.theme, // Preserve existing theme
      notificationPreferences: {
        ...preferences,
        [key]: !currentValue
      }
    });
  };

  return (
    <div className="space-y-4">
      {NOTIFICATION_TYPES.map(({ key, label, icon }) => {
        // Default to true if not explicitly set to false
        const isEnabled = preferences[key] !== false;

        return (
          <div key={key} className="flex items-center justify-between p-4 bg-surface rounded-lg border border-border">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-surface-hover rounded-md border border-border">
                {icon}
              </div>
              <div>
                <p className="text-sm font-medium text-text-primary">{label}</p>
                <p className="text-xs text-text-muted">Receive alerts for {label.toLowerCase()}</p>
              </div>
            </div>
            
            <button
              type="button"
              role="switch"
              aria-checked={isEnabled}
              onClick={() => handleToggle(key, isEnabled)}
              className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-accent focus:ring-offset-2 ${
                isEnabled ? 'bg-accent' : 'bg-surface-hover border-border'
              }`}
            >
              <span
                aria-hidden="true"
                className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                  isEnabled ? 'translate-x-5' : 'translate-x-0'
                }`}
              />
            </button>
          </div>
        );
      })}
    </div>
  );
};
