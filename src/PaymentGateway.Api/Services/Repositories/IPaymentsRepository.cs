using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services.Repositories;

public interface IPaymentsRepository
{
    void Add(PaymentEntity paymentEntity);
    PaymentEntity Get(Guid id);
}