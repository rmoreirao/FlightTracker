import { useState } from 'react';
import { flightApi, ApiError } from '@/lib/api';
import { 
  FlightSearchFormValues, 
  FlightSearchResults, 
  convertApiFlightToFlightOption,
  convertCabinClassToApi 
} from '@/lib/schemas';
import { FlightSearchParams } from '@/lib/api-types';

interface UseFlightSearchResult {
  searchResults: FlightSearchResults | null;
  isLoading: boolean;
  error: string | null;
  searchFlights: (formData: FlightSearchFormValues) => Promise<void>;
  clearResults: () => void;
  clearError: () => void;
}

export const useFlightSearch = (): UseFlightSearchResult => {
  const [searchResults, setSearchResults] = useState<FlightSearchResults | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const searchFlights = async (formData: FlightSearchFormValues) => {
    setIsLoading(true);
    setError(null);

    try {
      // Convert form data to API parameters
      const searchParams: FlightSearchParams = {
        originCode: formData.originCode,
        destinationCode: formData.destinationCode,
        departureDate: formData.departureDate,
        returnDate: formData.returnDate || undefined,
        cabins: convertCabinClassToApi(formData.cabinClass),
        adults: formData.adults,
        children: formData.children,
        infants: formData.infants,
      };

      const apiResult = await flightApi.searchFlights(searchParams);

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
    } finally {
      setIsLoading(false);
    }
  };

  const clearResults = () => {
    setSearchResults(null);
    setError(null);
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
  };
};