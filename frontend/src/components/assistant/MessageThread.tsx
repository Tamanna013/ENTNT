import React, { useEffect, useRef } from 'react';
import { AiChatMessage } from '../../types/aiChat';
import { Bot, User } from 'lucide-react';
import ReactMarkdown from 'react-markdown';

interface MessageThreadProps {
  messages: AiChatMessage[];
  streamingMessage: string | null;
}

export const MessageThread: React.FC<MessageThreadProps> = ({ messages, streamingMessage }) => {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, streamingMessage]);

  if (messages.length === 0 && !streamingMessage) {
    return (
      <div className="flex-1 flex items-center justify-center bg-white p-6">
        <div className="text-center max-w-md">
          <div className="bg-indigo-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 text-indigo-600">
            <Bot size={32} />
          </div>
          <h2 className="text-2xl font-bold text-slate-800 mb-2">How can I help you today?</h2>
          <p className="text-text-muted">Ask about fleet performance, maintenance schedules, or cargo risk analysis.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto bg-white p-6">
      <div className="max-w-4xl mx-auto space-y-6">
        {messages.map((msg, idx) => (
          <div key={msg.id || idx} className={`flex gap-4 ${msg.role === 'User' ? 'justify-end' : 'justify-start'}`}>
            {msg.role === 'Assistant' && (
              <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center flex-shrink-0 mt-1">
                <Bot size={18} className="text-indigo-700" />
              </div>
            )}
            
            <div className={`px-5 py-3 rounded-2xl max-w-[85%] ${ msg.role === 'User' ? 'bg-indigo-600 text-text-primary rounded-br-none' : 'bg-slate-100 text-slate-800 rounded-bl-none' }`}>
              {msg.role === 'User' ? (
                <div className="whitespace-pre-wrap">{msg.content}</div>
              ) : (
                <div className="prose prose-sm max-w-none dark:prose-invert">
                  <ReactMarkdown>{msg.content}</ReactMarkdown>
                </div>
              )}
            </div>

            {msg.role === 'User' && (
              <div className="w-8 h-8 rounded-full bg-slate-200 flex items-center justify-center flex-shrink-0 mt-1">
                <User size={18} className="text-slate-600" />
              </div>
            )}
          </div>
        ))}
        
        {streamingMessage && (
          <div className="flex gap-4 justify-start">
            <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center flex-shrink-0 mt-1">
              <Bot size={18} className="text-indigo-700" />
            </div>
            <div className="px-5 py-3 rounded-2xl max-w-[85%] bg-slate-100 text-slate-800 rounded-bl-none">
              <div className="prose prose-sm max-w-none">
                <ReactMarkdown>{streamingMessage}</ReactMarkdown>
              </div>
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>
    </div>
  );
};
