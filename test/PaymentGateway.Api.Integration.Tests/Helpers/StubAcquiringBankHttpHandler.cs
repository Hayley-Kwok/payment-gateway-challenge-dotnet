namespace PaymentGateway.Api.Tests.Helpers;

public static class StubAcquiringBankHttpHandler
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public StubHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    public static HttpClient CreateStubHttpClient(HttpResponseMessage response) => new(new StubHandler(response));
}
