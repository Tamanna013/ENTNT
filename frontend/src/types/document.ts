export interface Document {
  id: string;
  title: string;
  category: string;
  description: string | null;
  entityName: string | null;
  entityId: string | null;
  currentVersionNumber: number;
  currentVersionDownloadUrl: string;
  createdAt: string;
}

export interface CreateDocumentPayload {
  title: string;
  category: string;
  description?: string;
  entityName?: string;
  entityId?: string;
}

export interface UpdateDocumentPayload {
  title: string;
  category: string;
  description?: string;
}

export interface DocumentQuery {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
  category?: string;
  entityName?: string;
  entityId?: string;
}

export interface DocumentVersion {
  id: string;
  documentId: string;
  versionNumber: number;
  downloadUrl: string;
  fileName: string;
  uploadedByUserId: string;
  uploadedByUserName: string;
  changeNotes: string | null;
  createdAt: string;
}
