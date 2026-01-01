# Payment Gateway - API
## Functional Requirements
1. Process Payment (POST)
- implement an endpoint to process payment and return Authorized/Declined/Rejected
- the endpoint request should contain with validation
    - Card Number
    - Card Expiry Month/Year
    - CVV
    - Amount
    - Currency
- this endpoint should call the acquiring bank to process the payment

2. Payment Details Retrieval (GET)
- implement an endpoint to retrieve payment details by payment identifier

## Updated structure
```
src/
  PaymentGateway.Api/
    PaymentGateway.Api.csproj
    Program.cs                    # Spin up of the whole api & DI configuration
    Controllers/
      PaymentsController.cs       # HTTP endpoints: POST process payment, GET payment details (kept minimal for easier testing)
    Models/
      Entities/
        PaymentEntity.cs          # Stored payment record; used for retrieval and auditing
      Requests/
        AcquiringBankProcessPaymentRequest.cs  # Input request DTO for request to acquiring bank
        ProcessPaymentRequest.cs  # Input request DTO for processing a payment
      Responses/
        AcquiringBankProcessPaymentResponse.cs      # Bank response DTO
        AcquiringBankProcessPaymentErrorResponse.cs # DTO for storing bank error responses from the http call. Saved for auditing.
        ProcessPaymentResponse.cs                   # Output response DTO for process payment endpoint (POST)
        PostPaymentResponse.cs                      # Output response DTO for payment details retrieval endpoint (GET)
      Validators/
        ProcessPaymentRequestValidator.cs      # FluentValidation rules for ProcessPaymentRequest input
    Services/
      Clients/
        AcquiringBankClient.cs    # Encapsulates external communication with acquiring bank
      Processors/
        PaymentProcessor.cs       # Orchestrates validation, bank calls, and persistence
      Repositories/
        PaymentsRepository.cs     # In-memory storage implementation
      Retrievers/
        PaymentRetriever.cs       # Retrieves stored payments for GET endpoint
    appsettings.json
    appsettings.Development.json

test/
  PaymentGateway.Api.Unit.Tests/        # for unit testing  individual components
  PaymentGateway.Api.Integration.Tests/ # Endpoint-level tests with stubbed bank                            
```

## Made Design Choices
- Modular, SOLID-oriented design
    - Single Responsibility: See above structure graph; each component has a focused responsibility.
    - Open/Closed: New features (e.g., logging, retry logic) can be added by extending existing components or adding new ones without modifying existing code.
    - Liskov & Interface Segregation: Small, focused interfaces make mocks/stubs straightforward and interchangeable in tests.
    - Dependency Inversion: High-level components depend on interface. Wiring is done via DI for easy substitution and testing.
- Testing strategy
    - Unit tests: test components in isolation using mocks/stubs for dependencies. Make sure code paths are covered.
    - Integration tests: test the integration of components to make sure they work together as expected. Only stub the acquiring bank to simulate responses as it is an external dependency.
    - Manual testing: run the API and use bruno to test that endpoints behave as expected.
  
- How to handle invalid input for the process payment endpoint
  - Used FluentValidation to define validation rules for the ProcessPaymentRequest model and return only the rejected status and payment id back (400 bad request). 
  - The PaymentsController checks if all fields are present with the correct datatype and returns a 400 Bad Request with validation errors if invalid.
  - The more natural approach will be using fluent validation to reject the request before it reaches the processor layer but based on the requirement, it is implemented this way. 
    - If rejected status is absolutely required, this could be gotten around with custom middleware but this should be something that require further discussion.
  - Also, the rejection reason is not returned back to the client as it wasn't specified in the requirements. I didn't return it for security reasons (to avoid leaking validation rules).
## Other nice to have features 
Implemented:
I added an extra feature that I believe would be nice to have in a real-world payment gateway:
- Saving the failure reason when a payment is declined by the acquiring bank/rejected due to validation. This can help with troubleshooting and analytics or simply responding to a support request from clients.

Future improvements that could be made given more time:
- Idempotency: Ensure that repeated requests with the same idempotency key do not result in duplicate payments.
- Logging: Implement structured logging (e.g., using Serilog) to capture detailed information about payment processing for monitoring and debugging.
- Retry Logic: Implement retry mechanisms for transient failures when communicating with the acquiring bank.

Disclaimer: I did make use of copilot to assist with boilerplate code and repetitive patterns to speed up the implementation.