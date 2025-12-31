using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models.Entities;

public class PaymentEntity
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public required string FailReason { get; set; } // for internal uses/record keeping. Useful for handling support requests/investigations
    public required string Currency { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public int Amount { get; set; }
    
    public PostPaymentResponse ToPostPaymentResponse()
    {
        return new PostPaymentResponse
        {
            Id = Id,
            Status = Status,
            CardNumberLastFour = CardNumberLastFour,
            ExpiryMonth = ExpiryMonth,
            ExpiryYear = ExpiryYear,
            Currency = Currency,
            Amount = Amount
        };
    }
}