import { ItineraryDto } from './api-types';

export interface ItineraryOptionLeg {
  sequence: number;
  flightNumber: string;
  airlineCode: string;
  origin: string;
  destination: string;
  departureUtc: string;
  arrivalUtc: string;
  durationMinutes: number;
  cabinClass: string;
  priceAmount: number;
  priceCurrency: string;
  direction: 'Outbound' | 'Return';
}

export interface ItineraryOption {
  id: string;
  origin: string;
  finalDestination: string;
  isRoundTrip: boolean;
  outboundDeparture: string | null;
  returnDeparture: string | null;
  totalPrice: { amount: number; currency: string };
  totalDurationMinutes: number;
  formattedDuration: string;
  outboundLegs: ItineraryOptionLeg[];
  returnLegs: ItineraryOptionLeg[];
  airlineSummary: string;
  cabinClassSummary: string;
  stopsOutbound: number;
  stopsReturn?: number;
  isDirect: boolean;
}

export const minutesToDurationString = (minutes: number): string => {
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
};

export function convertItinerary(it: ItineraryDto): ItineraryOption {
  const outboundLegs = it.legs.filter(l => l.direction === 'Outbound');
  const returnLegs = it.legs.filter(l => l.direction === 'Return');
  const allLegs = it.legs;

  const airlineCodes = Array.from(new Set(allLegs.map(l => l.airlineCode)));
  const airlineSummary = airlineCodes.length === 0 ? '' : airlineCodes.length === 1 ? airlineCodes[0] : `${airlineCodes[0]} +${airlineCodes.length - 1}`;

  const cabinClasses = Array.from(new Set(allLegs.map(l => l.cabinClass)));
  const cabinClassSummary = cabinClasses.length === 0 ? '' : cabinClasses.length === 1 ? cabinClasses[0] : 'Mixed';

  const stopsOutbound = outboundLegs.length > 0 ? outboundLegs.length - 1 : 0;
  const stopsReturn = returnLegs.length > 0 ? returnLegs.length - 1 : undefined;

  const isDirect = !it.isRoundTrip
    ? allLegs.length === 1
    : (outboundLegs.length === 1 && (returnLegs.length === 0 || returnLegs.length === 1));

  return {
    id: it.id,
    origin: it.origin,
    finalDestination: it.finalDestination,
    isRoundTrip: it.isRoundTrip,
    outboundDeparture: it.outboundDeparture,
    returnDeparture: it.returnDeparture,
    totalPrice: { amount: it.totalPriceAmount, currency: it.totalPriceCurrency },
    totalDurationMinutes: it.totalDurationMinutes,
    formattedDuration: minutesToDurationString(it.totalDurationMinutes),
    outboundLegs: outboundLegs,
    returnLegs: returnLegs,
    airlineSummary,
    cabinClassSummary,
    stopsOutbound,
    stopsReturn,
    isDirect,
  };
}

export interface ItinerarySearchResultsUI {
  results: ItineraryOption[];
  page: number;
  pageSize: number;
  returned: number;
  hasNextPage: boolean;
  sortBy: string;
  sortOrder: string;
  roundTripRequested: boolean;
}

export function buildItinerarySearchResults(
  apiResult: { items: ItineraryDto[]; page: number; pageSize: number; returned: number; sortBy: string; sortOrder: string; roundTripRequested: boolean; }
): ItinerarySearchResultsUI {
  const converted = apiResult.items.map(convertItinerary);
  return {
    results: converted,
    page: apiResult.page,
    pageSize: apiResult.pageSize,
    returned: apiResult.returned,
    hasNextPage: apiResult.returned === apiResult.pageSize, // heuristic
    sortBy: apiResult.sortBy,
    sortOrder: apiResult.sortOrder,
    roundTripRequested: apiResult.roundTripRequested,
  };
}
