import React, { useState } from 'react';
import { AiConversation } from '../../types/aiChat';
import { MessageSquare, MoreVertical, Edit2, Trash2, Plus } from 'lucide-react';

interface ConversationListProps {
  conversations: AiConversation[];
  activeId: string | null;
  onSelect: (id: string) => void;
  onNew: () => void;
  onRename: (id: string, newTitle: string) => void;
  onDelete: (id: string) => void;
}

export const ConversationList: React.FC<ConversationListProps> = ({
  conversations,
  activeId,
  onSelect,
  onNew,
  onRename,
  onDelete
}) => {
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState('');

  const handleEditStart = (e: React.MouseEvent, conv: AiConversation) => {
    e.stopPropagation();
    setEditingId(conv.id);
    setEditTitle(conv.title);
  };

  const handleEditSave = () => {
    if (editingId && editTitle.trim()) {
      onRename(editingId, editTitle.trim());
    }
    setEditingId(null);
  };

  const handleEditKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleEditSave();
    if (e.key === 'Escape') setEditingId(null);
  };

  const handleDelete = (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    if (window.confirm('Are you sure you want to delete this conversation?')) {
      onDelete(id);
    }
  };

  return (
    <div className="flex flex-col h-full bg-slate-50 border-r border-slate-200 w-64 flex-shrink-0">
      <div className="p-4 border-b border-slate-200">
        <button
          onClick={onNew}
          className="w-full flex items-center justify-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-md font-medium transition-colors"
        >
          <Plus size={18} />
          New Chat
        </button>
      </div>
      <div className="flex-1 overflow-y-auto p-3 space-y-1">
        {conversations.length === 0 ? (
          <div className="text-sm text-text-muted text-center py-4">No conversations yet</div>
        ) : (
          conversations.map(conv => (
            <div
              key={conv.id}
              onClick={() => onSelect(conv.id)}
              className={`group flex items-center justify-between px-3 py-2 rounded-md cursor-pointer transition-colors ${ activeId === conv.id ? 'bg-indigo-100 text-indigo-900' : 'hover:bg-slate-200 text-slate-700' }`}
            >
              <div className="flex items-center gap-2 overflow-hidden flex-1">
                <MessageSquare size={16} className="flex-shrink-0 text-text-muted" />
                {editingId === conv.id ? (
                  <input
                    type="text"
                    value={editTitle}
                    onChange={(e) => setEditTitle(e.target.value)}
                    onBlur={handleEditSave}
                    onKeyDown={handleEditKeyDown}
                    autoFocus
                    className="flex-1 text-sm bg-white border border-indigo-300 rounded px-1 outline-none"
                    onClick={(e) => e.stopPropagation()}
                  />
                ) : (
                  <span className="text-sm truncate font-medium">{conv.title}</span>
                )}
              </div>
              
              {editingId !== conv.id && (
                <div className="flex opacity-0 group-hover:opacity-100 transition-opacity">
                  <button 
                    onClick={(e) => handleEditStart(e, conv)}
                    className="p-1 hover:bg-slate-300 rounded text-text-muted"
                    title="Rename"
                  >
                    <Edit2 size={14} />
                  </button>
                  <button 
                    onClick={(e) => handleDelete(e, conv.id)}
                    className="p-1 hover:bg-red-200 rounded text-red-500"
                    title="Delete"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};
