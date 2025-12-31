using PaymentGateway.Api.Models.Entities;

namespace PaymentGateway.Api.Services.Repositories;

public class PaymentsRepository : IPaymentsRepository
{
    private readonly List<PaymentEntity> _payments = new();
    
    public void Add(PaymentEntity paymentEntity)
    {
        _payments.Add(paymentEntity);
    }

    public PaymentEntity? Get(Guid id)
    {
        return _payments.FirstOrDefault(p => p.Id == id);
    }
}