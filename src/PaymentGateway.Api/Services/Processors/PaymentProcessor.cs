using FluentValidation;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Entities;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Services.Processors;

public class PaymentProcessor(IAcquiringBankClient acquiringBankClient, IPaymentsRepository paymentsRepository, IValidator<ProcessPaymentRequest> requestValidator) : IPaymentProcessor
{
    public async Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var paymentId = Guid.NewGuid();
        
        var validationResult = await requestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var paymentEntity = new PaymentEntity
            {
                Id = paymentId,
                FailReason = string.Join("; ", errorMessages),
                Status = PaymentStatus.Rejected,
                Currency = ""
            };
            paymentsRepository.Add(paymentEntity);
            
            return new ProcessPaymentResponse {Id = paymentId, Status = PaymentStatus.Rejected};
        }
        
        var (response, error) = await acquiringBankClient.ProcessPaymentAsync(request.ToAcquiringBankProcessPaymentRequest());

        if (error != null)
        {
            var paymentEntity = request.ToPaymentEntity(paymentId, PaymentStatus.Declined, error.ErrorMessage);
            paymentsRepository.Add(paymentEntity);
            
            return request.ToProcessPaymentResponse(paymentId, PaymentStatus.Declined);
        }

        if (response == null)
        {
            throw new InvalidOperationException("Both response and error from acquiring bank are null. Something went wrong.");
        }
        
        if (response.Authorized)
        {
            var paymentEntity = request.ToPaymentEntity(paymentId, PaymentStatus.Authorized);
            
            // Should probably save the authorised code too?
            paymentsRepository.Add(paymentEntity);
            
            return request.ToProcessPaymentResponse(paymentId, PaymentStatus.Authorized);
        }
        else
        {
            var paymentEntity = request.ToPaymentEntity(paymentId, PaymentStatus.Declined, "Payment was declined by acquiring bank.");
            paymentsRepository.Add(paymentEntity);
            
            return request.ToProcessPaymentResponse(paymentId, PaymentStatus.Declined);
        }
    }
}