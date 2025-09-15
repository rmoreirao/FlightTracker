'use client';

import { ItineraryOption, ItineraryOptionLeg, minutesToDurationString } from '@/lib/itinerary-utils';
import { format } from 'date-fns';
import { ArrowRightIcon } from '@heroicons/react/24/outline';

interface ItineraryDetailsProps {
  itinerary: ItineraryOption;
}

function formatTime(dt: string) {
  return format(new Date(dt), 'HH:mm');
}

function formatDate(dt: string) {
  return format(new Date(dt), 'MMM d');
}

function computeLayover(prev: ItineraryOptionLeg, next: ItineraryOptionLeg) {
  const diffMs = new Date(next.departureUtc).getTime() - new Date(prev.arrivalUtc).getTime();
  if (diffMs <= 0) return null;
  const minutes = Math.floor(diffMs / 60000);
  return minutesToDurationString(minutes);
}

export function ItineraryDetails({ itinerary }: ItineraryDetailsProps) {
  const { outboundLegs, returnLegs, isRoundTrip } = itinerary;

  const Section = ({ title, legs }: { title: string; legs: ItineraryOptionLeg[] }) => (
    <div className="space-y-3">
      <h4 className="text-sm font-semibold text-neutral-800">{title}</h4>
      {legs.length === 0 && (
        <div className="text-sm text-neutral-500">No leg details available.</div>
      )}
      {legs.map((leg, idx) => {
        const layover = idx < legs.length - 1 ? computeLayover(legs[idx], legs[idx + 1]) : null;
        return (
          <div key={leg.sequence} className="border border-neutral-200 rounded-md p-3 bg-white/50">
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
            {layover && (
              <div className="mt-2 text-xs text-amber-600 bg-amber-50 border border-amber-200 rounded px-2 py-1 inline-block">
                Layover {layover}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );

  return (
    <div className="space-y-8">
      <Section title="Outbound" legs={outboundLegs} />
      {isRoundTrip && returnLegs.length > 0 && (
        <Section title="Return" legs={returnLegs} />
      )}
    </div>
  );
}
