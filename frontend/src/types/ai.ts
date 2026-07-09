export interface AiSummaryResult {
    summary: string;
    isAvailable: boolean;
    generatedAt: string;
}

export interface AiRecommendationResult {
  recommendations: string[];
  isAvailable: boolean;
  generatedAt: string;
  disclaimer: string;
}
