'use client';

import { Suspense } from 'react';
import { FlightSearchForm } from '@/components/FlightSearchForm';
import { useItinerarySearch } from '@/hooks/useItinerarySearch';
import { ItineraryResults } from '@/components/ItineraryResults';
import { FlightSearchFormValues } from '@/lib/schemas';

function ItinerarySearchContent() {
  const {
    results,
    isLoading,
    error,
    search,
    setPage,
    setSort,
    toggleSortOrder,
    page,
    sortBy,
    sortOrder,
    isPaging,
    isSorting,
  } = useItinerarySearch();

  const handleSearch = async (data: FlightSearchFormValues) => {
    await search({
      originCode: data.originCode,
      destinationCode: data.destinationCode,
      departureDate: data.departureDate,
      returnDate: data.returnDate || undefined,
    });
  };

  return (
    <div className="space-y-8">
      <FlightSearchForm onSearch={handleSearch} isLoading={isLoading} />
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-800">{error}</div>
      )}
      {results && (
        <ItineraryResults
          results={results.results}
          page={page}
            hasNextPage={results.hasNextPage}
          onNext={() => setPage(page + 1)}
          onPrev={() => setPage(Math.max(1, page - 1))}
          isPaging={isPaging}
          sortBy={sortBy}
          sortOrder={sortOrder}
          onSortChange={setSort}
          onToggleSortOrder={toggleSortOrder}
          isSorting={isSorting}
        />
      )}
    </div>
  );
}

export function ItinerarySearchWrapper() {
  return (
    <Suspense fallback={<div className="bg-neutral-100 rounded-lg p-6 animate-pulse h-64"/>}>
      <ItinerarySearchContent />
    </Suspense>
  );
}
