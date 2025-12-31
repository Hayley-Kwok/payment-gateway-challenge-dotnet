using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Processors;
using PaymentGateway.Api.Services.Retrievers;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentRetriever paymentRetriever, IPaymentProcessor paymentProcessor) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentRetriever.GetPaymentRequest(id);
        if (payment is null)
        {
            return new NotFoundResult();
        }
        return Ok(payment);
    }
    
    [HttpPost("")]
    public async Task<ActionResult<ProcessPaymentResponse>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var response = await paymentProcessor.ProcessPaymentAsync(request);
        return Ok(response);
    }
}