import { apiClient } from './client';
import { NaturalLanguageSearchResult } from '../types/naturalLanguageSearch';

export const aiSearchApi = {
  search: async (query: string): Promise<NaturalLanguageSearchResult> => {
    const response = await apiClient.post<NaturalLanguageSearchResult>('/ai/natural-language-search', { query });
    return response.data;
  }
};
