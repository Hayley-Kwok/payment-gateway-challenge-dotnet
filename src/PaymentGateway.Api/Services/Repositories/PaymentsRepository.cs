using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services.Repositories;

public class PaymentsRepository
{
    public List<PaymentEntity> Payments = new();
    
    public void Add(PaymentEntity paymentEntity)
    {
        Payments.Add(paymentEntity);
    }

    public PaymentEntity Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}