# FlightTracker.Domain.Tests

This project contains comprehensive unit tests for the FlightTracker domain layer, implementing a robust testing strategy with xUnit, AutoFixture, and enforcing an 80% code coverage gate.

## 🎯 Testing Strategy

### Test Coverage Requirements
- **Line Coverage**: 80% minimum (enforced)
- **Branch Coverage**: 70% target (recommended)
- **Mutation Testing**: Optional for critical paths

### Testing Frameworks & Tools
- **xUnit**: Primary testing framework
- **AutoFixture**: Test data generation and customization
- **FluentAssertions**: Expressive assertions
- **Moq**: Mocking framework for dependencies
- **Coverlet**: Code coverage collection
- **ReportGenerator**: HTML coverage reports

## 🏗️ Project Structure

```
FlightTracker.Domain.Tests/
├── Base/
│   └── TestBase.cs                    # Base classes for different test types
├── Builders/
│   └── TestDataBuilders.cs           # Fluent builders for complex test data
├── Fixtures/
│   └── AutoFixtureCustomizations.cs  # AutoFixture customizations for domain objects
├── Entities/
│   ├── FlightTests.cs                # Tests for Flight entity
│   ├── FlightSegmentTests.cs         # Tests for FlightSegment entity
│   ├── AirportTests.cs               # Tests for Airport entity
│   ├── AirlineTests.cs               # Tests for Airline entity
│   ├── FlightQueryTests.cs           # Tests for FlightQuery entity
│   └── PriceSnapshotTests.cs         # Tests for PriceSnapshot entity
├── ValueObjects/
│   ├── MoneyTests.cs                 # Tests for Money value object
│   ├── DateRangeTests.cs             # Tests for DateRange value object
│   └── RouteKeyTests.cs              # Tests for RouteKey value object
├── Services/
│   ├── FlightServiceTests.cs         # Tests for domain services
│   └── PriceAnalysisServiceTests.cs  # Tests for price analysis
├── Events/
│   └── DomainEventTests.cs           # Tests for domain events
└── TestResults/                      # Generated test results and coverage reports
```

## 🚀 Running Tests

### Local Development

#### Run All Tests
```powershell
# Simple test run
dotnet test

# With coverage
.\scripts\Run-DomainTests.ps1

# With HTML report generation
.\scripts\Run-DomainTests.ps1 -GenerateReport -OpenReport

# Filter specific tests
.\scripts\Run-DomainTests.ps1 -Filter "Money*" -Verbose
```

#### Quick Commands
```bash
# Build and test
dotnet build && dotnet test

# Test with coverage (minimal)
dotnet test --collect:"XPlat Code Coverage"

# Test with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### CI/CD Pipeline

The GitHub Actions workflow automatically:
1. Runs all domain tests
2. Collects code coverage
3. Enforces 80% coverage gate
4. Generates HTML coverage reports
5. Comments coverage results on PRs
6. Fails builds if coverage is insufficient

## 📊 Coverage Reports

### Local Reports
- Run `.\scripts\Run-DomainTests.ps1 -GenerateReport` to generate HTML reports
- Reports are saved to `TestResults/Reports/index.html`
- JSON summary available for CI integration

### CI Reports
- Coverage badges automatically generated
- Artifacts uploaded for each build
- PR comments with coverage status
- Downloadable HTML reports

## 🧪 Test Patterns & Best Practices

### Base Test Classes

#### `DomainTestBase`
Base class providing AutoFixture configuration and common test utilities:
```csharp
public class MyDomainTests : DomainTestBase
{
    [Fact]
    public void Test_WithAutoFixture()
    {
        // Arrange
        var entity = Create<MyEntity>();
        
        // Act & Assert
        entity.Should().NotBeNull();
    }
}
```

#### `EntityTestBase<T>`
Base class for entity tests with common entity behavior tests:
```csharp
public class FlightTests : EntityTestBase<Flight>
{
    protected override Flight CreateValidEntity() => 
        FlightBuilder.Create().AsOneWay("JFK", "LAX", DateTime.Today.AddDays(1)).Build();

    // Inherits standard entity tests: equality, hash codes, etc.
}
```

#### `ValueObjectTestBase<T>`
Base class for value object tests with immutability and equality tests:
```csharp
public class MoneyTests : ValueObjectTestBase<Money>
{
    protected override Money CreateValidValueObject() => new(100m, "USD");
    protected override Money CreateDifferentValueObject() => new(200m, "EUR");

    // Inherits standard value object tests: equality, immutability, etc.
}
```

### Test Data Builders

Fluent builders for creating complex test data:
```csharp
var flight = FlightBuilder.Create()
    .AsRoundTrip("JFK", "LAX", DateTime.Today.AddDays(1), DateTime.Today.AddDays(8))
    .WithStatus(FlightStatus.Confirmed)
    .WithConfirmationCode("ABC123")
    .Build();

var segment = FlightSegmentBuilder.Create()
    .WithRoute("JFK", "LAX")
    .WithDepartureTime(DateTime.Today.AddHours(10))
    .WithDuration(TimeSpan.FromHours(6))
    .Build();
```

### AutoFixture Customizations

Domain-specific AutoFixture customizations ensure valid test data:
```csharp
// Money with valid currency codes
var money = fixture.Create<Money>(); // Always generates valid currency

// RouteKey with different airports
var route = fixture.Create<RouteKey>(); // Never same origin/destination

// DateRange with valid date ordering
var dateRange = fixture.Create<DateRange>(); // End date always >= start date
```

### Test Organization

#### Test Method Naming
Follow the pattern: `MethodName_Scenario_ExpectedBehavior`
```csharp
[Fact]
public void Constructor_WithNegativeAmount_ShouldThrowArgumentException()

[Fact]
public void Add_WithSameCurrency_ShouldReturnSum()

[Theory]
[InlineData(null)]
[InlineData("")]
public void Constructor_WithInvalidCurrency_ShouldThrowArgumentException(string currency)
```

#### Test Categories
Use attributes to categorize tests:
```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Domain", "ValueObject")]
public void Money_ShouldBeImmutable()
```

## 🔍 Coverage Analysis

### Coverage Targets by Component

| Component | Target | Current | Status |
|-----------|--------|---------|--------|
| Entities | 85% | TBD | 🎯 |
| Value Objects | 90% | TBD | 🎯 |
| Domain Services | 80% | TBD | 🎯 |
| Domain Events | 75% | TBD | 🎯 |
| **Overall** | **80%** | **TBD** | **🎯** |

### Exclusions from Coverage
- Infrastructure concerns (Program.cs, Startup.cs)
- Auto-generated code
- Simple property getters/setters
- Exception message strings

## 🧬 Mutation Testing (Optional)

For critical business logic, mutation testing validates test quality:
```bash
dotnet tool install -g dotnet-stryker
dotnet stryker --project "../../src/FlightTracker.Domain/FlightTracker.Domain.csproj"
```

Mutation testing thresholds:
- **High**: 85% (excellent)
- **Low**: 70% (acceptable)  
- **Break**: 60% (fails build)

## 📋 Test Checklist

For each new domain class, ensure:

- [ ] Unit tests for all public methods
- [ ] Constructor validation tests
- [ ] Business rule validation tests
- [ ] Edge case handling
- [ ] Error condition tests
- [ ] Equality and hash code tests (entities/value objects)
- [ ] Immutability tests (value objects)
- [ ] Domain event tests (if applicable)
- [ ] Performance tests (if critical)

## 🐛 Debugging Tests

### Common Issues
1. **AutoFixture Circular Dependencies**: Use `OmitAutoProperties()` or custom creators
2. **DateTime Precision**: Use `BeCloseTo()` for time comparisons
3. **Floating Point Precision**: Use appropriate precision for decimal comparisons
4. **Async Tests**: Use `async Task` and proper async assertion patterns

### Test Debugging Tips
```csharp
// Use output helper for debugging
private readonly ITestOutputHelper _output;

[Fact]
public void Debug_Test()
{
    var entity = Create<Entity>();
    _output.WriteLine($"Entity: {JsonSerializer.Serialize(entity)}");
    // ... test logic
}
```

## 🔄 Continuous Improvement

### Monitoring Test Quality
- Review coverage reports regularly
- Identify untested edge cases
- Refactor complex tests for maintainability
- Update test data builders as domain evolves

### Performance Monitoring
- Monitor test execution time
- Optimize slow-running tests
- Use `[Fact(Skip = "Performance")]` for known slow tests during development

---

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [AutoFixture Documentation](https://github.com/AutoFixture/AutoFixture)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Domain-Driven Design Testing Patterns](https://martinfowler.com/articles/practical-test-pyramid.html)
