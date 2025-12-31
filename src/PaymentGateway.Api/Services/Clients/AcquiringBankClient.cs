using System.Net;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Clients;

public class AcquiringBankClient(string url, HttpClient httpClient) : IAcquiringBankClient
{
    public const string LocalTestUrl = "http://localhost:8080/payments";

    public async Task<(AcquiringBankProcessPaymentResponse?, AcquiringBankProcessPaymentErrorResponse?)> ProcessPaymentAsync(AcquiringBankProcessPaymentRequest request)
    {
        var requestJson = JsonContent.Create(request);
        
        using var response = await httpClient.PostAsync(url, requestJson);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromJsonAsync<AcquiringBankProcessPaymentResponse>();
            if (responseData is null)
                return (null, new AcquiringBankProcessPaymentErrorResponse(HttpStatusCode.OK, "Acquiring bank returned an empty response."));

            return (responseData, null);
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        return (null, new AcquiringBankProcessPaymentErrorResponse(response.StatusCode, errorBody));
    }
}