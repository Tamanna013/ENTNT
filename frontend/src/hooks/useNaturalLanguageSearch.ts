import { useMutation } from '@tanstack/react-query';
import { aiSearchApi } from '../api/aiSearchApi';
import { NaturalLanguageSearchResult } from '../types/naturalLanguageSearch';

export const useNaturalLanguageSearch = () => {
  return useMutation<NaturalLanguageSearchResult, Error, string>({
    mutationFn: (query: string) => aiSearchApi.search(query),
  });
};
