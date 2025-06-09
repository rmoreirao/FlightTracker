'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FlightSearchFormValues, flightSearchSchema } from '@/lib/schemas';
import { MagnifyingGlassIcon, CalendarIcon, MapPinIcon } from '@heroicons/react/24/outline';
import { useState } from 'react';

interface FlightSearchFormProps {
  onSearch: (searchData: FlightSearchFormValues) => void;
  isLoading?: boolean;
}

export function FlightSearchForm({ onSearch, isLoading = false }: FlightSearchFormProps) {
  const [tripType, setTripType] = useState<'roundtrip' | 'oneway'>('roundtrip');
  
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
    setValue,
  } = useForm<FlightSearchFormValues>({
    resolver: zodResolver(flightSearchSchema),
    mode: 'onChange',
  });

  const onSubmit = (data: FlightSearchFormValues) => {
    // Clear return date if one-way trip
    if (tripType === 'oneway') {
      data.returnDate = '';
    }
    onSearch(data);
  };

  const handleTripTypeChange = (newTripType: 'roundtrip' | 'oneway') => {
    setTripType(newTripType);
    if (newTripType === 'oneway') {
      setValue('returnDate', '');
    }
  };

  return (
    <div className="bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-4 md:p-6">
      <div className="mb-6">
        <h1 className="text-4xl font-bold text-neutral-900 dark:text-neutral-900 mb-2">
          Find Your Flight
        </h1>
        <p className="text-neutral-700 dark:text-neutral-700">
          Search current prices and explore historical data
        </p>
      </div>

      {/* Trip Type Toggle */}
      <div className="flex gap-4 mb-6">
        <button
          type="button"
          onClick={() => handleTripTypeChange('roundtrip')}
          className={`px-4 py-2 rounded-md font-medium transition-colors ${
            tripType === 'roundtrip'
              ? 'bg-primary-500 text-white'
              : 'bg-neutral-50 text-neutral-700 hover:bg-neutral-100 dark:bg-neutral-50 dark:text-neutral-700'
          }`}
        >
          Round Trip
        </button>
        <button
          type="button"
          onClick={() => handleTripTypeChange('oneway')}
          className={`px-4 py-2 rounded-md font-medium transition-colors ${
            tripType === 'oneway'
              ? 'bg-primary-500 text-white'
              : 'bg-neutral-50 text-neutral-700 hover:bg-neutral-100 dark:bg-neutral-50 dark:text-neutral-700'
          }`}
        >
          One Way
        </button>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Origin and Destination */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-neutral-900 dark:text-neutral-900 mb-2">
              <MapPinIcon className="w-4 h-4 inline mr-1" />
              From
            </label>
            <input
              {...register('originCode')}
              type="text"
              placeholder="LAX"
              className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 uppercase"
              maxLength={3}
            />
            {errors.originCode && (
              <p className="mt-1 text-sm text-error-500">{errors.originCode.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-900 dark:text-neutral-900 mb-2">
              <MapPinIcon className="w-4 h-4 inline mr-1" />
              To
            </label>
            <input
              {...register('destinationCode')}
              type="text"
              placeholder="JFK"
              className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 uppercase"
              maxLength={3}
            />
            {errors.destinationCode && (
              <p className="mt-1 text-sm text-error-500">{errors.destinationCode.message}</p>
            )}
          </div>
        </div>

        {/* Departure and Return Dates */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-neutral-900 dark:text-neutral-900 mb-2">
              <CalendarIcon className="w-4 h-4 inline mr-1" />
              Departure Date
            </label>
            <input
              {...register('departureDate')}
              type="date"
              className="w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              min={new Date().toISOString().split('T')[0]}
            />
            {errors.departureDate && (
              <p className="mt-1 text-sm text-error-500">{errors.departureDate.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-neutral-900 dark:text-neutral-900 mb-2">
              <CalendarIcon className="w-4 h-4 inline mr-1" />
              Return Date {tripType === 'oneway' && <span className="text-neutral-500">(Optional)</span>}
            </label>
            <input
              {...register('returnDate')}
              type="date"
              disabled={tripType === 'oneway'}
              className={`w-full px-3 py-2 border border-neutral-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 ${
                tripType === 'oneway' ? 'bg-neutral-100 text-neutral-400' : ''
              }`}
              min={watch('departureDate') || new Date().toISOString().split('T')[0]}
            />
            {errors.returnDate && (
              <p className="mt-1 text-sm text-error-500">{errors.returnDate.message}</p>
            )}
          </div>
        </div>

        {/* Search Button */}
        <button
          type="submit"
          disabled={isLoading}
          className="w-full bg-primary-500 hover:bg-primary-600 disabled:bg-neutral-400 text-white font-medium py-3 px-4 rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2"
        >
          {isLoading ? (
            <div className="flex items-center justify-center">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
              Searching...
            </div>
          ) : (
            <div className="flex items-center justify-center">
              <MagnifyingGlassIcon className="w-5 h-5 mr-2" />
              Search Flights
            </div>
          )}
        </button>
      </form>
    </div>
  );
}
