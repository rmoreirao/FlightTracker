'use client';

import { Suspense, useState } from 'react';
import { FlightSearchForm } from '@/components/FlightSearchForm';
import { FlightResults } from '@/components/FlightResults';
import { FlightSearchFormValues } from '@/lib/schemas';
import { useFlightSearch } from '@/hooks/useFlightSearch';

function FlightSearchContent() {
  const [expandedFlightId, setExpandedFlightId] = useState<string | null>(null);
  
  const { 
    searchResults, 
    isLoading, 
    error, 
    searchFlights, 
    clearError,
    searchState,
    paginationInfo,
    setSorting,
    setPage,
    setPageSize,
    isSorting,
    isPaging
  } = useFlightSearch();

  const handleSearch = async (searchData: FlightSearchFormValues) => {
    clearError();
    setExpandedFlightId(null); // Close any expanded details when searching
    await searchFlights(searchData);
  };

  const handleFlightExpand = (flightId: string) => {
    setExpandedFlightId(expandedFlightId === flightId ? null : flightId);
  };

  return (
    <div className="space-y-8">
      <FlightSearchForm 
        onSearch={handleSearch} 
        isLoading={isLoading}
      />
      
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <div className="flex items-center">
            <div className="text-red-800">
              <h3 className="text-sm font-medium">Search Error</h3>
              <p className="text-sm mt-1">{error}</p>
            </div>
            <button
              onClick={clearError}
              className="ml-auto text-red-500 hover:text-red-700"
            >
              <span className="sr-only">Dismiss</span>
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
              </svg>
            </button>
          </div>
        </div>
      )}
      
      {searchResults && paginationInfo && (
        <FlightResults
          results={searchResults.results}
          lastUpdated={searchResults.lastUpdated}
          sortBy={searchState.sortBy}
          sortDirection={searchState.sortDirection}
          onSort={setSorting}
          isSorting={isSorting}
          paginationInfo={paginationInfo}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          isPaging={isPaging}
          expandedFlightId={expandedFlightId}
          onFlightExpand={handleFlightExpand}
        />
      )}
    </div>
  );
}

export function FlightSearchWrapper() {
  return (
    <Suspense fallback={
      <div className="space-y-8">
        <div className="bg-neutral-100 rounded-lg shadow-sm p-6">
          <div className="animate-pulse">
            <div className="h-8 bg-neutral-300 rounded w-1/3 mb-4"></div>
            <div className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="h-12 bg-neutral-300 rounded"></div>
                <div className="h-12 bg-neutral-300 rounded"></div>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="h-12 bg-neutral-300 rounded"></div>
                <div className="h-12 bg-neutral-300 rounded"></div>
              </div>
              <div className="h-12 bg-neutral-300 rounded"></div>
            </div>
          </div>
        </div>
      </div>
    }>
      <FlightSearchContent />
    </Suspense>
  );
}
