import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { documentsApi } from '../api/documentsApi';
import { DocumentQuery, CreateDocumentPayload, UpdateDocumentPayload } from '../types/document';
import { useToast } from './useToast';

export const useDocumentsQuery = (query: DocumentQuery) => {
  return useQuery({
    queryKey: ['documents', query],
    queryFn: () => documentsApi.getDocuments(query),
  });
};

export const useDocumentQuery = (id: string) => {
  return useQuery({
    queryKey: ['documents', id],
    queryFn: () => documentsApi.getDocumentById(id),
    enabled: !!id,
  });
};

export const useCreateDocumentMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ payload, file }: { payload: CreateDocumentPayload; file: File }) => 
      documentsApi.createDocument(payload, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents'] });
      showToast('Document created successfully.', 'success');
    },
    onError: (error) => {
      showToast('Failed to create document.', 'error');
      console.error(error);
    },
  });
};

export const useUpdateDocumentMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateDocumentPayload }) => 
      documentsApi.updateDocument(id, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['documents'] });
      queryClient.invalidateQueries({ queryKey: ['documents', variables.id] });
      showToast('Document updated successfully.', 'success');
    },
    onError: (error) => {
      showToast('Failed to update document.', 'error');
      console.error(error);
    },
  });
};

export const useDeleteDocumentMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (id: string) => documentsApi.deleteDocument(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents'] });
      showToast('Document deleted successfully.', 'success');
    },
    onError: (error) => {
      showToast('Failed to delete document.', 'error');
      console.error(error);
    },
  });
};

export const useUploadNewVersionMutation = (documentId: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ file, changeNotes }: { file: File; changeNotes?: string }) => 
      documentsApi.uploadNewVersion(documentId, file, changeNotes),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents', documentId] });
      queryClient.invalidateQueries({ queryKey: ['documents', documentId, 'versions'] });
      queryClient.invalidateQueries({ queryKey: ['documents'] });
      showToast('New version uploaded successfully.', 'success');
    },
    onError: (error) => {
      showToast('Failed to upload new version.', 'error');
      console.error(error);
    },
  });
};

export const useVersionsQuery = (documentId: string) => {
  return useQuery({
    queryKey: ['documents', documentId, 'versions'],
    queryFn: () => documentsApi.getVersions(documentId),
    enabled: !!documentId,
  });
};
