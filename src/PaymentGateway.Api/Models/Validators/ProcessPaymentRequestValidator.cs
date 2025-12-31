using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models.Validators;

public class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Matches(@"^\d+$").WithMessage("CardNumber must be numeric.")
            .Must(c => c.Length is >= 14 and <= 19).WithMessage("CardNumber must be between 14 and 19 characters long.");
        
        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12);

        RuleFor(x => x)
            .Must(request => 
            {
                var now = DateTime.UtcNow;
                return request.ExpiryYear > now.Year || 
                       (request.ExpiryYear == now.Year && request.ExpiryMonth >= now.Month);
            })
            .WithMessage("Card expiry date must not be in the past.");
        
        RuleFor(x => x.Currency).Must(c => c.Length == 3)
            .WithMessage("Currency must be a valid 3-letter ISO currency code.");
        RuleFor(x => x.CVV)
            .Matches(@"^\d+$").WithMessage("CVV must be numeric.")
            .Must(c => c.Length is 3 or 4).WithMessage("CVV must be 3 or 4 digits long.");
    }
}