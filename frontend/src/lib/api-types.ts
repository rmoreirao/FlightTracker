// TypeScript interfaces generated from FlightTracker API OpenAPI specification

export interface Airport {
  code: string | null;
  name: string | null;
  city: string | null;
  country: string | null;
  latitude: number | null;
  longitude: number | null;
  timezone: string | null;
}

export enum CabinClass {
  Economy = 1,
  PremiumEconomy = 2,
  Business = 3,
  First = 4,
}

export enum FlightStatus {
  Scheduled = 1,
  Active = 2,
  Landed = 3,
  Cancelled = 4,
  Incident = 5,
  Diverted = 6,
  Delayed = 7,
}

export interface Money {
  amount: number;
  currency: string | null;
}

export interface FlightSegment {
  id: number;
  flightNumber: string | null;
  airlineCode: string | null;
  origin: Airport;
  destination: Airport;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  aircraftType: string | null;
  segmentOrder: number;
  status: FlightStatus;
}

export interface Flight {
  flightNumber: string | null;
  airlineCode: string | null;
  airlineName: string | null;
  origin: Airport;
  destination: Airport;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  segments: FlightSegment[] | null;
  price: Money;
  cabinClass: CabinClass;
  deepLink: string | null;
  stops: number;
  status: FlightStatus;
  isDirect: boolean;
  isInternational: boolean;
  // Extended details for expandable view
  departureTerminal?: string | null;
  arrivalTerminal?: string | null;
  aircraftModel?: string | null;
  registration?: string | null;
  amenities?: FlightAmenities | null;
  fareBreakdown?: FareBreakdown | null;
  policies?: FlightPolicies | null;
}

export interface FlightAmenities {
  wifi: boolean;
  entertainment: boolean;
  power: boolean;
  meals?: string | null;
  legroom?: string | null;
}

export interface FareBreakdown {
  baseFare: number;
  taxes: number;
  fees: number;
  currency: string;
}

export interface FlightPolicies {
  cancellation?: string | null;
  changes?: string | null;
  baggage?: string | null;
}

export interface SearchFlightsResult {
  flights: Flight[] | null;
  lastUpdated: string;
  totalResults: number;
  currency: string | null;
  searchDuration: string;
}

// Frontend sorting and pagination types
export type SortOption = 'departureTime' | 'arrivalTime' | 'duration' | 'price' | 'airline';
export type SortDirection = 'asc' | 'desc';

export interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalResults: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface FlightSearchParams {
  originCode?: string;
  destinationCode?: string;
  departureDate?: string;
  returnDate?: string;
  cabins?: string;
  adults?: number;
  children?: number;
  infants?: number;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export interface HealthStatus {
  status: string | null;
  timestamp: string;
  version: string | null;
  environment: string | null;
}

export interface SystemInfo {
  applicationName: string | null;
  version: string | null;
  environment: string | null;
  machineName: string | null;
  processorCount: number;
  workingSet: number;
  timestamp: string;
}

export interface ProblemDetails {
  type: string | null;
  title: string | null;
  status: number | null;
  detail: string | null;
  instance: string | null;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors: Record<string, string[]> | null;
}

// Helper types for cabin class conversion
export const CabinClassNames: Record<CabinClass, string> = {
  [CabinClass.Economy]: 'Economy',
  [CabinClass.PremiumEconomy]: 'Premium Economy',
  [CabinClass.Business]: 'Business',
  [CabinClass.First]: 'First',
};

export const CabinClassValues: Record<string, CabinClass> = {
  'economy': CabinClass.Economy,
  'premium_economy': CabinClass.PremiumEconomy,
  'business': CabinClass.Business,
  'first': CabinClass.First,
};
