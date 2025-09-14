using FluentValidation;using FlightTracker.Api.Application.Queries;using FlightTracker.Domain.Enums;

namespace FlightTracker.Api.Application.Validation;

public class SearchItinerariesQueryValidator : AbstractValidator<SearchItinerariesQuery>
{
    public SearchItinerariesQueryValidator()
    {
        RuleFor(x=>x.OriginCode).NotEmpty().Length(3);
        RuleFor(x=>x.DestinationCode).NotEmpty().Length(3);
        RuleFor(x=>x).Must(x=>!string.Equals(x.OriginCode,x.DestinationCode,StringComparison.OrdinalIgnoreCase))
            .WithMessage("Origin and destination must differ");
        RuleFor(x=>x.DepartureDate.Date).GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-1));
        When(x=>x.ReturnDate.HasValue, () =>
        {
            RuleFor(x=>x.ReturnDate!.Value.Date).GreaterThan(x=>x.DepartureDate.Date);
        });
        RuleFor(x=>x.Options.Page).GreaterThan(0);
        RuleFor(x=>x.Options.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Options.MaxOutboundFlights).InclusiveBetween(1,200);
        RuleFor(x=>x.Options.MaxReturnFlights).InclusiveBetween(1,200);
        RuleFor(x=>x.Options.MaxCombinations).InclusiveBetween(1,2000);
        RuleFor(x=>x.Options.SortBy).IsInEnum();
        RuleFor(x=>x.Options.SortOrder).IsInEnum();
    }
}
