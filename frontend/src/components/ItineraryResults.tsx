'use client';

import { ItineraryOption } from '@/lib/itinerary-utils';
import { formatPrice } from '@/lib/api';
import { ArrowRightIcon, ArrowUpIcon, ArrowDownIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';

interface ItineraryResultsProps {
  results: ItineraryOption[];
  page: number;
  hasNextPage: boolean;
  onNext: () => void;
  onPrev: () => void;
  isPaging: boolean;
  sortBy: 'price' | 'duration';
  sortOrder: 'asc' | 'desc';
  onSortChange: (sort: 'price' | 'duration') => void;
  onToggleSortOrder: () => void;
  isSorting: boolean;
}

export function ItineraryResults({
  results,
  page,
  hasNextPage,
  onNext,
  onPrev,
  isPaging,
  sortBy,
  sortOrder,
  onSortChange,
  onToggleSortOrder,
  isSorting,
}: ItineraryResultsProps) {
  if (results.length === 0) {
    return (
      <div className="bg-neutral-100 rounded-lg p-8 text-center">
        <h3 className="text-lg font-medium mb-2">No itineraries found</h3>
        <p className="text-neutral-600">Try adjusting your search criteria.</p>
      </div>
    );
  }

  const SortButton = ({ label, value }: { label: string; value: 'price' | 'duration' }) => {
    const active = sortBy === value;
    return (
      <button
        disabled={isSorting}
        onClick={() => active ? onToggleSortOrder() : onSortChange(value)}
        className={`px-3 py-2 text-sm rounded-md flex items-center gap-1 transition-colors ${
          active ? 'bg-primary-500 text-white' : 'bg-neutral-200 text-neutral-700 hover:bg-neutral-300'
        }`}
      >
        {label}
        {active && (sortOrder === 'asc' ? <ArrowUpIcon className="w-4 h-4"/> : <ArrowDownIcon className="w-4 h-4"/>)}
      </button>
    );
  };

  const formatTime = (dt: string | null) => dt ? format(new Date(dt), 'HH:mm') : '';
  const formatDate = (dt: string | null) => dt ? format(new Date(dt), 'MMM d') : '';

  return (
    <div className="space-y-6">
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 bg-neutral-100 rounded-lg p-4">
        <div className="text-sm text-neutral-600">Page {page}</div>
        <div className="flex items-center gap-2">
          <span className="text-sm">Sort:</span>
          <SortButton label="Price" value="price" />
          <SortButton label="Duration" value="duration" />
        </div>
      </div>

      {results.map(it => (
        <div key={it.id} className={`bg-neutral-100 rounded-lg p-4 md:p-6 space-y-4 ${isPaging || isSorting ? 'opacity-75' : ''}`}>          
          <div className="flex flex-col md:flex-row md:justify-between gap-4">
            <div className="flex-1">
              <div className="flex items-center gap-3 mb-2">
                <div className="font-semibold">{it.airlineSummary}</div>
                <div className="text-sm bg-neutral-200 px-2 py-1 rounded">{it.cabinClassSummary}</div>
                {it.isRoundTrip && <div className="text-sm bg-blue-100 text-blue-800 px-2 py-1 rounded">Round Trip</div>}
                {it.isDirect && <div className="text-sm bg-green-100 text-green-700 px-2 py-1 rounded">Direct</div>}
              </div>

              {/* Outbound */}
              <div className="mb-2">
                <div className="text-sm font-medium mb-1">Outbound</div>
                {it.outboundLegs.length > 0 && (
                  <div className="flex items-center gap-3 text-sm text-neutral-700">
                    <div>
                      <div className="font-medium">{it.outboundLegs[0].origin}</div>
                      <div className="text-neutral-500">{formatDate(it.outboundLegs[0].departureUtc)} {formatTime(it.outboundLegs[0].departureUtc)}</div>
                    </div>
                    <ArrowRightIcon className="w-4 h-4" />
                    <div>
                      <div className="font-medium">{it.outboundLegs[it.outboundLegs.length - 1].destination}</div>
                      <div className="text-neutral-500">{formatDate(it.outboundLegs[it.outboundLegs.length - 1].arrivalUtc)} {formatTime(it.outboundLegs[it.outboundLegs.length - 1].arrivalUtc)}</div>
                    </div>
                    <div className="text-neutral-500">•</div>
                    <div>{it.stopsOutbound === 0 ? 'Nonstop' : `${it.stopsOutbound} stop${it.stopsOutbound > 1 ? 's' : ''}`}</div>
                  </div>
                )}
              </div>

              {/* Return */}
              {it.returnLegs.length > 0 && (
                <div className="mb-2">
                  <div className="text-sm font-medium mb-1">Return</div>
                  <div className="flex items-center gap-3 text-sm text-neutral-700">
                    <div>
                      <div className="font-medium">{it.returnLegs[0].origin}</div>
                      <div className="text-neutral-500">{formatDate(it.returnLegs[0].departureUtc)} {formatTime(it.returnLegs[0].departureUtc)}</div>
                    </div>
                    <ArrowRightIcon className="w-4 h-4" />
                    <div>
                      <div className="font-medium">{it.returnLegs[it.returnLegs.length - 1].destination}</div>
                      <div className="text-neutral-500">{formatDate(it.returnLegs[it.returnLegs.length - 1].arrivalUtc)} {formatTime(it.returnLegs[it.returnLegs.length - 1].arrivalUtc)}</div>
                    </div>
                    <div className="text-neutral-500">•</div>
                    <div>{(it.stopsReturn ?? 0) === 0 ? 'Nonstop' : `${it.stopsReturn} stop${(it.stopsReturn ?? 0) > 1 ? 's' : ''}`}</div>
                  </div>
                </div>
              )}

              <div className="text-sm text-neutral-600">Total duration: {it.formattedDuration}</div>
            </div>

            <div className="flex items-center gap-4">
              <div className="text-right">
                <div className="text-2xl font-bold">{formatPrice(it.totalPrice.amount, it.totalPrice.currency)}</div>
                <div className="text-sm text-neutral-600">Total price</div>
              </div>
              <button disabled className="bg-neutral-300 text-neutral-500 font-medium py-2 px-4 rounded-md cursor-not-allowed">Booking N/A</button>
            </div>
          </div>
        </div>
      ))}

      <div className="flex items-center justify-between gap-4 bg-neutral-100 rounded-lg p-4">
        <button
          onClick={onPrev}
          disabled={isPaging || page === 1}
          className="px-4 py-2 bg-neutral-200 rounded disabled:opacity-50"
        >Previous</button>
        <div className="text-sm">Page {page}</div>
        <button
          onClick={onNext}
          disabled={isPaging || !hasNextPage}
          className="px-4 py-2 bg-neutral-200 rounded disabled:opacity-50"
        >Next</button>
      </div>

      {(isPaging || isSorting) && (
        <div className="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 flex items-center gap-3">
            <div className="w-6 h-6 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
            <span className="text-neutral-700">
              {isSorting ? 'Sorting itineraries...' : 'Loading page...'}
            </span>
          </div>
        </div>
      )}
    </div>
  );
}
