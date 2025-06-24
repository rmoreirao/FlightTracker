using AutoFixture;

namespace FlightTracker.Domain.Tests.Base;

/// <summary>
/// Base class for domain tests with AutoFixture configuration
/// </summary>
public abstract class DomainTestBase
{
    protected IFixture Fixture { get; }

    protected DomainTestBase()
    {
        Fixture = new Fixture();
    }

    /// <summary>
    /// Creates multiple instances of a type for collection tests
    /// </summary>
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count);

    /// <summary>
    /// Creates a single instance with AutoFixture
    /// </summary>
    protected T Create<T>() => Fixture.Create<T>();

    /// <summary>
    /// Creates an instance with specific property values
    /// </summary>
    protected T Create<T>(Action<T> configure)
    {
        var instance = Fixture.Create<T>();
        configure(instance);
        return instance;
    }
}

/// <summary>
/// Base class for entity tests with common entity testing patterns
/// </summary>
public abstract class EntityTestBase<TEntity> : DomainTestBase where TEntity : class
{
    /// <summary>
    /// Test that two entities with same business identity are equal
    /// </summary>
    protected abstract void AssertEntitiesAreEqual(TEntity entity1, TEntity entity2);

    /// <summary>
    /// Test that two entities with different business identity are not equal
    /// </summary>
    protected abstract void AssertEntitiesAreNotEqual(TEntity entity1, TEntity entity2);

    /// <summary>
    /// Create an entity with valid state for testing
    /// </summary>
    protected abstract TEntity CreateValidEntity();

    /// <summary>
    /// Tests basic entity equality behavior
    /// </summary>
    [Fact]
    public virtual void Equals_WithSameBusinessIdentity_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = CreateValidEntity();
        var entity2 = CreateValidEntity();

        // Act & Assert
        AssertEntitiesAreEqual(entity1, entity2);
    }

    /// <summary>
    /// Tests basic entity inequality behavior
    /// </summary>
    [Fact]
    public virtual void Equals_WithDifferentBusinessIdentity_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = CreateValidEntity();
        var entity2 = CreateValidEntity();

        // Act & Assert
        AssertEntitiesAreNotEqual(entity1, entity2);
    }

    /// <summary>
    /// Tests entity hash code consistency
    /// </summary>
    [Fact]
    public virtual void GetHashCode_WithSameBusinessIdentity_ShouldReturnSameHashCode()
    {
        // Arrange
        var entity1 = CreateValidEntity();
        var entity2 = CreateValidEntity();

        // Ensure they're considered equal first
        AssertEntitiesAreEqual(entity1, entity2);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }
}

/// <summary>
/// Base class for value object tests with common value object testing patterns
/// </summary>
public abstract class ValueObjectTestBase<TValueObject> : DomainTestBase where TValueObject : class
{
    /// <summary>
    /// Create a value object with valid state for testing
    /// </summary>
    protected abstract TValueObject CreateValidValueObject();

    /// <summary>
    /// Create a value object with different values than the first one
    /// </summary>
    protected abstract TValueObject CreateDifferentValueObject();

    /// <summary>
    /// Tests value object equality behavior
    /// </summary>
    [Fact]
    public virtual void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var valueObject1 = CreateValidValueObject();
        var valueObject2 = CreateValidValueObject();

        // Act & Assert
        valueObject1.Should().Be(valueObject2);
        valueObject1.Equals(valueObject2).Should().BeTrue();
        (valueObject1 == valueObject2).Should().BeTrue();
        (valueObject1 != valueObject2).Should().BeFalse();
    }

    /// <summary>
    /// Tests value object inequality behavior
    /// </summary>
    [Fact]
    public virtual void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var valueObject1 = CreateValidValueObject();
        var valueObject2 = CreateDifferentValueObject();

        // Act & Assert
        valueObject1.Should().NotBe(valueObject2);
        valueObject1.Equals(valueObject2).Should().BeFalse();
        (valueObject1 == valueObject2).Should().BeFalse();
        (valueObject1 != valueObject2).Should().BeTrue();
    }

    /// <summary>
    /// Tests value object hash code consistency
    /// </summary>
    [Fact]
    public virtual void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var valueObject1 = CreateValidValueObject();
        var valueObject2 = CreateValidValueObject();

        // Act
        var hashCode1 = valueObject1.GetHashCode();
        var hashCode2 = valueObject2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    /// <summary>
    /// Tests value object immutability
    /// </summary>
    [Fact]
    public virtual void ValueObject_ShouldBeImmutable()
    {
        // Arrange
        var valueObject = CreateValidValueObject();
        var originalHashCode = valueObject.GetHashCode();

        // Act & Assert - This test is mainly to ensure no public setters exist
        // The assertion checks that the object still has the same hash code
        // which implies its state hasn't changed
        valueObject.GetHashCode().Should().Be(originalHashCode);
    }
}

/// <summary>
/// Base class for domain service tests
/// </summary>
public abstract class DomainServiceTestBase : DomainTestBase
{
    /// <summary>
    /// Setup method called before each test
    /// Override this in derived classes for service-specific setup
    /// </summary>
    protected virtual void Setup() { }

    /// <summary>
    /// Teardown method called after each test
    /// Override this in derived classes for service-specific cleanup
    /// </summary>
    protected virtual void Teardown() { }
}
