import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { 
  Document, 
  CreateDocumentPayload, 
  UpdateDocumentPayload, 
  DocumentQuery, 
  DocumentVersion 
} from '../types/document';

const BASE_URL = '/documents';

export const documentsApi = {
  getDocuments: async (query: DocumentQuery): Promise<PagedResult<Document>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', query.pageNumber.toString());
    params.append('pageSize', query.pageSize.toString());
    
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.category) params.append('category', query.category);
    if (query.entityName) params.append('entityName', query.entityName);
    if (query.entityId) params.append('entityId', query.entityId);

    const response = await apiClient.get<PagedResult<Document>>(`${BASE_URL}?${params.toString()}`);
    return response.data;
  },

  getDocumentById: async (id: string): Promise<Document> => {
    const response = await apiClient.get<Document>(`${BASE_URL}/${id}`);
    return response.data;
  },

  createDocument: async (payload: CreateDocumentPayload, file: File): Promise<Document> => {
    const formData = new FormData();
    formData.append('title', payload.title);
    formData.append('category', payload.category);
    if (payload.description) formData.append('description', payload.description);
    if (payload.entityName) formData.append('entityName', payload.entityName);
    if (payload.entityId) formData.append('entityId', payload.entityId);
    formData.append('file', file);

    const response = await apiClient.post<Document>(BASE_URL, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  updateDocument: async (id: string, payload: UpdateDocumentPayload): Promise<Document> => {
    const response = await apiClient.put<Document>(`${BASE_URL}/${id}`, payload);
    return response.data;
  },

  deleteDocument: async (id: string): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${id}`);
  },

  uploadNewVersion: async (documentId: string, file: File, changeNotes?: string): Promise<DocumentVersion> => {
    const formData = new FormData();
    formData.append('file', file);
    if (changeNotes) formData.append('changeNotes', changeNotes);

    const response = await apiClient.post<DocumentVersion>(`${BASE_URL}/${documentId}/versions`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  getVersions: async (documentId: string): Promise<DocumentVersion[]> => {
    const response = await apiClient.get<DocumentVersion[]>(`${BASE_URL}/${documentId}/versions`);
    return response.data;
  }
};
