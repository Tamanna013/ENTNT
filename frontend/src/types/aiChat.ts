export interface AiChatMessage {
  id: string;
  role: 'User' | 'Assistant' | 'System';
  content: string;
  createdAt: string;
}

export interface AiConversation {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
  messages: AiChatMessage[];
}

export interface UpdateConversationDto {
  title: string;
}
