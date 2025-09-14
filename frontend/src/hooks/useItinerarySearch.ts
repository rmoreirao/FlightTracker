import { useState } from 'react';
import { flightApi, ApiError } from '@/lib/api';
import { ItinerarySearchParams } from '@/lib/api-types';
import { buildItinerarySearchResults, ItinerarySearchResultsUI } from '@/lib/itinerary-utils';

export interface UseItinerarySearchResult {
  results: ItinerarySearchResultsUI | null;
  isLoading: boolean;
  error: string | null;
  search: (params: Omit<ItinerarySearchParams, 'page' | 'pageSize' | 'sortBy' | 'sortOrder'>) => Promise<void>;
  setPage: (page: number) => Promise<void>;
  setSort: (sortBy: 'price' | 'duration') => Promise<void>;
  toggleSortOrder: () => Promise<void>;
  page: number;
  pageSize: number;
  sortBy: 'price' | 'duration';
  sortOrder: 'asc' | 'desc';
  isPaging: boolean;
  isSorting: boolean;
}

const DEFAULT_PAGE_SIZE = 20;

export function useItinerarySearch(): UseItinerarySearchResult {
  const [results, setResults] = useState<ItinerarySearchResultsUI | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPageState] = useState(1);
  const [pageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sortBy, setSortBy] = useState<'price' | 'duration'>('price');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [baseParams, setBaseParams] = useState<Omit<ItinerarySearchParams, 'page' | 'pageSize' | 'sortBy' | 'sortOrder'> | null>(null);
  const [isPaging, setIsPaging] = useState(false);
  const [isSorting, setIsSorting] = useState(false);

  async function execute(params: ItinerarySearchParams, opts?: { transient?: boolean }) {
    try {
      if (!opts?.transient) setIsLoading(true);
      const apiResult = await flightApi.searchItineraries(params);
      setResults(buildItinerarySearchResults(apiResult));
    } catch (err) {
      let message = 'Failed to search itineraries.';
      if (err instanceof ApiError) {
        if (err.status === 400) message = 'Invalid search parameters.';
        else if (err.status === 429) message = 'Rate limit exceeded. Try again later.';
        else if (err.status === 500) message = 'Server error. Please retry.';
        else message = err.message;
      } else if (err instanceof Error) {
        message = err.message;
      }
      setError(message);
    } finally {
      if (!opts?.transient) setIsLoading(false);
    }
  }

  async function search(params: Omit<ItinerarySearchParams, 'page' | 'pageSize' | 'sortBy' | 'sortOrder'>) {
    setError(null);
    setBaseParams(params);
    setPageState(1);
    await execute({ ...params, page: 1, pageSize, sortBy, sortOrder });
  }

  async function setPage(pageNumber: number) {
    if (!baseParams) return;
    setIsPaging(true);
    try {
      setPageState(pageNumber);
      await execute({ ...baseParams, page: pageNumber, pageSize, sortBy, sortOrder }, { transient: true });
    } finally {
      setIsPaging(false);
    }
  }

  async function setSort(newSort: 'price' | 'duration') {
    if (!baseParams) return;
    setIsSorting(true);
    try {
      setSortBy(newSort);
      setPageState(1);
      await execute({ ...baseParams, page: 1, pageSize, sortBy: newSort, sortOrder });
    } finally {
      setIsSorting(false);
    }
  }

  async function toggleSortOrder() {
    if (!baseParams) return;
    setIsSorting(true);
    try {
      const newOrder = sortOrder === 'asc' ? 'desc' : 'asc';
      setSortOrder(newOrder);
      setPageState(1);
      await execute({ ...baseParams, page: 1, pageSize, sortBy, sortOrder: newOrder });
    } finally {
      setIsSorting(false);
    }
  }

  return {
    results,
    isLoading,
    error,
    search,
    setPage,
    setSort,
    toggleSortOrder,
    page,
    pageSize,
    sortBy,
    sortOrder,
    isPaging,
    isSorting,
  };
}
