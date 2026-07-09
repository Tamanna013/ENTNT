import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { useDocumentsQuery, useCreateDocumentMutation, useUpdateDocumentMutation, useDeleteDocumentMutation } from '../../hooks/useDocuments';
import { DocumentQuery, Document } from '../../types/document';
import { DocumentsTable } from '../../components/documents/DocumentsTable';
import { DocumentFormModal } from '../../components/documents/DocumentFormModal';
import { Pagination } from '../../components/ui/Pagination';
import { Button } from '../../components/ui/Button';
import { Plus, Search } from 'lucide-react';
import { DOCUMENT_CATEGORIES } from '../../lib/constants';

export const DocumentsPage: React.FC = () => {
  const navigate = useNavigate();
  const user = useAuthStore(state => state.user);
  
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager'));
  
  const [query, setQuery] = useState<DocumentQuery>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    category: ''
  });
  
  const [searchInput, setSearchInput] = useState('');

  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [editingDoc, setEditingDoc] = useState<Document | undefined>(undefined);
  
  const { data, isLoading } = useDocumentsQuery(query);
  const createMutation = useCreateDocumentMutation();
  const updateMutation = useUpdateDocumentMutation();
  const deleteMutation = useDeleteDocumentMutation();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setQuery(prev => ({ ...prev, searchTerm: searchInput, pageNumber: 1 }));
  };

  const handleCreateNew = () => {
    setEditingDoc(undefined);
    setIsFormModalOpen(true);
  };

  const handleEdit = (doc: Document) => {
    setEditingDoc(doc);
    setIsFormModalOpen(true);
  };

  const handleFormSubmit = async (payload: any, file?: File) => {
    if (editingDoc) {
      await updateMutation.mutateAsync({ id: editingDoc.id, payload });
    } else {
      if (!file) return;
      await createMutation.mutateAsync({ payload, file });
    }
    setIsFormModalOpen(false);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Documents</h1>
          <p className="mt-1 text-sm text-text-muted">Manage compliance, regulatory, and policy documents.</p>
        </div>
        {canWrite && (
          <Button onClick={handleCreateNew} className="flex items-center gap-2">
            <Plus className="w-4 h-4" />
            Create Document
          </Button>
        )}
      </div>

      <div className="bg-white p-4 rounded-lg border border-slate-200 mb-6 flex flex-col sm:flex-row gap-4 justify-between items-center">
        <form onSubmit={handleSearch} className="flex-1 w-full flex gap-2">
          <div className="relative flex-1 max-w-md">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Search className="h-4 w-4 text-text-muted" />
            </div>
            <input
              type="text"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              className="block w-full pl-10 pr-3 py-2 border border-slate-300 rounded-md leading-5 bg-white placeholder-slate-500 focus:outline-none focus:placeholder-slate-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              placeholder="Search by title..."
            />
          </div>
          <Button type="submit" variant="secondary">Search</Button>
        </form>

        <div className="w-full sm:w-auto">
          <select
            value={query.category || ''}
            onChange={(e) => setQuery(prev => ({ ...prev, category: e.target.value, pageNumber: 1 }))}
            className="block w-full pl-3 pr-10 py-2 text-base border-slate-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md"
          >
            <option value="">All Categories</option>
            {DOCUMENT_CATEGORIES.map(cat => (
              <option key={cat} value={cat}>{cat}</option>
            ))}
          </select>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-12 text-text-muted">Loading documents...</div>
      ) : (
        <>
          <DocumentsTable
            documents={data?.items || []}
            canWrite={canWrite}
            onView={(id) => navigate(`/documents/${id}`)}
            onEdit={handleEdit}
            onDelete={(id) => deleteMutation.mutate(id)}
          />

          {data && data.totalCount > 0 && (
            <div className="mt-6">
              <Pagination
                pageNumber={data.pageNumber}
                totalPages={Math.ceil(data.totalCount / data.pageSize)}
                onPageChange={(page) => setQuery(prev => ({ ...prev, pageNumber: page }))}
              />
            </div>
          )}
        </>
      )}

      <DocumentFormModal
        isOpen={isFormModalOpen}
        onClose={() => setIsFormModalOpen(false)}
        onSubmit={handleFormSubmit}
        document={editingDoc}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
};
