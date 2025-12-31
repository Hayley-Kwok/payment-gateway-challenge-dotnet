using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Retrievers;

public interface IPaymentRetriever
{
    PostPaymentResponse? GetPaymentRequest(Guid id);
}