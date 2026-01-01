using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Tests.Helpers;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    #region Get
    private readonly Random _random = new();
    
    [Fact]
    public async Task Get_CanRetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            FailReason = "",
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton<IPaymentsRepository>(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
        paymentResponse!.Status.Should().Be(PaymentStatus.Authorized);
        paymentResponse.Currency.Should().Be(payment.Currency);
        paymentResponse.Amount.Should().Be(payment.Amount);
        paymentResponse.ExpiryMonth.Should().Be(payment.ExpiryMonth);
        paymentResponse.ExpiryYear.Should().Be(payment.ExpiryYear);
        paymentResponse.CardNumberLastFour.Should().Be(payment.CardNumberLastFour);
    }

    [Fact]
    public async Task Get_Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion
    
    #region Post
    private static ProcessPaymentRequest ValidRequest() => new()
    {
        CardNumber = "4111111111111111",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "USD",
        Amount = 100,
        CVV = "123"
    };

    [Fact]
    public async Task Post_WhenBankReturnAuthorized_Returns200WithAuthorized()
    {
        // Arrange: stub acquiring bank HTTP to return authorized JSON
        var bankResponseBody = new AcquiringBankProcessPaymentResponse { Authorized = true, AuthorizationCode = "AUTH-XYZ" };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(bankResponseBody)
        };

        var factory = new WebApplicationFactory<PaymentsController>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace HttpClient used by AcquiringBankClient with a stubbed instance
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient));
                    if (descriptor is not null) services.Remove(descriptor);
                    services.AddSingleton(StubAcquiringBankHttpHandler.CreateStubHttpClient(httpResponse));
                });
            });

        var client = factory.CreateClient();
        var request = ValidRequest();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var payload = await response.Content.ReadFromJsonAsync<ProcessPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be(PaymentStatus.Authorized);
        payload.Currency.Should().Be(request.Currency);
        payload.Amount.Should().Be(request.Amount);
        payload.ExpiryMonth.Should().Be(request.ExpiryMonth);
        payload.ExpiryYear.Should().Be(request.ExpiryYear);
        payload.CardNumberLastFour.Should().Be(1111);
    }

    [Fact]
    public async Task Post_WhenBankReturnUnauthorized_Returns200WithDeclined()
    {
        // Arrange: stub acquiring bank HTTP to return Declined JSON
        var bankResponseBody = new AcquiringBankProcessPaymentResponse { Authorized = false };
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(bankResponseBody)
        };

        var factory = new WebApplicationFactory<PaymentsController>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient));
                    if (descriptor is not null) services.Remove(descriptor);
                    services.AddSingleton(StubAcquiringBankHttpHandler.CreateStubHttpClient(httpResponse));
                });
            });

        var client = factory.CreateClient();
        var request = ValidRequest();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var payload = await response.Content.ReadFromJsonAsync<ProcessPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public async Task Post_WhenBankReturnError_Returns200WithDeclined()
    {
        // Arrange: stub acquiring bank HTTP to return 400 with error body
        var errorBody = "Invalid card number";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody)
        };

        var factory = new WebApplicationFactory<PaymentsController>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient));
                    if (descriptor is not null) services.Remove(descriptor);
                    services.AddSingleton(StubAcquiringBankHttpHandler.CreateStubHttpClient(httpResponse));
                });
            });

        var client = factory.CreateClient();
        var request = ValidRequest();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var payload = await response.Content.ReadFromJsonAsync<ProcessPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public async Task Post_GivenInvalidRequest_Returns400WithRejected()
    {
        // Arrange: invalid request
        var factory = new WebApplicationFactory<PaymentsController>();
        var client = factory.CreateClient();

        var invalid = new ProcessPaymentRequest
        {
            CardNumber = "abcd", // non-numeric
            ExpiryMonth = 1,
            ExpiryYear = 2000, // past date
            Currency = "US", // not 3 letters
            Amount = 100,
            CVV = "xx" // non-numeric
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", invalid);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    #endregion
}