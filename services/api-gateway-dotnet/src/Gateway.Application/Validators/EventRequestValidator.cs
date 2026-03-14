using FluentValidation;
using Gateway.Application.DTOs;

namespace Gateway.Application.Validators;

/// <summary>
/// Validates incoming event requests.
/// Ensures all required fields are present and well-formed.
/// </summary>
public sealed class EventRequestValidator : AbstractValidator<EventRequest>
{
    /// <summary>
    /// Supported event types for validation.
    /// </summary>
    private static readonly string[] SupportedEventTypes =
    [
        "USER_REGISTERED",
        "PAYMENT_FAILED",
        "ORDER_SHIPPED",
        "SECURITY_ALERT"
    ];

    public EventRequestValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("EventType is required.")
            .Must(type => SupportedEventTypes.Contains(type))
            .WithMessage($"EventType must be one of: {string.Join(", ", SupportedEventTypes)}.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Timestamp)
            .Must(ts => ts == null || ts <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp cannot be in the future.");
    }
}
