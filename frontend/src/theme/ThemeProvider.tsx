import React, { createContext, useEffect, useState, useMemo } from 'react';
import { useMySettingsQuery, useUpdateMySettingsMutation } from '../hooks/useUserProfile';

export type ThemePreference = 'Light' | 'Dark' | 'System';
export type ResolvedTheme = 'light' | 'dark';

export interface ThemeContextType {
  preference: ThemePreference;
  resolvedTheme: ResolvedTheme;
  setPreference: (preference: ThemePreference) => void;
}

export const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [preference, setPreference] = useState<ThemePreference>('System');
  const [systemIsDark, setSystemIsDark] = useState<boolean>(false);

  const { data: settings, isLoading } = useMySettingsQuery();
  const updateSettings = useUpdateMySettingsMutation();

  useEffect(() => {
    if (settings && !isLoading) {
      setPreference((settings.theme as ThemePreference) || 'System');
    }
  }, [settings, isLoading]);

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    setSystemIsDark(mediaQuery.matches);

    const handler = (e: MediaQueryListEvent) => {
      setSystemIsDark(e.matches);
    };

    mediaQuery.addEventListener('change', handler);
    return () => mediaQuery.removeEventListener('change', handler);
  }, []);

  const resolvedTheme: ResolvedTheme = useMemo(() => {
    if (preference === 'Dark') return 'dark';
    if (preference === 'Light') return 'light';
    return systemIsDark ? 'dark' : 'light';
  }, [preference, systemIsDark]);

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', resolvedTheme);
  }, [resolvedTheme]);

  const handleSetPreference = (newPref: ThemePreference) => {
    setPreference(newPref);
    if (settings) {
      updateSettings.mutate({
        theme: newPref,
        notificationPreferences: settings.notificationPreferences
      });
    }
  };

  return (
    <ThemeContext.Provider value={{ preference, resolvedTheme, setPreference: handleSetPreference }}>
      {children}
    </ThemeContext.Provider>
  );
};
