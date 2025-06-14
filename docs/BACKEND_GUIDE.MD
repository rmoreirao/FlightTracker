# Flight Tracker Backend Implementation Guide

*Created: June 14, 2025*

This guide provides architectural guidance, best practices, and implementation details for the Flight Tracker backend, focusing on the "Flight Search (Live Prices)" feature. It follows the technical requirements outlined in the Technical Requirements document while ensuring maintainability, operability, reusability, and security.

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Application Layers](#2-application-layers)
3. [Database Schema](#3-database-schema)
4. [API Design](#4-api-design)
5. [Implementation Best Practices](#5-implementation-best-practices)
6. [Testing Strategy](#6-testing-strategy)
7. [Security Considerations](#7-security-considerations)
8. [Operational Excellence](#8-operational-excellence)
9. [Implementation Roadmap](#9-implementation-roadmap)

---

## 1. Architecture Overview

The Flight Tracker backend follows a clean architecture approach with CQRS (Command Query Responsibility Segregation) pattern implementation via MediatR. This provides a clear separation of concerns, enables high performance, and makes the system maintainable and testable.

### High-Level Architecture

```
┌───────────────────────────┐
│     API Layer             │
│  (Controllers/Endpoints)  │
└───────────┬───────────────┘
            │
┌───────────▼───────────────┐
│    Application Layer      │
│   (Commands & Queries)    │
└───────────┬───────────────┘
            │
┌───────────▼───────────────┐
│     Domain Layer          │
│   (Core Business Logic)   │
└───────────┬───────────────┘
            │
┌───────────▼───────────────┐
│   Infrastructure Layer    │
│  (External Dependencies)  │
└───────────────────────────┘
```

### Key Components

- **ASP.NET Core API**: Minimal API endpoints and controllers where needed
- **GraphQL API**: HotChocolate implementation for flexible queries
- **CQRS with MediatR**: Separate command and query paths
- **FluentValidation**: Request validation
- **Serilog**: Structured logging
- **OpenTelemetry**: Distributed tracing and metrics
- **EF Core & Dapper**: Persistence with performance optimization
- **Azure Cache for Redis**: Response caching
- **Identity & Authorization**: Microsoft Entra ID integration

---

## 2. Application Layers

### 2.1 API Layer

The API layer serves as the entry point for client applications, responsible for:
- Request routing
- Parameter binding
- Authentication/Authorization
- Response formatting
- Rate limiting
- GraphQL schema definition

#### REST API Structure

For REST endpoints, we use a combination of ASP.NET Core Minimal APIs (for simple endpoints) and controllers (for more complex scenarios).

```csharp
// Example of Minimal API endpoint
app.MapGet("/api/v1/flights/search", async (
    [FromQuery] string originCode,
    [FromQuery] string destinationCode,
    [FromQuery] DateTime departureDate,
    [FromQuery] DateTime? returnDate,
    IMediator mediator) =>
{
    var query = new SearchFlightsQuery(originCode, destinationCode, departureDate, returnDate);
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("SearchFlights")
.WithOpenApi()
.RequireRateLimiting("search")
.WithTags("Flights");
```

#### GraphQL Structure

For GraphQL, we use HotChocolate with code-first approach:

```csharp
public class FlightQuery
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<FlightResult>> GetFlights(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        [Service] IMediator mediator)
    {
        var query = new SearchFlightsQuery(originCode, destinationCode, departureDate, returnDate);
        var result = await mediator.Send(query);
        return result.AsQueryable();
    }
}
```

### 2.2 Application Layer

The application layer contains application logic and orchestrates domain objects to fulfill use cases:

- **Commands**: Write operations (not primary for search feature)
- **Queries**: Read operations (primary for search feature)
- **Validators**: Input validation rules
- **DTOs**: Data transfer objects for API responses
- **Mappers**: Object mapping between layers

```csharp
public record SearchFlightsQuery(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    DateTime? ReturnDate) : IRequest<SearchFlightsResult>;

public class SearchFlightsQueryValidator : AbstractValidator<SearchFlightsQuery>
{
    public SearchFlightsQueryValidator()
    {
        RuleFor(x => x.OriginCode).NotEmpty().Length(3);
        RuleFor(x => x.DestinationCode).NotEmpty().Length(3);
        RuleFor(x => x.DepartureDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date);
        RuleFor(x => x.ReturnDate).GreaterThanOrEqualTo(x => x.DepartureDate)
            .When(x => x.ReturnDate.HasValue);
    }
}

public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, SearchFlightsResult>
{
    private readonly IFlightService _flightService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchFlightsQueryHandler> _logger;

    public SearchFlightsQueryHandler(
        IFlightService flightService,
        ICacheService cacheService,
        ILogger<SearchFlightsQueryHandler> logger)
    {
        _flightService = flightService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SearchFlightsResult> Handle(
        SearchFlightsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"flights:{request.OriginCode}-{request.DestinationCode}:{request.DepartureDate:yyyyMMdd}:{request.ReturnDate:yyyyMMdd}";
        
        // Try get from cache
        var cachedResult = await _cacheService.GetAsync<SearchFlightsResult>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Cache hit for flight search {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogInformation("Cache miss for flight search {CacheKey}", cacheKey);
        
        using var activity = ActivitySource.StartActivity("SearchFlights");
        activity?.SetTag("origin", request.OriginCode);
        activity?.SetTag("destination", request.DestinationCode);
        
        var flights = await _flightService.SearchFlightsAsync(
            request.OriginCode,
            request.DestinationCode,
            request.DepartureDate,
            request.ReturnDate,
            cancellationToken);

        var result = new SearchFlightsResult(
            flights,
            DateTime.UtcNow);

        // Cache result for 5 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

### 2.3 Domain Layer

The domain layer contains the core business logic, domain entities, and business rules:

- **Entities**: Domain models
- **Services**: Domain services encapsulating complex business logic
- **Value Objects**: Immutable objects with value semantics
- **Domain Events**: Changes in domain state

```csharp
public class Flight
{
    public string FlightNumber { get; private set; }
    public string AirlineCode { get; private set; }
    public string AirlineName { get; private set; }
    public Airport Origin { get; private set; }
    public Airport Destination { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public TimeSpan Duration => ArrivalTime - DepartureTime;
    public List<FlightSegment> Segments { get; private set; } = new();
    public Money Price { get; private set; }
    public CabinClass CabinClass { get; private set; }
    public string DeepLink { get; private set; }

    // Constructor and methods...
}

public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(currency));
    }

    // Value object equality and operations...
}
```

### 2.4 Infrastructure Layer

The infrastructure layer implements interfaces defined in the domain and application layers:

- **Repositories**: Data access implementations
- **External Service Clients**: API clients for flight providers
- **Caching**: Redis implementation
- **Messaging**: Service bus or in-process pub/sub
- **Persistence**: Database context and configurations

```csharp
public class FlightProviderService : IFlightService
{
    private readonly IEnumerable<IFlightProvider> _flightProviders;
    private readonly ILogger<FlightProviderService> _logger;

    public FlightProviderService(
        IEnumerable<IFlightProvider> flightProviders,
        ILogger<FlightProviderService> logger)
    {
        _flightProviders = flightProviders;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Flight>> SearchFlightsAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        CancellationToken cancellationToken)
    {
        var tasks = _flightProviders.Select(provider => 
            SearchProviderWithFallbackAsync(provider, originCode, destinationCode, departureDate, returnDate, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(x => x)
            .ToList()
            .AsReadOnly();
    }

    private async Task<IReadOnlyList<Flight>> SearchProviderWithFallbackAsync(
        IFlightProvider provider,
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        CancellationToken cancellationToken)
    {
        try
        {
            return await provider.SearchFlightsAsync(
                originCode,
                destinationCode,
                departureDate,
                returnDate,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights with provider {ProviderName}", 
                provider.GetType().Name);
            return Array.Empty<Flight>();
        }
    }
}

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (value.IsNull)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from cache for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serialized, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
        }
    }
}
```

---

## 3. Database Schema

For the Flight Search feature, we need a schema that supports both the live search and historical data tracking.

### Key Tables

#### 1. Airports

```sql
CREATE TABLE airports (
    code CHAR(3) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    country VARCHAR(100) NOT NULL,
    latitude DECIMAL(10, 7),
    longitude DECIMAL(10, 7),
    timezone VARCHAR(50)
);
```

#### 2. Airlines

```sql
CREATE TABLE airlines (
    code CHAR(2) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    logo_url VARCHAR(255)
);
```

#### 3. Flight Queries

```sql
CREATE TABLE flight_queries (
    id UUID PRIMARY KEY,
    origin_code CHAR(3) NOT NULL REFERENCES airports(code),
    destination_code CHAR(3) NOT NULL REFERENCES airports(code),
    departure_date DATE NOT NULL,
    return_date DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    search_count INT NOT NULL DEFAULT 1,
    last_searched_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_flight_queries_routes ON flight_queries(origin_code, destination_code);
CREATE INDEX idx_flight_queries_dates ON flight_queries(departure_date, return_date);
```

#### 4. Price Snapshots (TimescaleDB Hypertable)

```sql
CREATE TABLE price_snapshots (
  id BIGSERIAL,
  query_id UUID NOT NULL REFERENCES flight_queries(id),
  airline_code CHAR(2) NOT NULL REFERENCES airlines(code),
  cabin VARCHAR(10) NOT NULL,
  price_cents INT NOT NULL,
  currency CHAR(3) NOT NULL,
  deep_link TEXT,
  flight_number VARCHAR(10),
  departure_time TIMESTAMPTZ,
  arrival_time TIMESTAMPTZ,
  stops INT NOT NULL DEFAULT 0,
  collected_at TIMESTAMPTZ NOT NULL,
  PRIMARY KEY (query_id, airline_code, cabin, collected_at)
);

SELECT create_hypertable('price_snapshots', 'collected_at');
CREATE INDEX idx_price_snapshots_query ON price_snapshots(query_id);
```

### Entity Framework Core Configuration

```csharp
public class FlightDbContext : DbContext
{
    public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
    {
    }

    public DbSet<Airport> Airports { get; set; }
    public DbSet<Airline> Airlines { get; set; }
    public DbSet<FlightQuery> FlightQueries { get; set; }
    public DbSet<PriceSnapshot> PriceSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("timescaledb");

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("CHAR(3)");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Airline>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnType("CHAR(2)");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<FlightQuery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            entity.HasOne(e => e.Origin)
                .WithMany()
                .HasForeignKey(e => e.OriginCode);
                
            entity.HasOne(e => e.Destination)
                .WithMany()
                .HasForeignKey(e => e.DestinationCode);
                
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.LastSearchedAt).HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<PriceSnapshot>(entity =>
        {
            entity.HasKey(e => new { e.QueryId, e.AirlineCode, e.Cabin, e.CollectedAt });
            
            entity.HasOne(e => e.FlightQuery)
                .WithMany(q => q.PriceSnapshots)
                .HasForeignKey(e => e.QueryId);
                
            entity.HasOne(e => e.Airline)
                .WithMany()
                .HasForeignKey(e => e.AirlineCode);
        });
    }
}
```

---

## 4. API Design

### 4.1 REST API Endpoints

#### Flight Search Endpoint

```
GET /api/v1/flights/search
```

Parameters:
- `originCode` (required): 3-letter IATA airport code
- `destinationCode` (required): 3-letter IATA airport code
- `departureDate` (required): ISO 8601 date format (YYYY-MM-DD)
- `returnDate` (optional): ISO 8601 date format (YYYY-MM-DD)
- `cabins` (optional): Comma-separated list of cabin classes (economy, premium_economy, business, first)
- `adults` (optional, default=1): Number of adult passengers
- `children` (optional, default=0): Number of child passengers
- `infants` (optional, default=0): Number of infant passengers

Response:
```json
{
  "flights": [
    {
      "airlineCode": "AA",
      "airlineName": "American Airlines",
      "flightNumber": "AA123",
      "origin": {
        "code": "LAX",
        "name": "Los Angeles International Airport"
      },
      "destination": {
        "code": "JFK",
        "name": "John F. Kennedy International Airport"
      },
      "departureTime": "2025-07-01T08:30:00Z",
      "arrivalTime": "2025-07-01T17:00:00Z",
      "duration": "08:30:00",
      "stops": 1,
      "price": {
        "amount": 299.99,
        "currency": "USD"
      },
      "cabin": "economy",
      "deepLink": "https://example.com/book/AA123"
    }
  ],
  "lastUpdated": "2025-06-14T12:34:56Z",
  "totalResults": 1,
  "currency": "USD"
}
```

### 4.2 GraphQL Schema

```graphql
type Query {
  flights(
    originCode: String!
    destinationCode: String!
    departureDate: Date!
    returnDate: Date
    cabins: [CabinClass!]
    adults: Int = 1
    children: Int = 0
    infants: Int = 0
  ): FlightConnection!
}

type FlightConnection {
  edges: [FlightEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
  lastUpdated: DateTime!
}

type FlightEdge {
  node: Flight!
  cursor: String!
}

type Flight {
  id: ID!
  airlineCode: String!
  airlineName: String!
  flightNumber: String!
  origin: Airport!
  destination: Airport!
  departureTime: DateTime!
  arrivalTime: DateTime!
  duration: Duration!
  stops: Int!
  price: Money!
  cabin: CabinClass!
  deepLink: String
}

type Airport {
  code: String!
  name: String!
  city: String!
  country: String!
}

type Money {
  amount: Float!
  currency: String!
}

scalar Duration
scalar Date
scalar DateTime

enum CabinClass {
  ECONOMY
  PREMIUM_ECONOMY
  BUSINESS
  FIRST
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}
```

### 4.3 API Documentation

Use Swagger/OpenAPI for REST API documentation and GraphQL Playground/Banana Cake Pop for GraphQL documentation.

```csharp
// Configure Swagger in Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Flight Tracker API",
        Version = "v1",
        Description = "API for searching flights and retrieving price history",
        Contact = new OpenApiContact
        {
            Name = "Flight Tracker Team",
            Email = "team@flighttracker.example.com"
        }
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api://flighttracker/flights.read", "Read flight information" }
                }
            }
        }
    });
    
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
```

---

## 5. Implementation Best Practices

### 5.1 C# Coding Standards

1. **Use C# 9 Features**:
   - Record types for DTOs and immutable objects
   - Init-only properties for immutability
   - Pattern matching for cleaner code
   - Target-typed new expressions
   - Top-level statements where appropriate

2. **Naming Conventions**:
   - PascalCase for types, methods, properties, constants
   - camelCase for parameters and local variables
   - Prefixes: `I` for interfaces, `Abstract` for abstract classes
   - Suffixes: `Controller` for controllers, `Service` for services, `Repository` for repositories

3. **Error Handling**:
   - Use exceptions for exceptional scenarios only
   - Return results or optional types for expected failure cases
   - Provide meaningful exception messages
   - Catch specific exceptions, not general Exception unless at boundaries

4. **Asynchronous Programming**:
   - Use async/await consistently
   - Avoid blocking calls (`.Result`, `.Wait()`)
   - Use `Task.WhenAll` for parallel operations
   - Pass CancellationToken through the call stack

5. **Dependency Injection**:
   - Register dependencies with appropriate lifetimes (transient, scoped, singleton)
   - Use constructor injection
   - Avoid service locator pattern
   - Register implementations by interfaces

### 5.2 CQRS with MediatR Best Practices

1. **Commands and Queries**:
   - Commands: Represent intent to change state, void or minimal response
   - Queries: Request data, no state changes

2. **Handlers**:
   - Single responsibility
   - Keep handlers small and focused
   - Use pipeline behaviors for cross-cutting concerns

3. **Validation**:
   - Use FluentValidation for input validation
   - Validate before processing in a pipeline behavior

4. **Performance**:
   - Use caching for queries
   - Optimize database queries with Dapper for read-heavy operations
   - Use projections to return only needed data

### 5.3 Data Access Best Practices

1. **Entity Framework Core**:
   - Use for transactional data changes
   - Configure entity relationships in OnModelCreating
   - Use migrations for schema changes
   - Avoid lazy loading in APIs

2. **Dapper**:
   - Use for complex read queries
   - Leverage projections for optimal data retrieval
   - Use multi-mapping for related entities

3. **Database Performance**:
   - Create appropriate indexes
   - Use pagination for large result sets
   - Use query timeouts to prevent long-running queries
   - Implement database query caching with Redis

### 5.4 API Design Best Practices

1. **RESTful Principles**:
   - Use appropriate HTTP methods
   - Return appropriate status codes
   - Version your APIs
   - Use content negotiation

2. **GraphQL Best Practices**:
   - Define clear schema with meaningful types
   - Implement pagination for lists
   - Use DataLoader for efficient data fetching
   - Implement proper error handling

3. **Security**:
   - Validate all inputs
   - Implement proper authentication and authorization
   - Use HTTPS for all endpoints
   - Implement rate limiting

4. **Performance**:
   - Implement caching where appropriate
   - Use compression for responses
   - Return only required data (projection)
   - Monitor endpoint performance

---

## 6. Testing Strategy

### 6.1 Unit Testing

Use xUnit with AutoFixture and NSubstitute for unit testing:

```csharp
public class SearchFlightsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IFlightService _flightService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchFlightsQueryHandler> _logger;
    private readonly SearchFlightsQueryHandler _handler;

    public SearchFlightsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _flightService = _fixture.Freeze<IFlightService>();
        _cacheService = _fixture.Freeze<ICacheService>();
        _logger = _fixture.Freeze<ILogger<SearchFlightsQueryHandler>>();
        _handler = new SearchFlightsQueryHandler(_flightService, _cacheService, _logger);
    }

    [Fact]
    public async Task Handle_WithCachedResult_ReturnsCachedResult()
    {
        // Arrange
        var query = _fixture.Create<SearchFlightsQuery>();
        var cachedResult = _fixture.Create<SearchFlightsResult>();
        
        _cacheService.GetAsync<SearchFlightsResult>(Arg.Any<string>())
            .Returns(cachedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(cachedResult);
        await _flightService.DidNotReceive()
            .SearchFlightsAsync(Arg.Any<string>(), Arg.Any<string>(), 
                Arg.Any<DateTime>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutCachedResult_ReturnsFromService()
    {
        // Arrange
        var query = _fixture.Create<SearchFlightsQuery>();
        var flights = _fixture.CreateMany<Flight>().ToList();
        
        _cacheService.GetAsync<SearchFlightsResult>(Arg.Any<string>())
            .Returns((SearchFlightsResult)null);
            
        _flightService.SearchFlightsAsync(
                query.OriginCode, query.DestinationCode, 
                query.DepartureDate, query.ReturnDate, Arg.Any<CancellationToken>())
            .Returns(flights);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Flights.Should().BeEquivalentTo(flights);
        
        await _cacheService.Received(1)
            .SetAsync(Arg.Any<string>(), Arg.Is<SearchFlightsResult>(r => r.Flights.SequenceEqual(flights)), 
                Arg.Any<TimeSpan>());
    }
}
```

### 6.2 Integration Testing

Use WebApplicationFactory and Testcontainers for integration testing:

```csharp
public class FlightSearchApiTests : IClassFixture<FlightApiWebApplicationFactory>
{
    private readonly FlightApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FlightSearchApiTests(FlightApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchFlights_WithValidParameters_ReturnsResults()
    {
        // Arrange
        var originCode = "LAX";
        var destinationCode = "JFK";
        var departureDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/flights/search?originCode={originCode}&destinationCode={destinationCode}&departureDate={departureDate}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SearchFlightsResponse>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
        result.Should().NotBeNull();
        result.Flights.Should().NotBeEmpty();
    }
}

public class FlightApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly RedisContainer _redisContainer;

    public FlightApiWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("timescale/timescaledb:latest-pg14")
            .WithDatabase("flighttracker_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:alpine")
            .Build();

        _dbContainer.StartAsync().GetAwaiter().GetResult();
        _redisContainer.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace real flight providers with mocks
            services.RemoveAll<IFlightProvider>();
            services.AddSingleton<IFlightProvider, MockFlightProvider>();
        });

        base.ConfigureWebHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContainer.DisposeAsync().GetAwaiter().GetResult();
            _redisContainer.DisposeAsync().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }
}
```

### 6.3 API Contract Testing

Use Playwright for API contract testing:

```typescript
// tests/api/flights.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Flight Search API', () => {
  test('should return flight results for valid search', async ({ request }) => {
    const response = await request.get('/api/v1/flights/search', {
      params: {
        originCode: 'LAX',
        destinationCode: 'JFK',
        departureDate: '2025-07-01',
      }
    });
    
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    
    expect(data.flights.length).toBeGreaterThan(0);
    expect(data.lastUpdated).toBeDefined();
    
    // Validate the structure of the first flight
    const flight = data.flights[0];
    expect(flight.airlineCode).toBeDefined();
    expect(flight.flightNumber).toBeDefined();
    expect(flight.departureTime).toBeDefined();
    expect(flight.arrivalTime).toBeDefined();
    expect(flight.price.amount).toBeGreaterThan(0);
  });
  
  test('should return 400 for invalid search parameters', async ({ request }) => {
    const response = await request.get('/api/v1/flights/search', {
      params: {
        originCode: 'INVALID',
        destinationCode: 'JFK',
        departureDate: '2025-07-01',
      }
    });
    
    expect(response.status()).toBe(400);
    const data = await response.json();
    expect(data.errors).toBeDefined();
  });
});
```

### 6.4 Performance Testing

Use k6 for performance testing:

```javascript
// tests/performance/flight-search.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },  // Ramp up to 20 users
    { duration: '1m', target: 20 },   // Stay at 20 users for 1 minute
    { duration: '30s', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p95<800'],  // 95% of requests should be below 800ms
    http_req_failed: ['rate<0.01'],  // Less than 1% of requests should fail
  },
};

export default function() {
  const originCodes = ['LAX', 'JFK', 'ORD', 'SFO', 'ATL'];
  const destinationCodes = ['LHR', 'CDG', 'FRA', 'AMS', 'MAD'];
  
  const originCode = originCodes[Math.floor(Math.random() * originCodes.length)];
  const destinationCode = destinationCodes[Math.floor(Math.random() * destinationCodes.length)];
  
  const departureDate = new Date();
  departureDate.setDate(departureDate.getDate() + 30 + Math.floor(Math.random() * 60));
  
  const formattedDate = departureDate.toISOString().split('T')[0];
  
  const response = http.get(
    `http://localhost:5000/api/v1/flights/search?originCode=${originCode}&destinationCode=${destinationCode}&departureDate=${formattedDate}`
  );
  
  check(response, {
    'status is 200': (r) => r.status === 200,
    'has flights': (r) => JSON.parse(r.body).flights.length > 0,
  });
  
  sleep(1);
}
```

---

## 7. Security Considerations

### 7.1 Authentication & Authorization

Use Microsoft Entra ID for authentication with appropriate scopes:

```csharp
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationOptions)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserRole", policy =>
        policy.RequireRole("User"));
        
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
        
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// In API endpoints
app.MapGet("/api/v1/admin/ingestions", async (IMediator mediator) =>
{
    var query = new GetIngestionStatusQuery();
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.RequireAuthorization("RequireAdminRole");
```

### 7.2 Rate Limiting

Implement rate limiting to prevent abuse:

```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                context.User.Identity.Name ?? "authenticated",
                partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 600,
                    Window = TimeSpan.FromHours(1)
                });
        }
        
        return RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                Window = TimeSpan.FromHours(1)
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            title = "Too many requests",
            status = 429,
            detail = "You've exceeded the allowed request rate. Please try again later."
        });
    };
});
```

### 7.3 Input Validation

Use FluentValidation for thorough input validation:

```csharp
// Register FluentValidation in Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SearchFlightsQueryValidator>();

// Implement validation behavior for MediatR
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}

// Register validation behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### 7.4 Secret Management

Use Azure Key Vault for storing secrets:

```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// In service implementations
public class KiwiFlightProvider : IFlightProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public KiwiFlightProvider(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Get API key from Key Vault
        _httpClient.DefaultRequestHeaders.Add("apikey", _configuration["FlightApis:Kiwi:ApiKey"]);
        _httpClient.BaseAddress = new Uri(_configuration["FlightApis:Kiwi:BaseUrl"]);
    }
    
    // Implementation...
}
```

### 7.5 Data Protection

Implement proper data protection:

```csharp
// In Program.cs
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(new Uri(builder.Configuration["DataProtection:BlobStorage"]))
    .ProtectKeysWithAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/keys/DataProtection"),
        new DefaultAzureCredential());
```

---

## 8. Operational Excellence

### 8.1 Logging

Implement structured logging with Serilog:

```csharp
// In Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.AzureAnalytics(
        workspaceId: context.Configuration["Logging:AzureAnalytics:WorkspaceId"],
        authenticationId: context.Configuration["Logging:AzureAnalytics:AuthenticationId"],
        logName: "FlightTrackerApi"));

// In handlers and services
public async Task<SearchFlightsResult> Handle(
    SearchFlightsQuery request,
    CancellationToken cancellationToken)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["OriginCode"] = request.OriginCode,
        ["DestinationCode"] = request.DestinationCode,
        ["DepartureDate"] = request.DepartureDate,
        ["ReturnDate"] = request.ReturnDate
    });
    
    _logger.LogInformation("Processing flight search request");
    
    // Implementation...
}
```

### 8.2 Monitoring and Observability

Implement OpenTelemetry for distributed tracing and metrics:

```csharp
// In Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("FlightTracker.API")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("FlightTracker.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("FlightTracker.API")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("FlightTracker.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
    });

// Create an ActivitySource
private static readonly ActivitySource ActivitySource = new ActivitySource("FlightTracker.API");

// Use activity for tracing
public async Task<SearchFlightsResult> Handle(
    SearchFlightsQuery request,
    CancellationToken cancellationToken)
{
    using var activity = ActivitySource.StartActivity("SearchFlights");
    activity?.SetTag("origin", request.OriginCode);
    activity?.SetTag("destination", request.DestinationCode);
    
    // Implementation...
}
```

### 8.3 Health Checks

Implement health checks for dependencies:

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        name: "database", 
        tags: new[] { "ready" })
    .AddRedis(builder.Configuration.GetConnectionString("Redis"), 
        name: "redis", 
        tags: new[] { "ready" })
    .AddUrlGroup(new Uri(builder.Configuration["FlightApis:Kiwi:HealthCheckUrl"]), 
        name: "kiwi-api", 
        tags: new[] { "live" });

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 8.4 Circuit Breakers

Implement circuit breakers for external services:

```csharp
// In Program.cs
builder.Services.AddHttpClient<IFlightProvider, KiwiFlightProvider>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));
}
```

### 8.5 Caching Strategy

Implement a comprehensive caching strategy:

```csharp
// In Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FlightTracker:";
});

// Response caching middleware
app.UseResponseCaching();

// Use output cache for specific endpoints
app.MapGet("/api/v1/airports", async (IMediator mediator) =>
{
    var query = new GetAirportsQuery();
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromDays(1)));
```

---

## 9. Implementation Roadmap

### Phase 1: Setup & Foundation (Week 1)
- [ ] Set up project structure and solution
- [ ] Configure EF Core with TimescaleDB
- [ ] Implement authentication with Microsoft Entra ID
- [ ] Configure logging and monitoring

### Phase 2: Core Domain (Week 2)
- [ ] Implement domain models
- [ ] Create database migrations
- [ ] Set up CQRS with MediatR
- [ ] Create validation rules

### Phase 3: Flight Search API (Weeks 3-4)
- [ ] Implement flight provider interfaces
- [ ] Create flight search query handlers
- [ ] Add REST endpoints for flight search
- [ ] Implement caching for search results

### Phase 4: GraphQL API (Week 5)
- [ ] Implement GraphQL schema
- [ ] Create resolvers for flight search
- [ ] Add filtering and sorting capabilities
- [ ] Implement data loaders for efficiency

### Phase 5: Testing & Optimization (Week 6)
- [ ] Write unit tests for all components
- [ ] Implement integration tests
- [ ] Create performance tests
- [ ] Optimize queries and caching strategy

### Phase 6: Deployment & CI/CD (Week 7)
- [ ] Set up GitHub Actions pipeline
- [ ] Create Azure infrastructure with Bicep
- [ ] Configure blue-green deployment
- [ ] Implement monitoring and alerts
