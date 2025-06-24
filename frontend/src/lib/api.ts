import { API_CONFIG, API_ENDPOINTS } from './config';
import { 
  SearchFlightsResult, 
  FlightSearchParams, 
  HealthStatus, 
  SystemInfo, 
  ProblemDetails, 
  ValidationProblemDetails,
  Flight 
} from './api-types';

export class ApiError extends Error {
  constructor(
    message: string,
    public status?: number,
    public details?: ProblemDetails | ValidationProblemDetails
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

class FlightTrackerApiClient {
  private baseUrl: string;
  private timeout: number;

  constructor() {
    this.baseUrl = API_CONFIG.baseUrl;
    this.timeout = API_CONFIG.timeout;
  }

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    
    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    };

    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), this.timeout);

      const response = await fetch(url, {
        ...config,
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        await this.handleErrorResponse(response);
      }

      const data = await response.json();
      return data as T;
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      
      if (error instanceof Error) {
        if (error.name === 'AbortError') {
          throw new ApiError('Request timeout');
        }
        throw new ApiError(`Network error: ${error.message}`);
      }
      
      throw new ApiError('Unknown error occurred');
    }
  }

  private async handleErrorResponse(response: Response): Promise<never> {
    let errorDetails: ProblemDetails | ValidationProblemDetails | undefined;
    
    try {
      errorDetails = await response.json();
    } catch {
      // If JSON parsing fails, we'll use default error message
    }

    const message = errorDetails?.title || 
                   errorDetails?.detail || 
                   `HTTP ${response.status}: ${response.statusText}`;

    throw new ApiError(message, response.status, errorDetails);
  }

  // Flight Search API
  async searchFlights(params: FlightSearchParams): Promise<SearchFlightsResult> {
    const queryParams = new URLSearchParams();
    
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        queryParams.append(key, value.toString());
      }
    });

    const endpoint = `${API_ENDPOINTS.flights.search}?${queryParams.toString()}`;
    return this.makeRequest<SearchFlightsResult>(endpoint);
  }

  // Get flight by airline code and flight number
  async getFlightByNumber(
    airlineCode: string, 
    flightNumber: string, 
    departureDate?: string
  ): Promise<Flight> {
    let endpoint = API_ENDPOINTS.flights.getByNumber
      .replace('{airlineCode}', airlineCode)
      .replace('{flightNumber}', flightNumber);

    if (departureDate) {
      endpoint += `?departureDate=${encodeURIComponent(departureDate)}`;
    }

    return this.makeRequest<Flight>(endpoint);
  }

  // Health Check APIs
  async getHealthStatus(): Promise<HealthStatus> {
    return this.makeRequest<HealthStatus>(API_ENDPOINTS.health.status);
  }

  async getSystemInfo(): Promise<SystemInfo> {
    return this.makeRequest<SystemInfo>(API_ENDPOINTS.health.info);
  }
}

// Export singleton instance
export const flightApi = new FlightTrackerApiClient();

// Utility functions for common operations
export const formatPrice = (amount: number, currency: string = 'USD'): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
  }).format(amount);
};

export const formatDuration = (durationString: string): string => {
  // Parse ISO 8601 duration format (PT6H30M) or handle other formats
  const match = durationString.match(/PT(?:(\d+)H)?(?:(\d+)M)?/);
  if (match) {
    const hours = parseInt(match[1] || '0');
    const minutes = parseInt(match[2] || '0');
    
    if (hours > 0 && minutes > 0) {
      return `${hours}h ${minutes}m`;
    } else if (hours > 0) {
      return `${hours}h`;
    } else {
      return `${minutes}m`;
    }
  }
  
  // Fallback for other duration formats
  return durationString;
};

export const formatDateTime = (dateTimeString: string): string => {
  const date = new Date(dateTimeString);
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  }).format(date);
};