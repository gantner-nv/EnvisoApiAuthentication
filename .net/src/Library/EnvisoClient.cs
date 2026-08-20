using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Library;

public class EnvisoClient : IDisposable
{
    public const string DefaultBaseUrl = "https://api.staging-enviso.io/";

    private const string TenantSecretHeader = "x-tenantsecretkey";
    private const string ApiKeyHeader = "x-api-key";
    private const string AuthenticationScheme = "Bearer";

    // Enviso's JSON responses use camelCase field names (e.g. "authToken"), but the DTOs
    // here use PascalCase properties for idiomatic C# — case-insensitive matching is needed
    // or every property silently deserializes to null instead of throwing.
    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly LoginGenerator _loginGenerator = new();
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private string? _authToken;

    public string ApiKey { get; }
    public string RsaPublicKey { get; }
    public string TenantSecretKey { get; }
    public Uri BaseUri { get; }
    public Uri LoginUri => new(BaseUri, "authenticationapi/v1/login");
    public string? AuthToken => _authToken;

    /// <param name="apiKey">the apikey to use for further requests</param>
    /// <param name="rsaPublicKey">the PEM-encoded RSA public key Enviso issued for the tenant, used to sign the login request</param>
    /// <param name="tenantSecretKey">the tenantsecretkey to use for further requests (authentication)</param>
    /// <param name="httpClient">
    /// an existing <see cref="HttpClient"/> to reuse (e.g. one obtained from
    /// <c>IHttpClientFactory</c>). If omitted, <see cref="EnvisoClient"/> creates and owns its
    /// own instance, disposed when the client itself is disposed.
    /// </param>
    /// <param name="baseUrl">the Enviso API base URL; defaults to the staging environment</param>
    public EnvisoClient(
        string apiKey,
        string rsaPublicKey,
        string tenantSecretKey,
        HttpClient? httpClient = null,
        string baseUrl = DefaultBaseUrl)
    {
        ApiKey = apiKey;
        RsaPublicKey = rsaPublicKey;
        TenantSecretKey = tenantSecretKey;
        BaseUri = new Uri(baseUrl, UriKind.Absolute);

        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Logs into Enviso and caches the access token for subsequent calls.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _authToken = await LoginAsync(cancellationToken);
    }

    public async Task<TResponse?> GetAsync<TResponse>(Uri uri, CancellationToken cancellationToken = default)
    {
        if (_authToken is null)
        {
            throw new InvalidOperationException(
                $"Call {nameof(InitializeAsync)} before making requests.");
        }

        var response = await SendAuthenticatedGetAsync(uri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Enviso's authenticationapi/v1/renew endpoint is documented as deprecated in
            // favor of just logging in again (LoginResponseDTO.RefreshToken is captured but
            // unused), so this re-runs the full login on expiry rather than refreshing.
            response.Dispose();
            _authToken = await LoginAsync(cancellationToken);
            response = await SendAuthenticatedGetAsync(uri, cancellationToken);
        }

        using (response)
        {
            return await ReadResponseAndDeserializeAsync<TResponse>(response, cancellationToken);
        }
    }

    private async Task<string> LoginAsync(CancellationToken cancellationToken)
    {
        var loginRequest = _loginGenerator.GenerateLogin(ApiKey, RsaPublicKey);
        using var request = new HttpRequestMessage(HttpMethod.Post, LoginUri)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add(ApiKeyHeader, ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var loginResponse = await ReadResponseAndDeserializeAsync<LoginResponseDTO>(response, cancellationToken);
        return loginResponse!.AuthToken;
    }

    private async Task<HttpResponseMessage> SendAuthenticatedGetAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, _authToken);
        request.Headers.Add(TenantSecretHeader, TenantSecretKey);
        request.Headers.Add(ApiKeyHeader, ApiKey);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<TResponse?> ReadResponseAndDeserializeAsync<TResponse>(
        HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        var responseContent = responseMessage.Content is null
            ? null
            : await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Call was not successful.{Environment.NewLine}Status: {responseMessage.StatusCode}" +
                (responseContent is null ? string.Empty : $"{Environment.NewLine}Response: '{responseContent}'"));
        }

        return responseContent is null
            ? default
            : JsonSerializer.Deserialize<TResponse>(responseContent, ResponseJsonOptions);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
