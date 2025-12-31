using FluentAssertions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Unit.Tests.Services.Repositories;

public class PaymentsRepositoryTests
{
    private static PaymentEntity NewEntity(Guid? id = null, PaymentStatus status = PaymentStatus.Authorized, string currency = "USD", int last4 = 1234, int month = 12, int year = 2030, int amount = 100, string? failReason = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Status = status,
            Currency = currency,
            CardNumberLastFour = last4,
            ExpiryMonth = month,
            ExpiryYear = year,
            Amount = amount,
            FailReason = failReason ?? string.Empty
        };

    [Fact]
    public void Add_ThenGet_ById_ShouldReturnSameEntity()
    {
        // Arrange
        var repo = new PaymentsRepository();
        var entity = NewEntity();

        // Act
        repo.Add(entity);
        var fetched = repo.Get(entity.Id);

        // Assert
        fetched.Should().NotBeNull();
        fetched.Should().BeSameAs(entity); // repository stores the same instance
        fetched.Id.Should().Be(entity.Id);
        fetched.Status.Should().Be(entity.Status);
        fetched.Currency.Should().Be(entity.Currency);
        fetched.CardNumberLastFour.Should().Be(entity.CardNumberLastFour);
        fetched.ExpiryMonth.Should().Be(entity.ExpiryMonth);
        fetched.ExpiryYear.Should().Be(entity.ExpiryYear);
        fetched.Amount.Should().Be(entity.Amount);
        fetched.FailReason.Should().Be(entity.FailReason);
    }

    [Fact]
    public void Get_UnknownId_ShouldReturnNull()
    {
        // Arrange
        var repo = new PaymentsRepository();

        // Act
        var fetched = repo.Get(Guid.NewGuid());

        // Assert
        fetched.Should().BeNull();
    }

    [Fact]
    public void Add_Multiple_ThenGet_ShouldReturnEachCorrectly()
    {
        // Arrange
        var repo = new PaymentsRepository();
        var first = NewEntity(id: Guid.NewGuid(), status: PaymentStatus.Authorized, currency: "USD", last4: 1111);
        var second = NewEntity(id: Guid.NewGuid(), status: PaymentStatus.Declined, currency: "EUR", last4: 2222, failReason: "Declined by bank");
        var third = NewEntity(id: Guid.NewGuid(), status: PaymentStatus.Rejected, currency: "GBP", last4: 3333, failReason: "Rejected by bank");

        // Act
        repo.Add(first);
        repo.Add(second);
        repo.Add(third);

        // Assert
        repo.Get(first.Id).Should().BeSameAs(first);
        repo.Get(second.Id).Should().BeSameAs(second);
        repo.Get(third.Id).Should().BeSameAs(third);
    }
}

