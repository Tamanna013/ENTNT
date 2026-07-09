import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { Notification, NotificationQuery } from '../types/notification';

export const notificationsApi = {
  getNotifications: async (query: NotificationQuery): Promise<PagedResult<Notification>> => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.unreadOnly !== undefined) params.append('unreadOnly', query.unreadOnly.toString());
    if (query.type) params.append('type', query.type);

    const response = await apiClient.get<PagedResult<Notification>>(`/notifications?${params.toString()}`);
    return response.data;
  },

  getUnreadCount: async (): Promise<{ count: number }> => {
    const response = await apiClient.get<{ count: number }>('/notifications/unread-count');
    return response.data;
  },

  markRead: async (id: string): Promise<void> => {
    await apiClient.put(`/notifications/${id}/read`);
  },

  markAllRead: async (): Promise<void> => {
    await apiClient.put('/notifications/read-all');
  }
};
