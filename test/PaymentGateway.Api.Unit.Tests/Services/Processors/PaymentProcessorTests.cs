using System.Net;

using FluentAssertions;
using Moq;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Processors;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Unit.Tests.Services.Processors;

public class PaymentProcessorTests
{
    private static ProcessPaymentRequest NewRequest() => new()
    {
        CardNumber = "4111111111111111",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "USD",
        Amount = 100,
        CVV = "123"
    };

    [Fact]
    public async Task ProcessPaymentAsync_WhenBankReturnsError_ShouldSaveDeclinedAndReturnDeclined()
    {
        // Arrange
        var bankMock = new Mock<IAcquiringBankClient>(MockBehavior.Strict);
        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        var processor = new PaymentProcessor(bankMock.Object, repoMock.Object);
        var request = NewRequest();

        var error = new AcquiringBankProcessPaymentErrorResponse(HttpStatusCode.BadRequest, "Invalid card number");

        bankMock
            .Setup(m => m.ProcessPaymentAsync(It.IsAny<AcquiringBankProcessPaymentRequest>()))
            .ReturnsAsync((null, error));

        repoMock
            .Setup(m => m.Add(It.IsAny<PaymentEntity>()))
            .Verifiable();

        // Act
        var response = await processor.ProcessPaymentAsync(request);

        // Assert
        response.Status.Should().Be(PaymentStatus.Declined);
        repoMock.Verify(m => m.Add(It.Is<PaymentEntity>(e =>
            e.Status == PaymentStatus.Declined &&
            e.FailReason == "Invalid card number" &&
            e.CardNumberLastFour == 1111 &&
            e.Currency == "USD" &&
            e.Amount == 100)), Times.Once);

        bankMock.VerifyAll();
        repoMock.VerifyAll();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenBankReturnsNullResponseAndNoError_ShouldThrow()
    {
        // Arrange
        var bankMock = new Mock<IAcquiringBankClient>(MockBehavior.Strict);
        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        var processor = new PaymentProcessor(bankMock.Object, repoMock.Object);
        var request = NewRequest();

        bankMock
            .Setup(m => m.ProcessPaymentAsync(It.IsAny<AcquiringBankProcessPaymentRequest>()))
            .ReturnsAsync((null, null));

        // Act
        var act = async () => await processor.ProcessPaymentAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        ex.Message.Should().Be("Both response and error from acquiring bank are null. Something went wrong.");

        bankMock.VerifyAll();
        repoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenBankReturnAuthorized_ShouldSaveAuthorizedAndReturnAuthorized()
    {
        // Arrange
        var bankMock = new Mock<IAcquiringBankClient>(MockBehavior.Strict);
        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        var processor = new PaymentProcessor(bankMock.Object, repoMock.Object);
        var request = NewRequest();

        var bankResponse = new AcquiringBankProcessPaymentResponse
        {
            Authorized = true,
            AuthorizationCode = "AUTH123"
        };

        bankMock
            .Setup(m => m.ProcessPaymentAsync(It.IsAny<AcquiringBankProcessPaymentRequest>()))
            .ReturnsAsync((bankResponse, null));

        repoMock
            .Setup(m => m.Add(It.IsAny<PaymentEntity>()))
            .Verifiable();

        // Act
        var response = await processor.ProcessPaymentAsync(request);

        // Assert
        response.Status.Should().Be(PaymentStatus.Authorized);
        repoMock.Verify(m => m.Add(It.Is<PaymentEntity>(e =>
            e.Status == PaymentStatus.Authorized &&
            e.FailReason == "")), Times.Once);

        bankMock.VerifyAll();
        repoMock.VerifyAll();
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenBankReturnRejected_ShouldSaveRejectedAndReturnRejected()
    {
        // Arrange
        var bankMock = new Mock<IAcquiringBankClient>(MockBehavior.Strict);
        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        var processor = new PaymentProcessor(bankMock.Object, repoMock.Object);
        var request = NewRequest();

        var bankResponse = new AcquiringBankProcessPaymentResponse
        {
            Authorized = false,
            AuthorizationCode = null
        };

        bankMock
            .Setup(m => m.ProcessPaymentAsync(It.IsAny<AcquiringBankProcessPaymentRequest>()))
            .ReturnsAsync((bankResponse, null));

        repoMock
            .Setup(m => m.Add(It.IsAny<PaymentEntity>()))
            .Verifiable();

        // Act
        var response = await processor.ProcessPaymentAsync(request);

        // Assert
        response.Status.Should().Be(PaymentStatus.Rejected);
        repoMock.Verify(m => m.Add(It.Is<PaymentEntity>(e =>
            e.Status == PaymentStatus.Rejected &&
            e.FailReason == "Payment was rejected by acquiring bank.")), Times.Once);

        bankMock.VerifyAll();
        repoMock.VerifyAll();
    }
}

