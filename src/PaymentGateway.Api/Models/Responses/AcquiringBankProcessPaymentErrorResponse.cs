using System.Net;

namespace PaymentGateway.Api.Models.Responses;

public record AcquiringBankProcessPaymentErrorResponse(HttpStatusCode StatusCode, string ErrorMessage);