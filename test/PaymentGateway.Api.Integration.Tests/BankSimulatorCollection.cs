namespace PaymentGateway.Api.Tests;

[CollectionDefinition("BankSimulator")]
public sealed class BankSimulatorCollection : ICollectionFixture<BankSimulatorDockerFixture>
{
    // Binds the docker compose fixture to the named collection so it runs once per test collection
}
