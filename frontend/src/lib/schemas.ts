import { z } from 'zod';
import { Flight, CabinClass } from './api-types';

// Example: Schema for a contact form
export const contactFormSchema = z.object({
  fullName: z.string().min(3, { message: "Full name must be at least 3 characters long." })
    .max(100, { message: "Full name must be no more than 100 characters." }),
  emailAddress: z.string().email({ message: "Please enter a valid email address." }),
  messageBody: z.string().min(10, { message: "Message must be at least 10 characters." })
    .max(1000, { message: "Message must be no more than 1000 characters." }),
  subscribeToNewsletter: z.boolean().optional(),
});

export type ContactFormValues = z.infer<typeof contactFormSchema>;

// Enhanced flight search form schema with API alignment
export const flightSearchSchema = z.object({
  originCode: z.string()
    .min(3, { message: "Origin airport code must be at least 3 characters." })
    .max(3, { message: "Origin airport code must be exactly 3 characters." })
    .regex(/^[A-Z]{3}$/, { message: "Origin must be a valid 3-letter airport code (e.g., LAX)." }),
  
  destinationCode: z.string()
    .min(3, { message: "Destination airport code must be at least 3 characters." })
    .max(3, { message: "Destination airport code must be exactly 3 characters." })
    .regex(/^[A-Z]{3}$/, { message: "Destination must be a valid 3-letter airport code (e.g., JFK)." }),
  
  departureDate: z.string()
    .min(1, { message: "Departure date is required." })
    .refine((date) => {
      const selectedDate = new Date(date);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return selectedDate >= today;
    }, { message: "Departure date must be today or in the future." }),
  
  returnDate: z.string()
    .optional()
    .or(z.literal("")),
    // Passenger counts aligned with API
  adults: z.number()
    .min(1, { message: "At least 1 adult passenger is required." })
    .max(9, { message: "Maximum 9 adult passengers allowed." }),
  
  children: z.number()
    .min(0, { message: "Number of children cannot be negative." })
    .max(8, { message: "Maximum 8 child passengers allowed." }),
  
  infants: z.number()
    .min(0, { message: "Number of infants cannot be negative." }),
  
  // Cabin class selection
  cabinClass: z.enum(['economy', 'premium_economy', 'business', 'first']),
    
}).refine((data) => {
  if (data.returnDate && data.returnDate !== "") {
    return new Date(data.returnDate) > new Date(data.departureDate);
  }
  return true;
}, {
  message: "Return date must be after departure date.",
  path: ["returnDate"]
}).refine((data) => {
  // Infants cannot exceed number of adults
  return data.infants <= data.adults;
}, {
  message: "Number of infants cannot exceed number of adults.",
  path: ["infants"]
});

export type FlightSearchFormValues = z.infer<typeof flightSearchSchema>;

// Updated flight result types to align with API
export interface FlightOption {
  id: string;
  airlineCode: string;
  airlineName: string;
  flightNumber: string;
  origin: {
    code: string;
    name: string;
    city: string;
  };
  destination: {
    code: string;
    name: string;
    city: string;
  };
  departureTime: string;
  arrivalTime: string;
  duration: string;
  price: {
    amount: number;
    currency: string;
  };
  stops: number;
  cabinClass: string;
  deepLink: string | null;
  isDirect: boolean;
  isInternational: boolean;
}

export interface FlightSearchResults {
  results: FlightOption[];
  lastUpdated: string;
  totalResults: number;
  currency: string;
  searchDuration: string;
}

// Search state management types
export interface SearchState {
  sortBy: SortOption;
  sortDirection: SortDirection;
  page: number;
  pageSize: number;
}

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

// Utility function to calculate pagination info
export const calculatePaginationInfo = (
  totalResults: number,
  currentPage: number,
  pageSize: number
): PaginationInfo => {
  const totalPages = Math.ceil(totalResults / pageSize);
  return {
    currentPage: Math.max(1, currentPage),
    totalPages: Math.max(1, totalPages),
    pageSize,
    totalResults,
    hasNextPage: currentPage < totalPages,
    hasPreviousPage: currentPage > 1,
  };
};

// Utility function to convert API Flight to FlightOption
export const convertApiFlightToFlightOption = (flight: Flight): FlightOption => {
  return {
    id: `${flight.airlineCode}-${flight.flightNumber}-${flight.departureTime}`,
    airlineCode: flight.airlineCode || '',
    airlineName: flight.airlineName || '',
    flightNumber: flight.flightNumber || '',
    origin: {
      code: flight.origin.code || '',
      name: flight.origin.name || '',
      city: flight.origin.city || '',
    },
    destination: {
      code: flight.destination.code || '',
      name: flight.destination.name || '',
      city: flight.destination.city || '',
    },
    departureTime: flight.departureTime,
    arrivalTime: flight.arrivalTime,
    duration: flight.duration,
    price: {
      amount: flight.price.amount,
      currency: flight.price.currency || 'USD',
    },
    stops: flight.stops,
    cabinClass: getCabinClassName(flight.cabinClass),
    deepLink: flight.deepLink,
    isDirect: flight.isDirect,
    isInternational: flight.isInternational,
  };
};

// Helper function to get cabin class name
const getCabinClassName = (cabinClass: CabinClass): string => {
  switch (cabinClass) {
    case CabinClass.Economy:
      return 'Economy';
    case CabinClass.PremiumEconomy:
      return 'Premium Economy';
    case CabinClass.Business:
      return 'Business';
    case CabinClass.First:
      return 'First';
    default:
      return 'Economy';
  }
};

// Helper function to convert form cabin class to API format
export const convertCabinClassToApi = (cabinClass: string): string => {
  const mapping: Record<string, string> = {
    'economy': 'Economy',
    'premium_economy': 'PremiumEconomy',
    'business': 'Business',
    'first': 'First',
  };
  return mapping[cabinClass] || 'Economy';
};
