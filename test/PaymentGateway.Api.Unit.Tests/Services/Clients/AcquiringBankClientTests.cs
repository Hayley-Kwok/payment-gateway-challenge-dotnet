using System.Net;
using System.Net.Http.Json;
using System.Text;

using FluentAssertions;
using Moq;
using Moq.Protected;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Clients;

namespace PaymentGateway.Api.Unit.Tests.Services.Clients;

public class AcquiringBankClientTests
{
    private static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_GivenValidResponseFromClient_ShouldReturnPaymentResponse()
    {
        // Arrange
        var expected = new AcquiringBankProcessPaymentResponse
        {
            Authorized = true,
            AuthorizationCode = "AUTH12345"
        };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expected)
        };
        var httpClient = CreateHttpClient(httpResponse);
        var client = new AcquiringBankClient("http://example/payments", httpClient);
        var request = new AcquiringBankProcessPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 100,
            CVV = "123"
        };

        // Act
        var (data, error) = await client.ProcessPaymentAsync(request);

        // Assert
        error.Should().BeNull();
        data.Should().NotBeNull();
        data.Authorized.Should().BeTrue();
        data.AuthorizationCode.Should().Be("AUTH12345");
    }

    [Fact]
    public async Task ProcessPaymentAsync_GivenInvalidJsonFromClient_ShouldReturnError()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("")
        };
        var httpClient = CreateHttpClient(httpResponse);
        var client = new AcquiringBankClient("http://example/payments", httpClient);
        var request = new AcquiringBankProcessPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 100,
            CVV = "123"
        };

        // Act
        var (data, error) = await client.ProcessPaymentAsync(request);

        // Assert
        data.Should().BeNull();
        error.Should().NotBeNull();
        error.StatusCode.Should().Be(HttpStatusCode.OK);
        error.ErrorMessage.Should().Be("Failed to parse acquiring bank response: The input does not contain any JSON tokens. Expected the input to start with a valid JSON token, when isFinalBlock is true. Path: $ | LineNumber: 0 | BytePositionInLine: 0.");
    }

    [Fact]
    public async Task ProcessPaymentAsync_GivenNoContentFromClient_ShouldReturnError()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };
        var httpClient = CreateHttpClient(httpResponse);
        var client = new AcquiringBankClient("http://example/payments", httpClient);
        var request = new AcquiringBankProcessPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryDate = "12/2030",
            Currency = "USD",
            Amount = 100,
            CVV = "123"
        };

        // Act
        var (data, error) = await client.ProcessPaymentAsync(request);

        // Assert
        data.Should().BeNull();
        error.Should().NotBeNull();
        error.StatusCode.Should().Be(HttpStatusCode.OK);
        error.ErrorMessage.Should().Be("Acquiring bank returned an empty response.");
    }
    
    [Fact]
    public async Task ProcessPaymentAsync_GivenFailedResponseFromClient_ShouldReturnError()
    {
        // Arrange
        var errorBody = "Invalid card number";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody)
        };
        var httpClient = CreateHttpClient(httpResponse);
        var client = new AcquiringBankClient("http://example/payments", httpClient);
        var request = new AcquiringBankProcessPaymentRequest
        {
            CardNumber = "0000",
            ExpiryDate = "01/2020",
            Currency = "USD",
            Amount = 10,
            CVV = "000"
        };

        // Act
        var (data, error) = await client.ProcessPaymentAsync(request);

        // Assert
        data.Should().BeNull();
        error.Should().NotBeNull();
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.ErrorMessage.Should().Be(errorBody);
    }
}
