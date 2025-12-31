using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public class AcquiringBankProcessPaymentRequest
{
    [JsonPropertyName("card_number")]
    public required string CardNumber { get; set; }
    
    [JsonPropertyName("expiry_date")]
    public required string ExpiryDate { get; set; }
    
    public required string Currency { get; set; }
    
    public required int Amount { get; set; }
    
    public required string CVV { get; set; }
}