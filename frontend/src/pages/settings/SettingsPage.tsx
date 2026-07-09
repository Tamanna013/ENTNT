import { useTheme } from '../../hooks/useTheme';
import { ThemeSelector } from '../../components/settings/ThemeSelector';
import { NotificationPreferencesForm } from '../../components/settings/NotificationPreferencesForm';
import { Palette, Bell } from 'lucide-react';

export const SettingsPage: React.FC = () => {
  const { preference, setPreference } = useTheme();

  return (
    <div className="max-w-4xl mx-auto py-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-text-primary">Settings</h1>
        <p className="mt-1 text-sm text-text-muted">Manage your application preferences and configuration.</p>
      </div>

      <div className="space-y-6">
        {/* Appearance Section */}
        <section className="bg-surface rounded-xl shadow-sm border border-border overflow-hidden">
          <div className="px-6 py-5 border-b border-border bg-surface-hover/50">
            <h3 className="text-lg font-medium text-text-primary flex items-center gap-2">
              <Palette size={20} className="text-accent" />
              Appearance
            </h3>
            <p className="mt-1 text-sm text-text-muted">
              Customize how the application looks on your device.
            </p>
          </div>
          
          <div className="p-6">
            <div className="max-w-xl">
              <label className="block text-sm font-medium text-text-primary mb-3">
                Theme Preference
              </label>
              <ThemeSelector value={preference} onChange={setPreference} />
              <p className="mt-4 text-sm text-text-muted">
                Choose Light or Dark mode, or let it follow your System settings.
              </p>
            </div>
          </div>
        </section>

        {/* Notifications Section */}
        <section className="bg-surface rounded-xl shadow-sm border border-border overflow-hidden">
          <div className="px-6 py-5 border-b border-border bg-surface-hover/50">
            <h3 className="text-lg font-medium text-text-primary flex items-center gap-2">
              <Bell size={20} className="text-accent" />
              Notifications
            </h3>
            <p className="mt-1 text-sm text-text-muted">
              Manage which alerts you want to receive.
            </p>
          </div>
          
          <div className="p-6">
            <div className="max-w-2xl">
              <NotificationPreferencesForm />
            </div>
          </div>
        </section>
      </div>
    </div>
  );
};
