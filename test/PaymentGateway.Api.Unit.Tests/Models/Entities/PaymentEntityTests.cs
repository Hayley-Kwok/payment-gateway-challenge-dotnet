using FluentAssertions;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Unit.Tests.Models.Entities;

public class PaymentEntityTests
{
    [Fact] public void ToPostPaymentResponse_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var entity = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            FailReason = "N/A",
            Currency = "USD",
            CardNumberLastFour = 1234,
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            Amount = 999
        };

        // Act
        PostPaymentResponse response = entity.ToPostPaymentResponse();

        // Assert
        response.Id.Should().Be(entity.Id);
        response.Status.Should().Be(entity.Status);
        response.CardNumberLastFour.Should().Be(entity.CardNumberLastFour);
        response.ExpiryMonth.Should().Be(entity.ExpiryMonth);
        response.ExpiryYear.Should().Be(entity.ExpiryYear);
        response.Currency.Should().Be(entity.Currency);
        response.Amount.Should().Be(entity.Amount);
    }
}
