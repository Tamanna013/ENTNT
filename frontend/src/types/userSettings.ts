export interface UserSettings {
  theme: 'Light' | 'Dark' | 'System';
  notificationPreferences: Record<string, boolean>;
  updatedAt: string;
}

export interface UpdateUserSettingsPayload {
  theme: 'Light' | 'Dark' | 'System';
  notificationPreferences: Record<string, boolean>;
}

export interface UpdateOwnProfilePayload {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}
