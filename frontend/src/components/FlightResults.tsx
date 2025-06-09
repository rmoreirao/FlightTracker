'use client';

import { FlightOption } from '@/lib/schemas';
import { ClockIcon, ArrowRightIcon, ArrowTopRightOnSquareIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';

interface FlightResultsProps {
  results: FlightOption[];
  lastUpdated: string;
  onSort: (sortBy: 'price' | 'duration' | 'stops') => void;
  currentSort: 'price' | 'duration' | 'stops';
}

export function FlightResults({ results, lastUpdated, onSort, currentSort }: FlightResultsProps) {
  const formatPrice = (cents: number, currency: string) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
    }).format(cents / 100);
  };

  const formatDuration = (minutes: number) => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  };

  const formatStops = (stops: number) => {
    if (stops === 0) return 'Nonstop';
    if (stops === 1) return '1 Stop';
    return `${stops} Stops`;
  };

  const formatTime = (timeString: string) => {
    return format(new Date(timeString), 'HH:mm');
  };

  if (results.length === 0) {
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
    <div className="space-y-4">
      {/* Header with sort options and last updated */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-4">
        <div className="flex items-center text-sm text-neutral-600 dark:text-neutral-600">
          <ClockIcon className="w-4 h-4 mr-1" />
          Prices updated {format(new Date(lastUpdated), 'MMM d, yyyy \'at\' HH:mm')}
        </div>
        
        <div className="flex gap-2">
          <span className="text-sm text-neutral-700 dark:text-neutral-700 mr-2">Sort by:</span>
          <button
            onClick={() => onSort('price')}
            className={`px-3 py-1 text-sm rounded-md transition-colors ${
              currentSort === 'price'
                ? 'bg-primary-500 text-white'
                : 'bg-neutral-200 text-neutral-700 hover:bg-neutral-300'
            }`}
          >
            Price
          </button>
          <button
            onClick={() => onSort('duration')}
            className={`px-3 py-1 text-sm rounded-md transition-colors ${
              currentSort === 'duration'
                ? 'bg-primary-500 text-white'
                : 'bg-neutral-200 text-neutral-700 hover:bg-neutral-300'
            }`}
          >
            Duration
          </button>
          <button
            onClick={() => onSort('stops')}
            className={`px-3 py-1 text-sm rounded-md transition-colors ${
              currentSort === 'stops'
                ? 'bg-primary-500 text-white'
                : 'bg-neutral-200 text-neutral-700 hover:bg-neutral-300'
            }`}
          >
            Stops
          </button>
        </div>
      </div>

      {/* Results */}
      <div className="space-y-4">
        {results.map((flight) => (
          <div
            key={flight.id}
            className="bg-neutral-100 dark:bg-neutral-100 rounded-lg shadow-sm p-4 md:p-6 hover:shadow-md transition-shadow"
          >
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
              {/* Flight Info */}
              <div className="flex-1">
                <div className="flex items-center gap-4 mb-2">
                  <div className="font-semibold text-neutral-900 dark:text-neutral-900">
                    {flight.airlineName} ({flight.airlineCode})
                  </div>
                  <div className="text-sm text-neutral-600 bg-neutral-200 px-2 py-1 rounded">
                    {flight.cabinClass.replace('_', ' ')}
                  </div>
                </div>
                
                <div className="flex items-center gap-4 text-sm text-neutral-700 dark:text-neutral-700">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{formatTime(flight.departureTime)}</span>
                    <ArrowRightIcon className="w-4 h-4" />
                    <span className="font-medium">{formatTime(flight.arrivalTime)}</span>
                  </div>
                  <div className="text-neutral-500">•</div>
                  <div>{formatDuration(flight.durationMinutes)}</div>
                  <div className="text-neutral-500">•</div>
                  <div>{formatStops(flight.stops)}</div>
                </div>
              </div>

              {/* Price and Book Button */}
              <div className="flex items-center gap-4">
                <div className="text-right">
                  <div className="text-2xl font-bold text-neutral-900 dark:text-neutral-900">
                    {formatPrice(flight.totalPriceCents, flight.currency)}
                  </div>
                  <div className="text-sm text-neutral-600">Total price</div>
                </div>
                
                <a
                  href={flight.bookingUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="bg-accent-500 hover:bg-orange-600 text-white font-medium py-2 px-4 rounded-md transition-colors flex items-center gap-2"
                >
                  Book Now
                  <ArrowTopRightOnSquareIcon className="w-4 h-4" />
                </a>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
