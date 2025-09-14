export const API_CONFIG = {
  baseUrl: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5243', // Use HTTP port for development
  timeout: 30000,
  retryAttempts: 3,
  retryDelay: 1000,
} as const;

export const API_ENDPOINTS = {
  flights: {
    search: '/api/v1/Flights/search',
    getByNumber: '/api/v1/Flights/{airlineCode}/{flightNumber}',
  },
  itineraries: {
    search: '/api/v1/Itineraries/search',
  },
  health: {
    status: '/api/v1/Health',
    info: '/api/v1/Health/info',
  },
} as const;
