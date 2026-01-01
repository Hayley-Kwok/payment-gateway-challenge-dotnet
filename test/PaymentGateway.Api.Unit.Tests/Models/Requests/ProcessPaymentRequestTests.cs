using FluentAssertions;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Unit.Tests.Models.Requests;

public class ProcessPaymentRequestTests
{
    [Fact]
    public void ToAcquiringBankProcessPaymentRequest_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var request = new ProcessPaymentRequest
        {
            CardNumber = "4111111111111234",
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            Currency = "USD",
            Amount = 999,
            CVV = "123"
        };

        // Act
        var bankRequest = request.ToAcquiringBankProcessPaymentRequest();

        // Assert
        bankRequest.CardNumber.Should().Be(request.CardNumber);
        bankRequest.ExpiryDate.Should().Be("12/2030");
        bankRequest.Currency.Should().Be("USD");
        bankRequest.Amount.Should().Be(999);
        bankRequest.CVV.Should().Be("123");
    }

    [Fact]
    public void ToProcessPaymentResponse_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var request = new ProcessPaymentRequest
        {
            CardNumber = "4111111111111234",
            ExpiryMonth = 5,
            ExpiryYear = 2028,
            Currency = "EUR",
            Amount = 1500,
            CVV = "999"
        };
        var paymentId = Guid.NewGuid();
        var status = PaymentStatus.Authorized;

        // Act
        ProcessPaymentResponse response = request.ToProcessPaymentResponse(paymentId, status);

        // Assert
        response.Id.Should().Be(paymentId);
        response.Status.Should().Be(status);
        response.CardNumberLastFour.Should().Be(1234);
        response.ExpiryMonth.Should().Be(5);
        response.ExpiryYear.Should().Be(2028);
        response.Currency.Should().Be("EUR");
        response.Amount.Should().Be(1500);
    }

    [Theory]
    [InlineData("5555444433339876", 9876)] //valid card number
    [InlineData("aasssdss", 0)] //string card number -> to confirm code doesn't break
    public void ToPaymentEntity_ShouldMapAllFieldsCorrectly(string cardNumber, int lastFourDigit)
    {
        // Arrange
        var request = new ProcessPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 1,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 250,
            CVV = "321"
        };
        var paymentId = Guid.NewGuid();
        var status = PaymentStatus.Declined;
        var errorMessage = "Insufficient funds";

        // Act
        PaymentEntity entity = request.ToPaymentEntity(paymentId, status, errorMessage);

        // Assert
        entity.Id.Should().Be(paymentId);
        entity.Status.Should().Be(PaymentStatus.Declined);
        entity.FailReason.Should().Be(errorMessage);
        entity.CardNumberLastFour.Should().Be(lastFourDigit);
        entity.ExpiryMonth.Should().Be(1);
        entity.ExpiryYear.Should().Be(2026);
        entity.Currency.Should().Be("GBP");
        entity.Amount.Should().Be(250);
    }
}
