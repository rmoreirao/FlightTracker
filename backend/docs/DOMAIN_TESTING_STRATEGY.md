# Flight Tracker Domain/Unit Testing Strategy

## Overview

This document outlines the comprehensive testing strategy for the Flight Tracker Domain layer, implementing the technical requirement:

**Domain/Unit Testing**: xUnit + AutoFixture with 80% coverage gate

## 1. Testing Architecture

### Technology Stack
- **Testing Framework**: xUnit.net
- **Test Data Generation**: AutoFixture
- **Mocking**: NSubstitute
- **Coverage**: Coverlet + ReportGenerator
- **Coverage Gate**: 80% minimum

### Project Structure
```
tests/
├── FlightTracker.Domain.Tests/           # Domain unit tests
│   ├── Entities/                         # Entity tests
│   ├── ValueObjects/                     # Value object tests
│   ├── Services/                         # Domain service tests
│   ├── Events/                           # Domain event tests
│   ├── TestFixtures/                     # Shared test data
│   └── TestHelpers/                      # Test utilities
├── FlightTracker.Api.Tests/              # Application layer tests
│   ├── Queries/                          # Query handler tests
│   ├── Commands/                         # Command handler tests
│   ├── Validators/                       # Validation tests
│   └── Controllers/                      # Controller tests
└── FlightTracker.IntegrationTests/       # Integration tests
```

## 2. Domain Layer Testing Strategy

### 2.1 Entity Testing

#### Test Categories for Each Entity:
1. **Constructor Validation Tests**
   - Valid parameter combinations
   - Invalid parameter validation
   - Null parameter handling
   - Business rule enforcement

2. **Behavior Tests**
   - Method execution with valid inputs
   - Method execution with invalid inputs
   - State changes after method calls
   - Business logic validation

3. **Invariant Tests**
   - Entity remains in valid state after operations
   - Immutability where required
   - Calculated properties work correctly

#### Entities to Test:
- ✅ **Flight**: Complex entity with multiple dependencies
- ✅ **FlightSegment**: Related entity with business rules
- ✅ **FlightQuery**: Analytics tracking entity
- ✅ **PriceSnapshot**: Historical data entity
- ✅ **Airport**: Basic entity with validation
- ✅ **Airline**: Simple entity with business rules

### 2.2 Value Object Testing

#### Test Categories:
1. **Equality Tests**
   - Structural equality
   - Hash code consistency
   - Equality operators

2. **Immutability Tests**
   - Objects cannot be modified after creation
   - Methods return new instances

3. **Validation Tests**
   - Constructor validation
   - Format validation
   - Business rule validation

#### Value Objects to Test:
- ✅ **Money**: Currency operations and validation
- ✅ **DateRange**: Date calculations and validation
- ✅ **RouteKey**: Route identification and equality

### 2.3 Service Interface Testing

#### Test Strategy:
- Test interface contracts using mocks
- Verify behavior expectations
- Test error handling scenarios

#### Services to Test:
- ✅ **IFlightService**: Flight search operations
- ✅ **ICacheService**: Caching operations
- ✅ **IPriceAnalysisService**: Price analysis operations

### 2.4 Domain Event Testing

#### Test Categories:
1. **Event Creation Tests**
   - Event properties are set correctly
   - Event ID and timestamp generation
   - Event data immutability

2. **Event Handler Integration**
   - Event publishing mechanism
   - Event handler registration

#### Events to Test:
- ✅ **FlightSearchPerformedEvent**
- ✅ **PriceSnapshotCollectedEvent**

## 3. AutoFixture Configuration

### 3.1 Custom Specimens
```csharp
// Airport code generator
public class AirportCodeSpecimen : ISpecimenBuilder
{
    private static readonly string[] ValidCodes = { "LAX", "JFK", "LHR", "CDG", "FRA" };
    
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo pi && pi.Name.EndsWith("Code") && pi.PropertyType == typeof(string))
        {
            return ValidCodes[Random.Shared.Next(ValidCodes.Length)];
        }
        return new NoSpecimen();
    }
}
```

### 3.2 Entity Builders
```csharp
public static class FlightBuilder
{
    public static Flight CreateValid(IFixture fixture)
    {
        var origin = fixture.Create<Airport>();
        var destination = fixture.Create<Airport>();
        while (destination.Code == origin.Code)
        {
            destination = fixture.Create<Airport>();
        }
        
        var departureTime = DateTime.UtcNow.AddDays(30).AddHours(fixture.Create<int>() % 24);
        var arrivalTime = departureTime.AddHours(fixture.Create<int>() % 12 + 1);
        
        return new Flight(
            fixture.Create<string>(),
            fixture.Create<string>(),
            fixture.Create<string>(),
            origin,
            destination,
            departureTime,
            arrivalTime,
            fixture.Create<Money>(),
            fixture.Create<CabinClass>());
    }
}
```

## 4. Test Categories and Coverage Targets

### 4.1 Coverage Targets by Component

| Component | Target Coverage | Priority |
|-----------|-----------------|----------|
| Entities | 85% | High |
| Value Objects | 90% | High |
| Service Interfaces | 80% | Medium |
| Domain Events | 75% | Medium |
| Enums | 70% | Low |

### 4.2 Test Types

#### Happy Path Tests (40% of tests)
- Valid constructor parameters
- Successful method execution
- Correct calculations and transformations

#### Validation Tests (35% of tests)
- Invalid parameters
- Boundary conditions
- Business rule violations

#### Edge Case Tests (15% of tests)
- Null/empty values
- Extreme values
- Concurrent scenarios

#### Integration Tests (10% of tests)
- Cross-entity interactions
- Complex business scenarios
- End-to-end domain flows

## 5. Testing Patterns and Best Practices

### 5.1 Test Naming Convention
```csharp
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Constructor_WithValidParameters_CreatesFlightSuccessfully
- AddSegment_WithNullSegment_ThrowsArgumentNullException
- UpdatePrice_WithValidMoney_UpdatesPriceCorrectly
```

### 5.2 AAA Pattern (Arrange, Act, Assert)
```csharp
[Fact]
public void Constructor_WithValidParameters_CreatesFlightSuccessfully()
{
    // Arrange
    var fixture = new Fixture();
    var origin = fixture.Create<Airport>();
    var destination = fixture.Create<Airport>();
    var departureTime = DateTime.UtcNow.AddDays(30);
    var arrivalTime = departureTime.AddHours(5);
    var price = fixture.Create<Money>();
    var cabinClass = CabinClass.Economy;

    // Act
    var flight = new Flight(
        "AA123",
        "AA",
        "American Airlines",
        origin,
        destination,
        departureTime,
        arrivalTime,
        price,
        cabinClass);

    // Assert
    flight.Should().NotBeNull();
    flight.FlightNumber.Should().Be("AA123");
    flight.Origin.Should().Be(origin);
    flight.Destination.Should().Be(destination);
    flight.Price.Should().Be(price);
}
```

### 5.3 Test Data Management
```csharp
public class DomainTestFixture
{
    public IFixture Fixture { get; }
    
    public DomainTestFixture()
    {
        Fixture = new Fixture();
        
        // Configure AutoFixture
        Fixture.Customize(new AutoNSubstituteCustomization());
        Fixture.Customizations.Add(new AirportCodeSpecimen());
        Fixture.Customizations.Add(new ValidDateTimeSpecimen());
        
        // Register entity builders
        Fixture.Register(() => FlightBuilder.CreateValid(Fixture));
    }
}
```

## 6. Coverage Measurement and Gates

### 6.1 Coverage Tools Configuration
```xml
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="ReportGenerator" Version="5.1.19" />
```

### 6.2 Coverage Collection
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

### 6.3 CI/CD Integration
```yaml
# GitHub Actions workflow
- name: Run Domain Tests with Coverage
  run: |
    dotnet test tests/FlightTracker.Domain.Tests/ \
      --collect:"XPlat Code Coverage" \
      --results-directory ./TestResults/ \
      --logger "trx;LogFileName=domain-tests.trx"

- name: Generate Coverage Report
  run: |
    reportgenerator \
      -reports:"TestResults/*/coverage.cobertura.xml" \
      -targetdir:"CoverageReport" \
      -reporttypes:"Html;Cobertura;JsonSummary"

- name: Check Coverage Gate
  run: |
    python scripts/check-coverage.py \
      --threshold 80 \
      --report CoverageReport/Summary.json
```

## 7. Test Organization Strategy

### 7.1 Test Classes Organization
```csharp
// One test class per domain class
public class FlightTests : IClassFixture<DomainTestFixture>
{
    private readonly DomainTestFixture _fixture;
    
    public FlightTests(DomainTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    public class ConstructorTests : FlightTests
    {
        public ConstructorTests(DomainTestFixture fixture) : base(fixture) { }
        // Constructor-specific tests
    }
    
    public class BusinessMethodTests : FlightTests
    {
        public BusinessMethodTests(DomainTestFixture fixture) : base(fixture) { }
        // Business method tests
    }
}
```

### 7.2 Shared Test Infrastructure
```csharp
public static class TestConstants
{
    public static readonly string[] ValidAirportCodes = { "LAX", "JFK", "LHR" };
    public static readonly string[] ValidAirlineCodes = { "AA", "BA", "LH" };
    public static readonly string[] ValidCurrencies = { "USD", "EUR", "GBP" };
}

public static class TestDataBuilder
{
    public static Airport CreateAirport(string code = "LAX") => new(code, "Test Airport", "Test City", "Test Country");
    public static Money CreateMoney(decimal amount = 100, string currency = "USD") => new(amount, currency);
}
```

## 8. Implementation Phases

### Phase 1: Infrastructure Setup (Week 1)
- ✅ Create test projects
- ✅ Configure NuGet packages
- ✅ Set up AutoFixture with customizations
- ✅ Create base test classes and fixtures

### Phase 2: Entity Tests (Week 1-2)
- ✅ Flight entity comprehensive tests
- ✅ FlightSegment entity tests
- ✅ FlightQuery entity tests
- ✅ PriceSnapshot entity tests
- ✅ Airport and Airline entity tests

### Phase 3: Value Object Tests (Week 2)
- ✅ Money value object tests
- ✅ DateRange value object tests
- ✅ RouteKey value object tests

### Phase 4: Service Interface Tests (Week 2)
- ✅ IFlightService contract tests
- ✅ ICacheService contract tests
- ✅ IPriceAnalysisService contract tests

### Phase 5: Coverage and Quality (Week 3)
- ✅ Achieve 80% coverage target
- ✅ Set up coverage gates in CI/CD
- ✅ Code quality improvements based on coverage gaps

## 9. Quality Gates and Metrics

### 9.1 Coverage Gates
- **Domain Assembly**: Minimum 80% line coverage
- **Critical Paths**: Minimum 90% coverage for business logic
- **Public API**: 100% coverage for public methods

### 9.2 Quality Metrics
- **Test Execution Time**: < 30 seconds for full domain test suite
- **Test Reliability**: 0% flaky tests tolerance
- **Maintainability**: Max 50 lines per test method

### 9.3 Reporting
```json
{
  "coverage": {
    "line": 85.2,
    "branch": 78.9,
    "method": 92.1
  },
  "tests": {
    "total": 156,
    "passed": 156,
    "failed": 0,
    "skipped": 0
  },
  "performance": {
    "execution_time": "00:00:23.456",
    "slowest_test": "FlightTests.ComplexBusinessScenario (2.1s)"
  }
}
```

## 10. Continuous Improvement

### 10.1 Coverage Monitoring
- Weekly coverage reports
- Trend analysis for coverage regression
- Hotspot identification for undertested code

### 10.2 Test Quality Review
- Monthly test code review sessions
- Performance optimization for slow tests
- Test maintainability improvements

### 10.3 Automation
- Automatic test generation for new domain classes
- Coverage-driven test suggestions
- Performance regression detection

This strategy ensures comprehensive domain testing with high coverage, maintainable test code, and reliable quality gates integrated into the CI/CD pipeline.
