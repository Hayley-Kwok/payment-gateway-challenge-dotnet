using System;
using FluentAssertions;
using Moq;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Services.Retrievers;

namespace PaymentGateway.Api.Unit.Tests.Services.Retrievers;

public class PaymentRetrieverTests
{
    [Fact]
    public void GetPaymentRequest_WhenEntityExists_ShouldReturnMappedResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new PaymentEntity
        {
            Id = id,
            Status = PaymentStatus.Authorized,
            FailReason = string.Empty,
            Currency = "USD",
            CardNumberLastFour = 5678,
            ExpiryMonth = 11,
            ExpiryYear = 2031,
            Amount = 150
        };

        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.Get(id)).Returns(entity);

        var retriever = new PaymentRetriever(repoMock.Object);

        // Act
        PostPaymentResponse? result = retriever.GetPaymentRequest(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Status.Should().Be(PaymentStatus.Authorized);
        result.CardNumberLastFour.Should().Be(5678);
        result.ExpiryMonth.Should().Be(11);
        result.ExpiryYear.Should().Be(2031);
        result.Currency.Should().Be("USD");
        result.Amount.Should().Be(150);
        
        repoMock.Verify(r => r.Get(id), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetPaymentRequest_WhenEntityMissing_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repoMock = new Mock<IPaymentsRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.Get(id)).Returns((PaymentEntity?)null);
        var retriever = new PaymentRetriever(repoMock.Object);

        // Act
        var result = retriever.GetPaymentRequest(id);

        // Assert
        result.Should().BeNull();
        repoMock.Verify(r => r.Get(id), Times.Once);
        repoMock.VerifyNoOtherCalls();
    }
}

