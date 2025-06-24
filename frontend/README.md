
## FlightTracker Frontend

This is the frontend application for FlightTracker, built with Next.js 14, TypeScript, and Tailwind CSS. It integrates with the FlightTracker API to provide real-time flight search functionality.

## Features

- 🛫 **Flight Search**: Search for flights between airports with real-time pricing
- 👥 **Passenger Management**: Support for adults, children, and infants
- 🎫 **Cabin Classes**: Economy, Premium Economy, Business, and First class options
- 📅 **Flexible Dates**: One-way and round-trip flight searches
- 💰 **Price Comparison**: Sort by price, duration, and number of stops
- 📱 **Responsive Design**: Mobile-first responsive interface

## API Integration

The frontend integrates with the FlightTracker API endpoints:

- **Flight Search**: `GET /api/v1/Flights/search`
- **Flight Details**: `GET /api/v1/Flights/{airlineCode}/{flightNumber}`
- **Health Check**: `GET /api/v1/Health`

### Environment Configuration

Update `.env.local` with your API configuration:

```bash
NEXT_PUBLIC_API_BASE_URL=https://localhost:7263
NEXT_PUBLIC_APP_NAME=Flight Tracker
NEXT_PUBLIC_APP_VERSION=1.0.0
```

## Getting Started

First, install the dependencies:

```bash
npm install
```

then run the development server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Project Structure

```
src/
├── app/                    # Next.js App Router
│   ├── layout.tsx         # Root layout
│   ├── page.tsx           # Home page with flight search
│   └── globals.css        # Global styles
├── components/            # React components
│   ├── FlightSearchForm.tsx
│   ├── FlightResults.tsx
│   ├── AirportAutocomplete.tsx
│   └── Navbar.tsx
├── hooks/                 # Custom React hooks
│   └── useFlightSearch.ts # Flight search logic
├── lib/                   # Utility libraries
│   ├── api.ts            # API client implementation
│   ├── api-types.ts      # TypeScript types from OpenAPI
│   ├── config.ts         # API configuration
│   ├── schemas.ts        # Zod validation schemas
│   └── types.ts          # Additional TypeScript types
└── ...
```

## Development

### API Integration

The application uses a custom API client (`src/lib/api.ts`) that handles:

- **Authentication**: Bearer token support (when needed)
- **Error Handling**: Proper error messages and retry logic
- **Type Safety**: Full TypeScript integration with API responses
- **Request Configuration**: Timeout, retry, and base URL configuration

### Form Validation

Flight search forms use Zod schemas for validation:

- Airport code validation (3-letter IATA codes)
- Date validation (future dates only)
- Passenger count limits (API constraints)
- Cabin class enumeration

### State Management

The application uses React hooks for state management:

- `useFlightSearch`: Handles flight search API calls and state
- React Hook Form: Form state and validation
- Local component state: UI interactions and sorting

## Technologies Used

- **Next.js 14**: React framework with App Router
- **TypeScript**: Type-safe development
- **Tailwind CSS**: Utility-first CSS framework
- **React Hook Form**: Form state management
- **Zod**: Schema validation
- **Heroicons**: Icon library
- **date-fns**: Date manipulation

## API Error Handling

The application handles various API error scenarios:

- **400 Bad Request**: Validation errors with detailed messages
- **429 Rate Limit**: Too many requests notification
- **500 Server Error**: Generic server error handling
- **Network Errors**: Connection timeout and retry logic

## Build and Deploy

To build for production:

```bash
npm run build
npm start
```

The application can be deployed to Vercel, Netlify, or any platform supporting Next.js.
