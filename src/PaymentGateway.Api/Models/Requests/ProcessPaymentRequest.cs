using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models.Requests;

public class ProcessPaymentRequest
{
    public required string CardNumber { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public required int Amount { get; set; }
    public required string CVV { get; set; }

    public AcquiringBankProcessPaymentRequest ToAcquiringBankProcessPaymentRequest() =>
        new()
        {
            CardNumber = CardNumber,
            ExpiryDate = $"{ExpiryMonth}/{ExpiryYear}",
            Currency = Currency,
            Amount = Amount,
            CVV = CVV
        };
    
    public ProcessPaymentResponse ToProcessPaymentResponse(Guid paymentId, PaymentStatus paymentStatus) =>
        new()
        {
            Id = paymentId,
            Status = paymentStatus,
            CardNumberLastFour = int.Parse(CardNumber[^4..]),
            ExpiryMonth = ExpiryMonth,
            ExpiryYear = ExpiryYear,
            Currency = Currency,
            Amount = Amount
        };
    
    public PostPaymentResponse ToPostPaymentResponse(Guid paymentId, PaymentStatus paymentStatus) =>
        new()
        {
            Id = paymentId,
            Status =  paymentStatus,
            CardNumberLastFour = int.Parse(CardNumber[^4..]),
            ExpiryMonth = ExpiryMonth,
            ExpiryYear = ExpiryYear,
            Currency = Currency,
            Amount = Amount,
        };
    
    public PaymentDto ToPaymentDto(Guid paymentId, PaymentStatus paymentStatus, string errorMessage = "") =>
        new()
        {
            Id = paymentId,
            Status =  paymentStatus,
            FailReason = errorMessage,
            CardNumberLastFour = int.Parse(CardNumber[^4..]),
            ExpiryMonth = ExpiryMonth,
            ExpiryYear = ExpiryYear,
            Currency = Currency,
            Amount = Amount,
        };
}