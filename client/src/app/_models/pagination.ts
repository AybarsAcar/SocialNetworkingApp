/* 
this has to match inside the response header we are sending from our api
*/
export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

/* 
T will be an iterable, array of objects i.e. Member[]
*/
export class PaginatedResult<T> {
  result: T;
  pagination: Pagination;
}
