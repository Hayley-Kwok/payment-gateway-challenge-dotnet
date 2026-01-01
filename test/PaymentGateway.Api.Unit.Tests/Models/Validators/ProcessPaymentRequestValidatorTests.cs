using FluentValidation.TestHelper;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Validators;

namespace PaymentGateway.Api.Unit.Tests.Models.Validators;


public class ProcessPaymentRequestValidatorTests
{
    private readonly ProcessPaymentRequestValidator _validator = new();

    private ProcessPaymentRequest CreateValidRequest()
    {
        var now = DateTime.UtcNow;
        return new ProcessPaymentRequest
        {
            CardNumber = "3503489848184448",
            ExpiryMonth = now.Month,
            ExpiryYear = now.Year + 1,
            Currency = "USD",
            CVV = "315",
            Amount = 1111
        };
    }

    [Fact]
    public void GivenValidData_ValidationShouldPass()
    {
        var model = CreateValidRequest();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenNonPositiveAmount_ValidationShouldFail(int amount)
    {
        var model = CreateValidRequest();
        model.Amount = amount;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(1050)]
    public void GivenValidAmount_ValidationShouldPass(int amount)
    {
        var model = CreateValidRequest();
        model.Amount = amount;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcd")]
    [InlineData("1234567890123")]        // 13 digits (too short)
    [InlineData("12345678901234567890")] // 20 digits (too long)
    public void GivenInvalidCardNumber_ValidationShouldFail(string cardNumber)
    {
        var model = CreateValidRequest();
        model.CardNumber = cardNumber;

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(13)]
    public void GivenOutOfRangeExpiryMonth_ValidationShouldFail(int month)
    {
        var model = CreateValidRequest();
        model.ExpiryMonth = month;

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth);
    }

    [Fact]
    public void GivenPastExpiryCombination_ValidationShouldFail()
    {
        var model = CreateValidRequest();
        model.ExpiryYear = 2025;
        model.ExpiryMonth = 11;

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void GivenNowExpiryCombination_ValidationShouldPass()
    {
        var now = DateTime.UtcNow;
        var model = CreateValidRequest();
        model.ExpiryYear = now.Year;
        model.ExpiryMonth = now.Month;

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDA")]
    [InlineData("")]
    public void GivenInvalidCurrency_ValidationShouldFail(string currency)
    {
        var model = CreateValidRequest();
        model.Currency = currency;

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    public void GivenInvalidCVV_ValidationShouldFail(string cvv)
    {
        var model = CreateValidRequest();
        model.CVV = cvv;

        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CVV);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void GivenValidCVV_ValidationShouldPass(string cvv)
    {
        var model = CreateValidRequest();
        model.CVV = cvv;

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CVV);
    }
}