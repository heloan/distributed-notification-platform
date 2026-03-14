using FluentAssertions;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.RuleEngine;

namespace NotificationService.Tests.Infrastructure.RuleEngine;

/// <summary>
/// Unit tests for <see cref="InMemoryRuleEngine"/>.
/// </summary>
public sealed class InMemoryRuleEngineTest
{
    private readonly InMemoryRuleEngine _sut = new();

    [Theory]
    [InlineData(EventType.UserRegistered, new[] { ChannelType.Email })]
    [InlineData(EventType.PaymentFailed, new[] { ChannelType.Slack })]
    [InlineData(EventType.OrderShipped, new[] { ChannelType.Sms })]
    public void Evaluate_ShouldReturnCorrectRule_ForKnownEventType(
        EventType eventType, ChannelType[] expectedChannels)
    {
        // Act
        var rule = _sut.Evaluate(eventType);

        // Assert
        rule.Should().NotBeNull();
        rule!.EventType.Should().Be(eventType);
        rule.Channels.Should().BeEquivalentTo(expectedChannels);
        rule.MessageTemplate.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Evaluate_ShouldReturnMultipleChannels_ForSecurityAlert()
    {
        // Act
        var rule = _sut.Evaluate(EventType.SecurityAlert);

        // Assert
        rule.Should().NotBeNull();
        rule!.Channels.Should().HaveCount(2);
        rule.Channels.Should().Contain(ChannelType.Email);
        rule.Channels.Should().Contain(ChannelType.Slack);
    }
}
