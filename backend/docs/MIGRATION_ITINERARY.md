# Itinerary Aggregate Migration Notes

The Itinerary aggregate introduces two new tables: `Itineraries` and `ItineraryLegs`.

## EF Core Migration (to be generated)
Run (example):
```
dotnet ef migrations add AddItineraries --project src/FlightTracker.Infrastructure --startup-project src/FlightTracker.Api --output-dir Migrations
```
Apply:
```
dotnet ef database update --project src/FlightTracker.Infrastructure --startup-project src/FlightTracker.Api
```
## Schema Outline (Conceptual)
- Itineraries(Id PK, CreatedAt, TotalPriceAmount, TotalPriceCurrency)
- ItineraryLegs(ItineraryId FK, Sequence, FlightId, FlightNumber, AirlineCode, OriginCode, DestinationCode, DepartureUtc, ArrivalUtc, LegPriceAmount, LegPriceCurrency, CabinClass, Direction)

## Post Deployment
1. Deploy migration.
2. Warm cache: invoke /api/v1/itineraries/search on popular routes.
3. Monitor logs for pairing volume and performance metrics.

## Backward Compatibility
Existing flight search endpoints unchanged. Itinerary feature isolated to new controller.
