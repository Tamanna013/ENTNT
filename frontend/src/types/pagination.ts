export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface PaginationQuery {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
  searchTerm?: string;
}
