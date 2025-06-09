import { z } from 'zod';

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

// Flight search form schema (FS-1)
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
    .min(1, { message: "Return date is required." })
    .optional()
    .or(z.literal(""))
}).refine((data) => {
  if (data.returnDate && data.returnDate !== "") {
    return new Date(data.returnDate) > new Date(data.departureDate);
  }
  return true;
}, {
  message: "Return date must be after departure date.",
  path: ["returnDate"]
});

export type FlightSearchFormValues = z.infer<typeof flightSearchSchema>;

// Flight result types
export interface FlightOption {
  id: string;
  airlineCode: string;
  airlineName: string;
  totalPriceCents: number;
  currency: string;
  stops: number;
  durationMinutes: number;
  departureTime: string;
  arrivalTime: string;
  bookingUrl: string;
  cabinClass: 'economy' | 'premium_economy' | 'business' | 'first';
}

export interface FlightSearchResults {
  results: FlightOption[];
  lastUpdated: string;
  searchId: string;
}
