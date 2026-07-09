import React, { useState, useEffect } from 'react';
import { 
  useConversationsQuery, 
  useConversationQuery, 
  useCreateConversationMutation,
  useUpdateConversationMutation,
  useDeleteConversationMutation
} from '../../hooks/useAiChat';
import { streamMessage } from '../../api/aiChatApi';
import { ConversationList } from '../../components/assistant/ConversationList';
import { MessageThread } from '../../components/assistant/MessageThread';
import { ChatInput } from '../../components/assistant/ChatInput';

export const AiAssistantPage: React.FC = () => {
  const [activeId, setActiveId] = useState<string | null>(null);
  const [streamingContent, setStreamingContent] = useState<string | null>(null);
  const [isStreaming, setIsStreaming] = useState(false);
  const [streamError, setStreamError] = useState<string | null>(null);

  const { data: conversations = [], isLoading: isLoadingList } = useConversationsQuery();
  const { data: activeConversation, refetch: refetchActive } = useConversationQuery(activeId);
  
  const createConv = useCreateConversationMutation();
  const updateConv = useUpdateConversationMutation();
  const deleteConv = useDeleteConversationMutation();

  // Select first conversation if none selected
  useEffect(() => {
    if (!activeId && conversations.length > 0 && !isLoadingList) {
      setActiveId(conversations[0].id);
    }
  }, [conversations, activeId, isLoadingList]);

  const handleNewChat = async () => {
    const newConv = await createConv.mutateAsync();
    setActiveId(newConv.id);
  };

  const handleSend = async (text: string) => {
    if (!text.trim()) return;
    
    let targetId = activeId;
    if (!targetId) {
      const newConv = await createConv.mutateAsync();
      targetId = newConv.id;
      setActiveId(targetId);
      // Wait for React to mount the active conversation
      await new Promise(resolve => setTimeout(resolve, 100));
    }

    setIsStreaming(true);
    setStreamingContent('');
    setStreamError(null);

    // Optimistically refetch to show user message quickly
    setTimeout(() => {
      refetchActive();
    }, 500);

    streamMessage(
      targetId!, 
      text,
      (chunk) => setStreamingContent(prev => (prev || '') + chunk),
      (err) => setStreamError(err),
      () => {
        setIsStreaming(false);
        setStreamingContent(null);
        refetchActive();
      }
    );
  };

  return (
    <div className="flex h-[calc(100vh-64px)] overflow-hidden">
      <ConversationList
        conversations={conversations}
        activeId={activeId}
        onSelect={setActiveId}
        onNew={handleNewChat}
        onRename={(id, title) => updateConv.mutate({ id, title })}
        onDelete={(id) => {
          deleteConv.mutate(id);
          if (activeId === id) setActiveId(null);
        }}
      />
      <div className="flex flex-col flex-1 min-w-0 bg-white">
        <MessageThread 
          messages={activeConversation?.messages || []}
          streamingMessage={streamingContent}
        />
        <ChatInput 
          onSend={handleSend}
          disabled={isStreaming}
          error={streamError}
        />
      </div>
    </div>
  );
};

export default AiAssistantPage;
