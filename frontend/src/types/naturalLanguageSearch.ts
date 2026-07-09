export interface NaturalLanguageSearchResult {
  interpretedModule: string | null;
  interpretedFilters: Record<string, unknown>;
  results: unknown;
  isAvailable: boolean;
  message: string | null;
}
