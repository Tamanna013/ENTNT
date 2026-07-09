import React, { useState, useEffect } from 'react';
import { Button } from '../ui/Button';
import { FileUpload } from '../ui/FileUpload';
import { DOCUMENT_CATEGORIES } from '../../lib/constants';
import { Document, CreateDocumentPayload, UpdateDocumentPayload } from '../../types/document';

interface DocumentFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (payload: any, file?: File) => void;
  document?: Document;
  isLoading?: boolean;
}

export const DocumentFormModal: React.FC<DocumentFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  document,
  isLoading = false
}) => {
  const isEditMode = !!document;
  const [title, setTitle] = useState('');
  const [category, setCategory] = useState<string>(DOCUMENT_CATEGORIES[0]);
  const [description, setDescription] = useState('');
  const [file, setFile] = useState<File | null>(null);
  
  // Entity binding omitted for simplicity in this milestone unless launched from a context
  // but the payload allows it.
  
  useEffect(() => {
    if (isOpen) {
      if (document) {
        setTitle(document.title);
        setCategory(document.category);
        setDescription(document.description || '');
      } else {
        setTitle('');
        setCategory(DOCUMENT_CATEGORIES[0]);
        setDescription('');
        setFile(null);
      }
    }
  }, [isOpen, document]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (isEditMode) {
      const payload: UpdateDocumentPayload = {
        title,
        category,
        description: description || undefined
      };
      onSubmit(payload);
    } else {
      if (!file) return;
      const payload: CreateDocumentPayload = {
        title,
        category,
        description: description || undefined
      };
      onSubmit(payload, file);
    }
  };

  const isFormValid = isEditMode ? (title && category) : (title && category && file);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-xl flex flex-col max-h-[90vh]">
        <div className="flex items-center justify-between p-6 border-b border-slate-200">
          <h2 className="text-xl font-semibold text-slate-900">
            {isEditMode ? 'Edit Document' : 'Upload New Document'}
          </h2>
          <button onClick={onClose} className="text-text-muted hover:text-slate-500">
            &times;
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="p-6 overflow-y-auto space-y-6">
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Title <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="e.g., Q3 Safety Guidelines"
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Category <span className="text-red-500">*</span>
              </label>
              <select
                required
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                {DOCUMENT_CATEGORIES.map(cat => (
                  <option key={cat} value={cat}>{cat}</option>
                ))}
              </select>
            </div>
            
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Description
              </label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                className="w-full rounded-md border border-slate-300 px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Optional details about this document..."
              />
            </div>

            {!isEditMode && (
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-2">
                  Document File (Initial Version) <span className="text-red-500">*</span>
                </label>
                <FileUpload
                  onFileSelected={setFile}
                  accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg"
                />
              </div>
            )}
          </div>
          
          <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <Button type="button" variant="secondary" onClick={onClose} disabled={isLoading}>
              Cancel
            </Button>
            <Button type="submit" disabled={!isFormValid || isLoading}>
              {isLoading ? 'Saving...' : (isEditMode ? 'Save Changes' : 'Upload Document')}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
