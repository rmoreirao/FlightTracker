# Flight Tracker API - Validation Rules

This document describes the comprehensive validation rules implemented in the Flight Tracker API.

## Overview

The API uses **FluentValidation** with **MediatR pipeline behaviors** to provide robust input validation following the CQRS pattern as specified in the BACKEND_GUIDE.MD.

## Validation Architecture

### Components

1. **ValidationBehavior<TRequest, TResponse>** - MediatR pipeline behavior that validates requests
2. **LoggingBehavior<TRequest, TResponse>** - MediatR pipeline behavior for request/response logging
3. **Query/Command Validators** - FluentValidation validators for each query and command
4. **Custom Validation Extensions** - Reusable validation logic for common business rules

### Pipeline Flow

```
API Request → Controller → MediatR → LoggingBehavior → ValidationBehavior → Handler
```

## Implemented Validation Rules

### Flight Search Query (`SearchFlightsQuery`)

#### Airport Code Validation
- **Format**: Must be exactly 3 characters, uppercase letters only
- **Recognition**: Must be a known airport code from our database
- **Business Rule**: Origin and destination must be different
- **Route Viability**: Route must be commercially viable (served by airlines)

#### Date Validation
- **Departure Date**: Must be within booking window (today to 365 days in future)
- **Return Date**: Must be after departure date (if provided)
- **Travel Duration**: Must be between 1 day and 90 days for round trips

#### Passenger Validation
- **Adults**: Must be between 1 and 9
- **Children**: Must be between 0 and 9
- **Infants**: Must be between 0 and number of adults (lap children rule)
- **Total**: Cannot exceed 9 passengers total
- **Business Rule**: All passenger counts must be non-negative

#### Cabin Class Validation
- **Valid Values**: economy, premium_economy, business, first
- **Format**: Case-insensitive, comma-separated list
- **Optional**: If not provided, defaults to all cabin classes

### Flight Details Query (`GetFlightDetailsQuery`)

#### Flight Number Validation
- **Format**: Must contain only uppercase letters and numbers
- **Length**: Cannot exceed 10 characters
- **Required**: Cannot be empty

#### Airline Code Validation
- **Format**: Must be exactly 2 characters, uppercase letters only
- **Required**: Cannot be empty

#### Date Validation
- **Range**: Cannot be more than 1 day in the past or 1 year in the future
- **Business Rule**: Allows recent historical lookups and future bookings

### Price Alert Command (`CreatePriceAlertCommand`)

#### Price Validation
- **Range**: Must be between 0 and 50,000
- **Currency**: Must be valid 3-letter ISO code (USD, EUR, GBP, CAD, AUD, JPY)
- **Business Rule**: Prevents unrealistic price alerts

#### Email Validation
- **Format**: Must be valid email address format
- **Length**: Cannot exceed 254 characters
- **Required**: Cannot be empty

#### Date Validation
- **Departure**: Cannot be in the past or more than 1 year in future
- **Return**: Cannot be more than 30 days after departure (if provided)

## Custom Validation Extensions

### AirportCodeValidator
- `MustBeValidAirportCode()` - Validates format and recognition
- `MustHaveDifferentAirports()` - Ensures origin ≠ destination
- `MustBeViableRoute()` - Checks commercial viability

### DateRangeValidator
- `MustBeWithinBookingWindow()` - Validates booking window
- `MustHaveReasonableTravelDuration()` - Validates trip duration

### PassengerValidator
- `MustHaveValidPassengerDistribution()` - Validates passenger rules

## Error Handling

### Validation Errors
- Returns HTTP 400 (Bad Request)
- Uses ASP.NET Core `ValidationProblemDetails`
- Provides field-specific error messages
- Groups multiple errors per field

### Error Response Format
```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "OriginCode": ["Airport code is not recognized"],
    "Adults": ["At least one adult passenger is required"]
  }
}
```

## Example API Calls

### Valid Flight Search
```http
GET /api/v1/flights/search?originCode=LAX&destinationCode=JFK&departureDate=2025-07-01&adults=2&children=1
```

### Invalid Flight Search (Same Origin/Destination)
```http
GET /api/v1/flights/search?originCode=LAX&destinationCode=LAX&departureDate=2025-07-01
```
Response: 400 Bad Request with validation errors

### Invalid Flight Search (Unknown Airport)
```http
GET /api/v1/flights/search?originCode=XXX&destinationCode=JFK&departureDate=2025-07-01
```
Response: 400 Bad Request - "Airport code is not recognized"

### Invalid Flight Search (Past Date)
```http
GET /api/v1/flights/search?originCode=LAX&destinationCode=JFK&departureDate=2025-01-01
```
Response: 400 Bad Request - "Departure date cannot be in the past"

## Business Rules Implemented

1. **Airport Recognition**: Only known airport codes are accepted
2. **Route Viability**: Routes must be commercially viable
3. **Passenger Distribution**: Infant/adult ratio enforcement
4. **Booking Window**: Reasonable advance booking timeframe
5. **Travel Duration**: Prevents extremely long trips
6. **Currency Support**: Limited to major currencies
7. **Price Limits**: Prevents unrealistic price alerts

## Integration with CQRS

The validation system is fully integrated with the CQRS pattern:

1. **Queries** are validated before execution
2. **Commands** are validated before execution  
3. **Pipeline Behaviors** handle cross-cutting concerns
4. **Domain Logic** remains pure and focused on business rules
5. **Separation of Concerns** between validation and business logic

## Performance Considerations

- Validation runs in pipeline before expensive operations
- Custom validators use efficient lookup mechanisms
- Caching is applied for repeated validations
- Early failure prevents unnecessary processing

## Future Enhancements

1. **Database Integration**: Real airport and airline code validation
2. **Dynamic Rules**: Configuration-driven validation rules
3. **Localization**: Multi-language error messages
4. **Rate Limiting**: Per-user validation limits
5. **Audit Logging**: Validation failure tracking
