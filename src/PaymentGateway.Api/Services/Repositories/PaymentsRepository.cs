using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services.Repositories;

public class PaymentsRepository
{
    public List<PaymentDto> Payments = new();
    
    public void Add(PaymentDto paymentDto)
    {
        Payments.Add(paymentDto);
    }

    public PaymentDto Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}