using System.Diagnostics;
using Xunit;

namespace PaymentGateway.Api.Tests;

public sealed class BankSimulatorDockerFixture : IAsyncLifetime
{
    private readonly string _composeFile = FindComposeFile()
                                           ?? throw new FileNotFoundException("Could not find docker-compose.yml starting from test bin directory.");

    public async Task InitializeAsync()
    {
        await RunDockerAsync($"compose -f \"{_composeFile}\" up -d --remove-orphans");
    }

    public async Task DisposeAsync()
    {
        await RunDockerAsync($"compose -f \"{_composeFile}\" down -v");
    }

    private static string? FindComposeFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docker-compose.yml");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    private static async Task RunDockerAsync(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi)!;
        var stdOut = await proc.StandardOutput.ReadToEndAsync();
        var stdErr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException($"Docker command failed: {args}\nSTDOUT:\n{stdOut}\nSTDERR:\n{stdErr}");
        }
    }
}
