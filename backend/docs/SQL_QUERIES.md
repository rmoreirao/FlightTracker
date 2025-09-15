# Queries

### Look for round-trip flights from AMS to GIG
```sql
select	*
from "Itineraries" it
		inner join "ItineraryLegs" itdep
			on it."Id" = itdep."ItineraryId"
			and itdep."Direction" = 0
		inner join "ItineraryLegs"  itret
			ON it."Id" = itret."ItineraryId"
			and itret."Direction" = 1
where	itdep."OriginCode" = 'AMS'
and 	itret."OriginCode" = 'GIG'
```