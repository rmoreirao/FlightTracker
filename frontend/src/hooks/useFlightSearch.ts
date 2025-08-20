import { useState, useCallback } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { flightApi, ApiError } from '@/lib/api';
import { 
  FlightSearchFormValues, 
  FlightSearchResults, 
  convertApiFlightToFlightOption,
  convertCabinClassToApi,
  SearchState,
  SortOption,
  SortDirection,
  PaginationInfo,
  calculatePaginationInfo
} from '@/lib/schemas';
import { FlightSearchParams } from '@/lib/api-types';

interface UseFlightSearchResult {
  searchResults: FlightSearchResults | null;
  isLoading: boolean;
  error: string | null;
  searchFlights: (formData: FlightSearchFormValues) => Promise<void>;
  clearResults: () => void;
  clearError: () => void;
  
  // Search state
  searchState: SearchState;
  lastSearchParams: FlightSearchFormValues | null;
  
  // Pagination info
  paginationInfo: PaginationInfo | null;
  
  // Sort and pagination methods
  setSorting: (sortBy: SortOption, direction?: SortDirection) => Promise<void>;
  setPage: (page: number) => Promise<void>;
  setPageSize: (size: number) => Promise<void>;
  
  // Loading states
  isSorting: boolean;
  isPaging: boolean;
}

const DEFAULT_SEARCH_STATE: SearchState = {
  sortBy: 'departureTime',
  sortDirection: 'asc',
  page: 1,
  pageSize: 20,
};

export const useFlightSearch = (): UseFlightSearchResult => {
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const [searchResults, setSearchResults] = useState<FlightSearchResults | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastSearchParams, setLastSearchParams] = useState<FlightSearchFormValues | null>(null);
  const [isSorting, setIsSorting] = useState(false);
  const [isPaging, setIsPaging] = useState(false);
  
  // Initialize search state from URL or defaults
  const initializeSearchState = (): SearchState => {
    return {
      sortBy: (searchParams.get('sortBy') as SortOption) || DEFAULT_SEARCH_STATE.sortBy,
      sortDirection: (searchParams.get('sortDirection') as SortDirection) || DEFAULT_SEARCH_STATE.sortDirection,
      page: parseInt(searchParams.get('page') || '1'),
      pageSize: parseInt(searchParams.get('pageSize') || '20'),
    };
  };
  
  const [searchState, setSearchState] = useState<SearchState>(initializeSearchState);
  
  // Calculate pagination info
  const paginationInfo = searchResults ? calculatePaginationInfo(
    searchResults.totalResults,
    searchState.page,
    searchState.pageSize
  ) : null;

  // Update URL with current search state
  const updateUrlParams = useCallback((newState: Partial<SearchState>, formData?: FlightSearchFormValues) => {
    const params = new URLSearchParams(searchParams.toString());
    
    // Update search state params
    if (newState.sortBy) params.set('sortBy', newState.sortBy);
    if (newState.sortDirection) params.set('sortDirection', newState.sortDirection);
    if (newState.page) params.set('page', newState.page.toString());
    if (newState.pageSize) params.set('pageSize', newState.pageSize.toString());
    
    // Update search form params if provided
    if (formData) {
      params.set('origin', formData.originCode);
      params.set('destination', formData.destinationCode);
      params.set('departure', formData.departureDate);
      if (formData.returnDate) params.set('return', formData.returnDate);
      params.set('cabin', formData.cabinClass);
      params.set('adults', formData.adults.toString());
      params.set('children', formData.children.toString());
      params.set('infants', formData.infants.toString());
    }
    
    router.push(`?${params.toString()}`, { scroll: false });
  }, [router, searchParams]);

  const performSearch = async (
    formData: FlightSearchFormValues, 
    searchStateOverrides: Partial<SearchState> = {}
  ): Promise<void> => {
    const currentSearchState = { ...searchState, ...searchStateOverrides };
    
    try {
      // Convert form data to API parameters
      const searchApiParams: FlightSearchParams = {
        originCode: formData.originCode,
        destinationCode: formData.destinationCode,
        departureDate: formData.departureDate,
        returnDate: formData.returnDate || undefined,
        cabins: convertCabinClassToApi(formData.cabinClass),
        adults: formData.adults,
        children: formData.children,
        infants: formData.infants,
        sortBy: currentSearchState.sortBy,
        sortOrder: currentSearchState.sortDirection,
        page: currentSearchState.page,
        pageSize: currentSearchState.pageSize,
      };

      const apiResult = await flightApi.searchFlights(searchApiParams);

      // Convert API response to frontend format
      const flightOptions = apiResult.flights?.map(convertApiFlightToFlightOption) || [];

      const results: FlightSearchResults = {
        results: flightOptions,
        lastUpdated: apiResult.lastUpdated,
        totalResults: apiResult.totalResults,
        currency: apiResult.currency || 'USD',
        searchDuration: apiResult.searchDuration,
      };

      setSearchResults(results);
      setLastSearchParams(formData);
      
      // Update search state
      setSearchState(currentSearchState);
      
      // Update URL
      updateUrlParams(currentSearchState, formData);
      
    } catch (err) {
      let errorMessage = 'An unexpected error occurred while searching for flights.';

      if (err instanceof ApiError) {
        if (err.status === 400) {
          errorMessage = 'Please check your search parameters and try again.';
          if (err.details && 'errors' in err.details && err.details.errors) {
            // Handle validation errors
            const validationErrors = Object.values(err.details.errors).flat();
            errorMessage = validationErrors.join(' ');
          }
        } else if (err.status === 429) {
          errorMessage = 'Too many requests. Please wait a moment and try again.';
        } else if (err.status === 500) {
          errorMessage = 'Server error. Please try again later.';
        } else {
          errorMessage = err.message;
        }
      } else if (err instanceof Error) {
        errorMessage = err.message;
      }

      setError(errorMessage);
      console.error('Flight search error:', err);
    }
  };

  const searchFlights = async (formData: FlightSearchFormValues): Promise<void> => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Reset to first page for new searches
      await performSearch(formData, { page: 1 });
    } finally {
      setIsLoading(false);
    }
  };

  const setSorting = async (sortBy: SortOption, direction?: SortDirection): Promise<void> => {
    if (!lastSearchParams) return;
    
    setIsSorting(true);
    setError(null);
    
    try {
      const newDirection = direction || (searchState.sortBy === sortBy && searchState.sortDirection === 'asc' ? 'desc' : 'asc');
      await performSearch(lastSearchParams, { 
        sortBy, 
        sortDirection: newDirection,
        page: 1 // Reset to first page when sorting
      });
    } finally {
      setIsSorting(false);
    }
  };

  const setPage = async (page: number): Promise<void> => {
    if (!lastSearchParams) return;
    
    setIsPaging(true);
    setError(null);
    
    try {
      await performSearch(lastSearchParams, { page });
    } finally {
      setIsPaging(false);
    }
  };

  const setPageSize = async (pageSize: number): Promise<void> => {
    if (!lastSearchParams) return;
    
    setIsPaging(true);
    setError(null);
    
    try {
      await performSearch(lastSearchParams, { 
        pageSize,
        page: 1 // Reset to first page when changing page size
      });
    } finally {
      setIsPaging(false);
    }
  };

  const clearResults = () => {
    setSearchResults(null);
    setError(null);
    setLastSearchParams(null);
    setSearchState(DEFAULT_SEARCH_STATE);
  };

  const clearError = () => {
    setError(null);
  };

  return {
    searchResults,
    isLoading,
    error,
    searchFlights,
    clearResults,
    clearError,
    searchState,
    lastSearchParams,
    paginationInfo,
    setSorting,
    setPage,
    setPageSize,
    isSorting,
    isPaging,
  };
};