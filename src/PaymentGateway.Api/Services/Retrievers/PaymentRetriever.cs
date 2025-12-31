using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Services.Retrievers;

public class PaymentRetriever(IPaymentsRepository paymentsRepository)
{
    public PostPaymentResponse? GetPaymentRequest(Guid id)
    {
        var paymentEntity = paymentsRepository.Get(id);

        return paymentEntity?.ToPostPaymentResponse();
    }
}