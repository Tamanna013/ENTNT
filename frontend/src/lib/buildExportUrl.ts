export function buildExportUrl(baseUrl: string, filters: Record<string, unknown>, format: "csv" | "xlsx"): string {
  const queryParams = new URLSearchParams();
  queryParams.append('format', format);

  for (const [key, value] of Object.entries(filters)) {
    // Exclude pagination parameters
    if (key === 'pageNumber' || key === 'pageSize') {
      continue;
    }

    // Exclude empty/null values
    if (value === undefined || value === null || value === '') {
      continue;
    }

    queryParams.append(key, String(value));
  }

  return `${baseUrl}?${queryParams.toString()}`;
}
