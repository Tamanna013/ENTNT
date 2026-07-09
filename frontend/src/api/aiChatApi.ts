import { apiClient as client } from './client';
import { AiConversation, UpdateConversationDto } from '../types/aiChat';

export const getConversations = async (): Promise<AiConversation[]> => {
  const response = await client.get<AiConversation[]>('/ai/conversations');
  return response.data;
};

export const getConversation = async (id: string): Promise<AiConversation> => {
  const response = await client.get<AiConversation>(`/ai/conversations/${id}`);
  return response.data;
};

export const createConversation = async (): Promise<AiConversation> => {
  const response = await client.post<AiConversation>('/ai/conversations');
  return response.data;
};

export const updateConversation = async (id: string, dto: UpdateConversationDto): Promise<AiConversation> => {
  const response = await client.put<AiConversation>(`/ai/conversations/${id}`, dto);
  return response.data;
};

export const deleteConversation = async (id: string): Promise<void> => {
  await client.delete(`/ai/conversations/${id}`);
};

export const streamMessage = (
  id: string,
  message: string,
  onChunk: (chunk: string) => void,
  onError: (error: string) => void,
  onComplete: () => void
) => {
  // Using native fetch for SSE stream reading because axios doesn't support streams cleanly in browser
  const token = localStorage.getItem('token');
  
  // Create AbortController to be able to stop fetch if needed
  const controller = new AbortController();

  fetch(`/api/v1/ai/conversations/${id}/messages`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify({ message }),
    signal: controller.signal
  }).then(async (response) => {
    if (response.status === 429) {
      onError("You've reached the AI usage limit for this hour. Please try again later.");
      onComplete();
      return;
    }
    
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }

    const reader = response.body?.getReader();
    const decoder = new TextDecoder('utf-8');
    
    if (!reader) return;

    let buffer = '';
    
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      
      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n\n');
      
      // keep the last chunk if it's not complete
      buffer = lines.pop() || '';
      
      for (const line of lines) {
        if (line.startsWith('data: ')) {
          try {
            const data = JSON.parse(line.substring(6));
            if (data.error) {
              onError(data.error);
            } else if (data.content) {
              // the chunk is safe, unescape newlines if needed
              const unescaped = data.content.replace(/\\n/g, '\n');
              onChunk(unescaped);
            }
          } catch (e) {
            console.error('Error parsing SSE chunk', e);
          }
        }
      }
    }
    onComplete();
  }).catch(err => {
    if (err.name !== 'AbortError') {
      onError(err.message || 'An error occurred connecting to the AI assistant.');
      onComplete();
    }
  });

  return controller;
};
