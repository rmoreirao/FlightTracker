'use client';

import { useState } from 'react';
import { FlightSearchForm } from '@/components/FlightSearchForm';
import { FlightResults } from '@/components/FlightResults';
import { Navbar } from '@/components/Navbar';
import { FlightSearchFormValues, FlightOption, FlightSearchResults } from '@/lib/schemas';

// Mock data for demonstration (replace with actual API calls)
const mockFlightResults: FlightOption[] = [
  {
    id: '1',
    airlineCode: 'AA',
    airlineName: 'American Airlines',
    totalPriceCents: 45999,
    currency: 'USD',
    stops: 0,
    durationMinutes: 360,
    departureTime: '2025-06-15T08:00:00Z',
    arrivalTime: '2025-06-15T14:00:00Z',
    bookingUrl: 'https://example.com/book/1',
    cabinClass: 'economy'
  },
  {
    id: '2',
    airlineCode: 'DL',
    airlineName: 'Delta Air Lines',
    totalPriceCents: 52999,
    currency: 'USD',
    stops: 1,
    durationMinutes: 420,
    departureTime: '2025-06-15T06:30:00Z',
    arrivalTime: '2025-06-15T13:30:00Z',
    bookingUrl: 'https://example.com/book/2',
    cabinClass: 'economy'
  },
  {
    id: '3',
    airlineCode: 'UA',
    airlineName: 'United Airlines',
    totalPriceCents: 41999,
    currency: 'USD',
    stops: 0,
    durationMinutes: 345,
    departureTime: '2025-06-15T14:00:00Z',
    arrivalTime: '2025-06-15T19:45:00Z',
    bookingUrl: 'https://example.com/book/3',
    cabinClass: 'economy'
  }
];

export default function Home() {
  const [searchResults, setSearchResults] = useState<FlightSearchResults | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [sortBy, setSortBy] = useState<'price' | 'duration' | 'stops'>('price');

  const handleSearch = async (searchData: FlightSearchFormValues) => {
    setIsLoading(true);
    
    // Simulate API call delay
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // Mock search results (replace with actual API call)
    // In a real implementation, you would use searchData to make the API call
    console.log('Search data:', searchData);
    const results: FlightSearchResults = {
      results: mockFlightResults,
      lastUpdated: new Date().toISOString(),
      searchId: 'mock-search-' + Date.now()
    };
    
    setSearchResults(results);
    setIsLoading(false);
  };

  const handleSort = (newSortBy: 'price' | 'duration' | 'stops') => {
    setSortBy(newSortBy);
    
    if (searchResults) {
      const sortedResults = [...searchResults.results].sort((a, b) => {
        switch (newSortBy) {
          case 'price':
            return a.totalPriceCents - b.totalPriceCents;
          case 'duration':
            return a.durationMinutes - b.durationMinutes;
          case 'stops':
            return a.stops - b.stops;
          default:
            return 0;
        }
      });
      
      setSearchResults({
        ...searchResults,
        results: sortedResults
      });
    }
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
