using FluentAssertions;
using FluentValidation;
using Gateway.Application.DTOs;
using Gateway.Application.Validators;

namespace Gateway.Api.Tests.Validators;

/// <summary>
/// Unit tests for EventRequestValidator.
/// Tests validation rules for all event request fields.
/// </summary>
public class EventRequestValidatorTests
{
    private readonly EventRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com",
            Timestamp: DateTime.UtcNow);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyEventType_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "",
            UserId: "123",
            Email: "user@email.com");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EventType");
    }

    [Fact]
    public async Task Validate_UnsupportedEventType_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "INVALID_TYPE",
            UserId: "123",
            Email: "user@email.com");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "EventType" &&
            e.ErrorMessage.Contains("must be one of"));
    }

    [Theory]
    [InlineData("USER_REGISTERED")]
    [InlineData("PAYMENT_FAILED")]
    [InlineData("ORDER_SHIPPED")]
    [InlineData("SECURITY_ALERT")]
    public async Task Validate_SupportedEventTypes_ShouldPass(string eventType)
    {
        // Arrange
        var request = new EventRequest(
            EventType: eventType,
            UserId: "123",
            Email: "user@email.com");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyUserId_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "",
            Email: "user@email.com");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task Validate_InvalidEmail_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "not-an-email");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmptyEmail_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_NullTimestamp_ShouldPass()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com",
            Timestamp: null);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_FutureTimestamp_ShouldFail()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com",
            Timestamp: DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Timestamp");
    }

    [Fact]
    public async Task Validate_MultipleErrors_ShouldReturnAll()
    {
        // Arrange
        var request = new EventRequest(
            EventType: "",
            UserId: "",
            Email: "");

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(3);
    }
}
