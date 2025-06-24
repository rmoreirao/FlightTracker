'use client';

import { useState } from 'react';
import { FlightSearchForm } from '@/components/FlightSearchForm';
import { FlightResults } from '@/components/FlightResults';
import { Navbar } from '@/components/Navbar';
import { FlightSearchFormValues } from '@/lib/schemas';
import { useFlightSearch } from '@/hooks/useFlightSearch';

export default function Home() {
  const { searchResults, isLoading, error, searchFlights, clearError } = useFlightSearch();
  const [sortBy, setSortBy] = useState<'price' | 'duration' | 'stops'>('price');

  const handleSearch = async (searchData: FlightSearchFormValues) => {
    clearError();
    await searchFlights(searchData);
  };
  const handleSort = (newSortBy: 'price' | 'duration' | 'stops') => {
    setSortBy(newSortBy);
    // Note: Sorting would need to be implemented in the hook or state management
    // For now, just update the sort preference
  };
  return (
    <div className="min-h-screen bg-neutral-50 dark:bg-neutral-50">
      <Navbar />
      
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
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
          
          {searchResults && (
            <FlightResults
              results={searchResults.results}
              lastUpdated={searchResults.lastUpdated}
              onSort={handleSort}
              currentSort={sortBy}
            />
          )}
        </div>
      </main>
    </div>
  );
}
