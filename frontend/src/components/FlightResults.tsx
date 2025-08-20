'use client';

import { FlightOption, SortOption, SortDirection, PaginationInfo } from '@/lib/schemas';
import { ClockIcon, ArrowRightIcon, ArrowTopRightOnSquareIcon, ArrowUpIcon, ArrowDownIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';
import { formatPrice, formatDuration } from '@/lib/api';
import { Pagination } from './Pagination';

interface FlightResultsProps {
  results: FlightOption[];
  lastUpdated: string;
  
  // Sorting props
  sortBy: SortOption;
  sortDirection: SortDirection;
  onSort: (sortBy: SortOption, direction?: SortDirection) => void;
  isSorting: boolean;
  
  // Pagination props
  paginationInfo: PaginationInfo;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  isPaging: boolean;
}

export function FlightResults({ 
  results, 
  lastUpdated, 
  sortBy,
  sortDirection,
  onSort, 
  isSorting,
  paginationInfo,
  onPageChange,
  onPageSizeChange,
  isPaging
}: FlightResultsProps) {
  const formatStops = (stops: number) => {
    if (stops === 0) return 'Nonstop';
    if (stops === 1) return '1 Stop';
    return `${stops} Stops`;
  };

  const formatTime = (timeString: string) => {
    return format(new Date(timeString), 'HH:mm');
  };

  const formatDate = (timeString: string) => {
    return format(new Date(timeString), 'MMM d');
  };

  const calculateFlightDuration = (departure: string, arrival: string) => {
    const depTime = new Date(departure);
    const arrTime = new Date(arrival);
    const diffMs = arrTime.getTime() - depTime.getTime();
    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  };

  const SortButton = ({ 
    option, 
    label, 
    className = "" 
  }: { 
    option: SortOption; 
    label: string; 
    className?: string;
  }) => {
    const isActive = sortBy === option;
    const isLoading = isSorting && isActive;
    
    return (
      <button
        onClick={() => onSort(option)}
        disabled={isSorting}
        className={`flex items-center gap-1 px-3 py-2 text-sm rounded-md transition-all disabled:opacity-50 disabled:cursor-not-allowed ${
          isActive
            ? 'bg-primary-500 text-white'
            : 'bg-neutral-200 text-neutral-700 hover:bg-neutral-300'
        } ${className}`}
      >
        {label}
        {isLoading ? (
          <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
        ) : isActive ? (
          sortDirection === 'asc' ? 
            <ArrowUpIcon className="w-4 h-4" /> : 
            <ArrowDownIcon className="w-4 h-4" />
        ) : null}
      </button>
    );
  };

  if (paginationInfo.totalResults === 0) {
    return (
      <div className="bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-8 text-center">
        <div className="text-neutral-500 mb-4">
          <svg className="w-16 h-16 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          <h3 className="text-lg font-medium text-neutral-900 dark:text-neutral-900 mb-2">No flights found</h3>
          <p className="text-neutral-600">Try adjusting your search criteria</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header with sort options and last updated */}
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-4">
        <div className="flex items-center text-sm text-neutral-600 dark:text-neutral-600">
          <ClockIcon className="w-4 h-4 mr-1" />
          Prices updated {format(new Date(lastUpdated), 'MMM d, yyyy \'at\' HH:mm')}
        </div>
        
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-sm text-neutral-700 dark:text-neutral-700 mr-2">Sort by:</span>
          <SortButton option="departureTime" label="Departure" />
          <SortButton option="arrivalTime" label="Arrival" />
          <SortButton option="duration" label="Duration" />
          <SortButton option="price" label="Price" />
          <SortButton option="airline" label="Airline" />
        </div>
      </div>

      {/* Top Pagination */}
      <Pagination
        paginationInfo={paginationInfo}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
        isLoading={isPaging}
      />

      {/* Results */}
      <div className="space-y-4">
        {results.map((flight) => (
          <div
            key={flight.id}
            className={`bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-4 md:p-6 hover:shadow-md transition-all ${
              (isSorting || isPaging) ? 'opacity-75' : ''
            }`}
          >
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              {/* Flight Info */}
              <div className="flex-1">
                <div className="flex items-center gap-4 mb-2">
                  <div className="font-semibold text-neutral-900 dark:text-neutral-900">
                    {flight.airlineName} {flight.flightNumber && `(${flight.flightNumber})`}
                  </div>
                  <div className="text-sm text-neutral-600 bg-neutral-200 px-2 py-1 rounded">
                    {flight.cabinClass}
                  </div>
                  {flight.isDirect && (
                    <div className="text-sm text-green-600 bg-green-100 px-2 py-1 rounded">
                      Direct
                    </div>
                  )}
                </div>
                
                {/* Route Information */}
                <div className="flex items-center gap-4 mb-2">
                  <div className="text-sm">
                    <div className="font-medium">{flight.origin.code}</div>
                    <div className="text-neutral-600">{flight.origin.city}</div>
                  </div>
                  <ArrowRightIcon className="w-4 h-4 text-neutral-400" />
                  <div className="text-sm">
                    <div className="font-medium">{flight.destination.code}</div>
                    <div className="text-neutral-600">{flight.destination.city}</div>
                  </div>
                </div>
                
                <div className="flex items-center gap-4 text-sm text-neutral-700 dark:text-neutral-700">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{formatTime(flight.departureTime)}</span>
                    <span className="text-neutral-500">{formatDate(flight.departureTime)}</span>
                    <ArrowRightIcon className="w-4 h-4" />
                    <span className="font-medium">{formatTime(flight.arrivalTime)}</span>
                    <span className="text-neutral-500">{formatDate(flight.arrivalTime)}</span>
                  </div>
                  <div className="text-neutral-500">•</div>
                  <div>{flight.duration ? formatDuration(flight.duration) : calculateFlightDuration(flight.departureTime, flight.arrivalTime)}</div>
                  <div className="text-neutral-500">•</div>
                  <div>{formatStops(flight.stops)}</div>
                </div>
              </div>

              {/* Price and Book Button */}
              <div className="flex items-center gap-4">
                <div className="text-right">
                  <div className="text-2xl font-bold text-neutral-900 dark:text-neutral-900">
                    {formatPrice(flight.price.amount, flight.price.currency)}
                  </div>
                  <div className="text-sm text-neutral-600">Total price</div>
                </div>
                
                {flight.deepLink ? (
                  <a
                    href={flight.deepLink}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-accent-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded-md transition-colors flex items-center gap-2"
                  >
                    Book Now
                    <ArrowTopRightOnSquareIcon className="w-4 h-4" />
                  </a>
                ) : (
                  <button
                    disabled
                    className="bg-neutral-300 text-neutral-500 font-medium py-2 px-4 rounded-md cursor-not-allowed"
                  >
                    Not Available
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Bottom Pagination */}
      <Pagination
        paginationInfo={paginationInfo}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
        isLoading={isPaging}
      />

      {/* Loading overlay */}
      {(isSorting || isPaging) && (
        <div className="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 flex items-center gap-3">
            <div className="w-6 h-6 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
            <span className="text-neutral-700">
              {isSorting ? 'Sorting results...' : 'Loading page...'}
            </span>
          </div>
        </div>
      )}
    </div>
  );
}
