using Library;

namespace EnvisoConsole;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Creating a login request for enviso.");

        var apiKey = RequiredEnv("ENVISO_API_KEY");
        var rsaPublicKey = RequiredEnv("ENVISO_PUBLIC_KEY");
        var tenantSecretKey = RequiredEnv("ENVISO_TENANT_SECRET");
        var baseUrl = Environment.GetEnvironmentVariable("ENVISO_BASE_URL") ?? EnvisoClient.DefaultBaseUrl;

        await ExecuteSimpleCallAsync(apiKey, rsaPublicKey, tenantSecretKey, baseUrl);
    }

    private static string RequiredEnv(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value))
        {
            Console.Error.WriteLine($"Missing required env var: {name}");
            Environment.Exit(1);
        }

        return value!;
    }

    public static async Task ExecuteSimpleCallAsync(
        string apiKey, string rsaPublicKey, string tenantSecretKey, string baseUrl = EnvisoClient.DefaultBaseUrl)
    {
        using var envisoClient = new EnvisoClient(apiKey, rsaPublicKey, tenantSecretKey, baseUrl: baseUrl);
        await envisoClient.InitializeAsync();

        Console.WriteLine($"Auth token: {envisoClient.AuthToken}");
    }
}
