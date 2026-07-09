import { PaginationQuery } from './pagination';

export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  relatedEntityName: string | null;
  relatedEntityId: string | null;
  isRead: boolean;
  readAt: string | null;
  createdAt: string;
}

export interface NotificationQuery extends PaginationQuery {
  unreadOnly?: boolean;
  type?: string;
}
