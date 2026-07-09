export interface AiUsageReportRow {
  userId: string;
  userName: string;
  requestCount: number;
  successCount: number;
  totalTokensUsed: number | null;
}
