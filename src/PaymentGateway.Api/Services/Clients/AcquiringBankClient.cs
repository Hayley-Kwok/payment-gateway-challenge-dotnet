using System.Net;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Clients;

public class AcquiringBankClient(string url, HttpClient httpClient, ILogger<AcquiringBankClient> logger) : IAcquiringBankClient
{
    public const string LocalTestUrl = "http://localhost:8080/payments";

    public async Task<(AcquiringBankProcessPaymentResponse?, AcquiringBankProcessPaymentErrorResponse?)> ProcessPaymentAsync(AcquiringBankProcessPaymentRequest request)
    {
        var requestJson = JsonContent.Create(request);
        
        logger.LogInformation("Sending payment request to {Url}", url);
        using var response = await httpClient.PostAsync(url, requestJson);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var responseData = await response.Content.ReadFromJsonAsync<AcquiringBankProcessPaymentResponse>();
                if (responseData is null)
                {
                    logger.LogWarning("[BankClient] Empty success body from acquiring bank. Status={StatusCode}", HttpStatusCode.OK);
                    return (null, new AcquiringBankProcessPaymentErrorResponse(HttpStatusCode.OK, "Acquiring bank returned an empty response."));
                }
                
                logger.LogInformation("[BankClient] Parsed success body. Authorized={Authorized}", responseData.Authorized);
                return (responseData, null);
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError("[BankClient] Failed to parse success body from acquiring bank");
                return (null, new AcquiringBankProcessPaymentErrorResponse(HttpStatusCode.OK, $"Failed to parse acquiring bank response: {ex.Message}"));
            }
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        logger.LogWarning("[BankClient] Non-success from acquiring bank. Status={StatusCode}", response.StatusCode);
        return (null, new AcquiringBankProcessPaymentErrorResponse(response.StatusCode, errorBody));
    }
}