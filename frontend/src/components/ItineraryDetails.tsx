'use client';

import { ItineraryOption, ItineraryOptionLeg, minutesToDurationString } from '@/lib/itinerary-utils';
import { format } from 'date-fns';
import { ArrowRightIcon, WifiIcon, TvIcon, BoltIcon, CakeIcon, MapPinIcon, PaperAirplaneIcon, ChevronDownIcon } from '@heroicons/react/24/outline';
import { formatPrice } from '@/lib/api';
import { useState, useMemo, useCallback } from 'react';

interface ItineraryDetailsProps {
  itinerary: ItineraryOption;
}

function formatTime(dt: string) {
  return format(new Date(dt), 'HH:mm');
}

function formatDate(dt: string) {
  return format(new Date(dt), 'MMM d');
}

// Helper returning numeric layover minutes
function layoverMinutes(prev: ItineraryOptionLeg, next: ItineraryOptionLeg) {
  const diffMs = new Date(next.departureUtc).getTime() - new Date(prev.arrivalUtc).getTime();
  if (diffMs <= 0) return 0;
  return Math.floor(diffMs / 60000);
}

function totalLayoverMinutes(legs: ItineraryOptionLeg[]): number {
  if (legs.length < 2) return 0;
  let sum = 0;
  for (let i = 0; i < legs.length - 1; i++) {
    const diffMs = new Date(legs[i + 1].departureUtc).getTime() - new Date(legs[i].arrivalUtc).getTime();
    if (diffMs > 0) sum += Math.floor(diffMs / 60000);
  }
  return sum;
}

export function ItineraryDetails({ itinerary }: ItineraryDetailsProps) {
  const { outboundLegs, returnLegs, isRoundTrip, totalPrice, formattedDuration, stopsOutbound, stopsReturn } = itinerary;
  // Re-added derived leg references (were removed inadvertently)
  const firstOutbound = outboundLegs.length > 0 ? outboundLegs[0] : undefined;
  const lastOutbound = outboundLegs.length > 0 ? outboundLegs[outboundLegs.length - 1] : undefined;
  const firstReturn = returnLegs.length > 0 ? returnLegs[0] : undefined;
  const lastReturn = returnLegs.length > 0 ? returnLegs[returnLegs.length - 1] : undefined;

  // Re-add mockDetails used in amenities & fare sections
  const mockDetails = {
    aircraftModels: Array.from(new Set([...outboundLegs, ...returnLegs].map(l => l.airlineCode + ' ' + (l.cabinClass || '')))),
    amenities: {
      wifi: true,
      entertainment: true,
      power: true,
      meals: 'Snack & beverage service',
      legroom: 'Standard (31 in)'
    },
    fareBreakdown: {
      baseFare: Math.round(totalPrice.amount * 0.70),
      taxes: Math.round(totalPrice.amount * 0.20),
      fees: Math.round(totalPrice.amount * 0.10),
      currency: totalPrice.currency
    },
    policies: {
      cancellation: 'Free cancellation within 24 hours of booking',
      changes: 'Ticket changes permitted with fee',
      baggage: '1 carry-on + 1 checked bag (23kg) included'
    }
  };

  // Collapsible segment groups
  const [showOutbound, setShowOutbound] = useState(true);
  const [showReturn, setShowReturn] = useState(true);
  const LONG_LAYOVER_MINUTES = 240; // 4h threshold

  // Memoized layover arrays for outbound/return
  const outboundLayovers = useMemo(() => {
    const arr: { index: number; minutes: number; fromLeg: ItineraryOptionLeg; toLeg: ItineraryOptionLeg }[] = [];
    for (let i = 0; i < outboundLegs.length - 1; i++) {
      const mins = layoverMinutes(outboundLegs[i], outboundLegs[i + 1]);
      if (mins > 0) arr.push({ index: i, minutes: mins, fromLeg: outboundLegs[i], toLeg: outboundLegs[i + 1] });
    }
    return arr;
  }, [outboundLegs]);
  const returnLayovers = useMemo(() => {
    const arr: { index: number; minutes: number; fromLeg: ItineraryOptionLeg; toLeg: ItineraryOptionLeg }[] = [];
    for (let i = 0; i < returnLegs.length - 1; i++) {
      const mins = layoverMinutes(returnLegs[i], returnLegs[i + 1]);
      if (mins > 0) arr.push({ index: i, minutes: mins, fromLeg: returnLegs[i], toLeg: returnLegs[i + 1] });
    }
    return arr;
  }, [returnLegs]);

  const toggleOutbound = useCallback(() => setShowOutbound(p => !p), []);
  const toggleReturn = useCallback(() => setShowReturn(p => !p), []);

  const renderLegCard = (leg: ItineraryOptionLeg) => {
    return (
      <div key={leg.sequence} className="border border-neutral-200 rounded-md p-3 bg-white/50 transition-colors">
        <div className="flex items-center justify-between text-sm">
          <div className="font-medium">{leg.airlineCode} {leg.flightNumber}</div>
          <div className="text-neutral-500">{minutesToDurationString(leg.durationMinutes)}</div>
        </div>
        <div className="mt-2 flex items-center gap-4 text-sm text-neutral-700">
          <div>
            <div className="font-medium">{leg.origin}</div>
            <div className="text-neutral-500">{formatDate(leg.departureUtc)} {formatTime(leg.departureUtc)}</div>
          </div>
          <ArrowRightIcon className="w-4 h-4 text-neutral-400" />
          <div>
            <div className="font-medium">{leg.destination}</div>
            <div className="text-neutral-500">{formatDate(leg.arrivalUtc)} {formatTime(leg.arrivalUtc)}</div>
          </div>
        </div>
        <div className="mt-2 text-xs text-neutral-500 flex flex-wrap gap-2">
          <span>Cabin: {leg.cabinClass}</span>
          <span>Seq: {leg.sequence}</span>
        </div>
      </div>
    );
  };

  const renderLegsWithLayovers = (legs: ItineraryOptionLeg[], layovers: { index: number; minutes: number; fromLeg: ItineraryOptionLeg; toLeg: ItineraryOptionLeg }[]) => {
    const result: JSX.Element[] = [];
    for (let i = 0; i < legs.length; i++) {
      result.push(renderLegCard(legs[i]));
      const lay = layovers.find(l => l.index === i);
      if (lay) {
        const long = lay.minutes >= LONG_LAYOVER_MINUTES;
        const cls = long ? 'text-red-700 bg-red-50 border-red-300' : 'text-amber-600 bg-amber-50 border-amber-200';
        const formatted = minutesToDurationString(lay.minutes);
        const start = formatTime(lay.fromLeg.arrivalUtc);
        const end = formatTime(lay.toLeg.departureUtc);
        result.push(
          <div
            key={`layover-${lay.index}-${lay.minutes}`}
            className={`mt-2 text-xs ${cls} border rounded px-2 py-1 inline-block`}
            title={`Layover at ${lay.fromLeg.destination}\nArrival: ${formatDate(lay.fromLeg.arrivalUtc)} ${start}\nNext Departure: ${formatDate(lay.toLeg.departureUtc)} ${end}\nDuration: ${formatted}`}
          >
            Layover {formatted} • {lay.fromLeg.destination}{long && ' (Long)'}
          </div>
        );
      }
    }
    return result;
  };

  const outboundLayoverTotal = totalLayoverMinutes(outboundLegs);
  const returnLayoverTotal = totalLayoverMinutes(returnLegs);

  return (
    <div className="border-t border-neutral-200 bg-neutral-50 p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center">
        <h3 className="text-lg font-semibold text-neutral-900">Itinerary Details</h3>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Route Information */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3 flex items-center gap-2">
            <MapPinIcon className="w-5 h-5" />
            Route Information
          </h4>
          <div className="space-y-4 text-sm">
            <div>
              <div className="text-neutral-500">Outbound</div>
              {firstOutbound && lastOutbound ? (
                <>
                  <div className="font-medium">{firstOutbound.origin} → {lastOutbound.destination}</div>
                  <div className="text-neutral-600">{formatDate(firstOutbound.departureUtc)} {formatTime(firstOutbound.departureUtc)} to {formatDate(lastOutbound.arrivalUtc)} {formatTime(lastOutbound.arrivalUtc)}</div>
                  <div className="text-neutral-600">Stops: {stopsOutbound}</div>
                </>
              ) : (
                <div className="text-neutral-600">No outbound data</div>
              )}
            </div>
            {isRoundTrip && (
              <div>
                <div className="text-neutral-500">Return</div>
                {firstReturn && lastReturn ? (
                  <>
                    <div className="font-medium">{firstReturn.origin} → {lastReturn.destination}</div>
                    <div className="text-neutral-600">{formatDate(firstReturn.departureUtc)} {formatTime(firstReturn.departureUtc)} to {formatDate(lastReturn.arrivalUtc)} {formatTime(lastReturn.arrivalUtc)}</div>
                    <div className="text-neutral-600">Stops: {stopsReturn ?? 0}</div>
                  </>
                ) : (
                  <div className="text-neutral-600">No return data</div>
                )}
              </div>
            )}
            <div>
              <div className="text-neutral-500">Total Duration</div>
              <div className="font-medium">{formattedDuration}</div>
              {(outboundLayoverTotal > 0 || (isRoundTrip && returnLayoverTotal > 0)) && (
                <div className="text-xs text-neutral-500 mt-1">
                  Layovers: {outboundLayoverTotal > 0 && <span>Outbound {minutesToDurationString(outboundLayoverTotal)}</span>}
                  {outboundLayoverTotal > 0 && (isRoundTrip && returnLayoverTotal > 0) && <span> • </span>}
                  {isRoundTrip && returnLayoverTotal > 0 && <span>Return {minutesToDurationString(returnLayoverTotal)}</span>}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Aircraft & Amenities */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3 flex items-center gap-2">
            <PaperAirplaneIcon className="w-5 h-5" />
            Aircraft & Amenities
          </h4>
          <div className="space-y-3 text-sm">
            <div>
              <div className="text-neutral-500">Operating Carriers / Cabins</div>
              <div className="text-neutral-700 space-y-1">
                {mockDetails.aircraftModels.map((m, i) => (
                  <div key={i}>{m}</div>
                ))}
              </div>
            </div>

            <div>
              <div className="text-neutral-500 mb-2">Amenities</div>
              <div className="space-y-1">
                <div className="flex items-center gap-2">
                  <WifiIcon className={`w-4 h-4 ${mockDetails.amenities.wifi ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.wifi ? 'text-neutral-700' : 'text-neutral-400'}>Wi-Fi {mockDetails.amenities.wifi ? 'Available' : 'Not Available'}</span>
                </div>
                <div className="flex items-center gap-2">
                  <TvIcon className={`w-4 h-4 ${mockDetails.amenities.entertainment ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.entertainment ? 'text-neutral-700' : 'text-neutral-400'}>Entertainment {mockDetails.amenities.entertainment ? 'Available' : 'Not Available'}</span>
                </div>
                <div className="flex items-center gap-2">
                  <BoltIcon className={`w-4 h-4 ${mockDetails.amenities.power ? 'text-green-500' : 'text-neutral-300'}`} />
                  <span className={mockDetails.amenities.power ? 'text-neutral-700' : 'text-neutral-400'}>Power Outlets {mockDetails.amenities.power ? 'Available' : 'Not Available'}</span>
                </div>
                <div className="flex items-center gap-2">
                  <CakeIcon className="w-4 h-4 text-green-500" />
                  <span className="text-neutral-700">{mockDetails.amenities.meals}</span>
                </div>
              </div>
            </div>

            <div>
              <div className="text-neutral-500">Legroom</div>
              <div className="text-neutral-700">{mockDetails.amenities.legroom}</div>
            </div>
          </div>
        </div>

        {/* Fare & Policies */}
        <div className="bg-white rounded-lg p-4">
          <h4 className="font-medium text-neutral-900 mb-3">Fare & Policies</h4>
          <div className="space-y-4 text-sm">
            <div>
              <div className="text-neutral-500 mb-2">Fare Breakdown</div>
              <div className="space-y-1">
                <div className="flex justify-between">
                  <span className="text-neutral-600">Base Fare</span>
                  <span className="text-neutral-900">{formatPrice(mockDetails.fareBreakdown.baseFare, mockDetails.fareBreakdown.currency)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-neutral-600">Taxes & Fees</span>
                  <span className="text-neutral-900">{formatPrice(mockDetails.fareBreakdown.taxes + mockDetails.fareBreakdown.fees, mockDetails.fareBreakdown.currency)}</span>
                </div>
                <div className="border-t border-neutral-200 pt-1">
                  <div className="flex justify-between font-medium">
                    <span>Total</span>
                    <span>{formatPrice(totalPrice.amount, totalPrice.currency)}</span>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <div className="text-neutral-500 mb-2">Policies</div>
              <div className="space-y-2 text-neutral-700">
                <div><span className="font-medium">Cancellation:</span> {mockDetails.policies.cancellation}</div>
                <div><span className="font-medium">Changes:</span> {mockDetails.policies.changes}</div>
                <div><span className="font-medium">Baggage:</span> {mockDetails.policies.baggage}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Segment details */}
      <div className="bg-white rounded-lg p-4 space-y-6">
        <h4 className="font-medium text-neutral-900 mb-3">Segments</h4>
        <div className="space-y-8">
          <div>
            <button
              type="button"
              onClick={toggleOutbound}
              className="flex items-center gap-2 text-sm font-semibold text-neutral-700 mb-3 group"
              aria-expanded={showOutbound}
            >
              <ChevronDownIcon className={`h-4 w-4 transition-transform ${showOutbound ? 'rotate-180' : ''}`} />
              Outbound Segments
              <span className="text-xs font-normal text-neutral-500">({outboundLegs.length} leg{outboundLegs.length !== 1 ? 's' : ''})</span>
            </button>
            <div className={`space-y-4 origin-top transition-all duration-300 ${showOutbound ? 'opacity-100 scale-y-100' : 'opacity-0 scale-y-0 h-0 overflow-hidden'}`}>
              {showOutbound && renderLegsWithLayovers(outboundLegs, outboundLayovers)}
            </div>
          </div>
          {isRoundTrip && returnLegs.length > 0 && (
            <div>
              <button
                type="button"
                onClick={toggleReturn}
                className="flex items-center gap-2 text-sm font-semibold text-neutral-700 mb-3 group"
                aria-expanded={showReturn}
              >
                <ChevronDownIcon className={`h-4 w-4 transition-transform ${showReturn ? 'rotate-180' : ''}`} />
                Return Segments
                <span className="text-xs font-normal text-neutral-500">({returnLegs.length} leg{returnLegs.length !== 1 ? 's' : ''})</span>
              </button>
              <div className={`space-y-4 origin-top transition-all duration-300 ${showReturn ? 'opacity-100 scale-y-100' : 'opacity-0 scale-y-0 h-0 overflow-hidden'}`}>
                {showReturn && renderLegsWithLayovers(returnLegs, returnLayovers)}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
