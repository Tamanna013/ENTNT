import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  getConversations, 
  getConversation, 
  createConversation, 
  updateConversation, 
  deleteConversation 
} from '../api/aiChatApi';

export const useConversationsQuery = () => {
  return useQuery({
    queryKey: ['ai-conversations'],
    queryFn: getConversations,
  });
};

export const useConversationQuery = (id: string | null) => {
  return useQuery({
    queryKey: ['ai-conversation', id],
    queryFn: () => getConversation(id as string),
    enabled: !!id,
  });
};

export const useCreateConversationMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createConversation,
    onSuccess: (newConv) => {
      queryClient.invalidateQueries({ queryKey: ['ai-conversations'] });
    },
  });
};

export const useUpdateConversationMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, title }: { id: string; title: string }) => updateConversation(id, { title }),
    onSuccess: (updatedConv) => {
      queryClient.invalidateQueries({ queryKey: ['ai-conversations'] });
      queryClient.invalidateQueries({ queryKey: ['ai-conversation', updatedConv.id] });
    },
  });
};

export const useDeleteConversationMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteConversation,
    onSuccess: (_, deletedId) => {
      queryClient.invalidateQueries({ queryKey: ['ai-conversations'] });
      queryClient.removeQueries({ queryKey: ['ai-conversation', deletedId] });
    },
  });
};
