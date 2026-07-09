import React from 'react';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { FileText, Eye, Edit2, Trash2 } from 'lucide-react';
import { Document } from '../../types/document';

interface DocumentsTableProps {
  documents: Document[];
  canWrite: boolean;
  onView: (id: string) => void;
  onEdit: (document: Document) => void;
  onDelete: (id: string) => void;
}

const getCategoryColor = (category: string) => {
  switch (category) {
    case 'Regulatory': return 'red';
    case 'Insurance': return 'blue';
    case 'Contract': return 'purple';
    case 'Certificate': return 'green';
    case 'Policy': return 'yellow';
    default: return 'gray';
  }
};

export const DocumentsTable: React.FC<DocumentsTableProps> = ({
  documents,
  canWrite,
  onView,
  onEdit,
  onDelete
}) => {
  const columns: Column<Document>[] = [
    {
      key: 'title',
      header: 'Title',
      render: (doc) => (
        <div className="flex flex-col">
          <span className="font-medium text-slate-900">{doc.title}</span>
          {doc.description && (
            <span className="text-sm text-text-muted truncate max-w-md">{doc.description}</span>
          )}
        </div>
      )
    },
    {
      key: 'category',
      header: 'Category',
      render: (doc) => <Badge text={doc.category} color={getCategoryColor(doc.category) as any} />
    },
    {
      key: 'currentVersion',
      header: 'Current Version',
      render: (doc) => <Badge text={`v${doc.currentVersionNumber}`} color="blue" />
    },
    {
      key: 'entityLink',
      header: 'Entity Link',
      render: (doc) => doc.entityName ? (
        <span className="text-sm text-slate-600 bg-slate-100 px-2 py-1 rounded">
          {doc.entityName}
        </span>
      ) : (
        <span className="text-text-muted italic text-sm">Standalone</span>
      )
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (doc) => (
        <span className="text-sm text-slate-600">
          {new Date(doc.createdAt).toLocaleDateString()}
        </span>
      )
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (doc) => (
        <div className="flex justify-end gap-2">
          <Button
            variant="secondary"
            onClick={() => onView(doc.id)}
            title="View Details"
          >
            <Eye className="w-4 h-4" />
          </Button>
          
          {canWrite && (
            <>
              <Button
                variant="secondary"
                onClick={() => onEdit(doc)}
                title="Edit Metadata"
              >
                <Edit2 className="w-4 h-4" />
              </Button>
              <Button
                className="bg-red-600 hover:bg-red-700 text-white"
                onClick={() => {
                  if (window.confirm('Are you sure you want to delete this document? Version history will be inaccessible.')) {
                    onDelete(doc.id);
                  }
                }}
                title="Delete Document"
              >
                <Trash2 className="w-4 h-4" />
              </Button>
            </>
          )}
        </div>
      )
    }
  ];

  if (documents.length === 0) {
    return (
      <div className="text-center py-12 bg-white rounded-lg border border-slate-200">
        <FileText className="mx-auto h-12 w-12 text-text-primary" />
        <h3 className="mt-2 text-sm font-medium text-slate-900">No documents found</h3>
        <p className="mt-1 text-sm text-text-muted">
          Adjust your search or filters, or create a new document.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
      <Table columns={columns} data={documents} />
    </div>
  );
};
