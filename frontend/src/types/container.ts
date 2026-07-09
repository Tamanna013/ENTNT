import { PaginationQuery } from './pagination';

export interface Container {
  id: string;
  containerNumber: string;
  type: string;
  status: string;
  currentVoyageId: string | null;
  voyageNumber: string | null;
  linkedCargoIds: string[];
  createdAt: string;
}

export interface CreateContainerPayload {
  containerNumber: string;
  type: string;
  currentVoyageId?: string;
}

export interface UpdateContainerPayload {
  type: string;
  status: string;
  currentVoyageId?: string;
}

export interface ContainerQuery extends PaginationQuery {
  status?: string;
  type?: string;
  voyageId?: string;
}

export interface ContainerTrackingEvent {
  id: string;
  containerId: string;
  eventType: string;
  location: string;
  timestamp: string;
  notes: string | null;
  recordedByUserId: string;
  recordedByUserName: string;
  createdAt: string;
}

export interface RecordTrackingEventPayload {
  eventType: string;
  location: string;
  timestamp: string;
  notes?: string;
}
