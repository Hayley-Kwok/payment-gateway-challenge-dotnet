using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Clients;


public interface IAcquiringBankClient
{
    Task<(AcquiringBankProcessPaymentResponse?, AcquiringBankProcessPaymentErrorResponse?)> ProcessPaymentAsync(AcquiringBankProcessPaymentRequest request);
}