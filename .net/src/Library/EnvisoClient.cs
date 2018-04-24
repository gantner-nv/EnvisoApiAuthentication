using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Library
{
    public class EnvisoClient
    {
        private const string TENANTSECRETHEADER = "x-tenantsecretkey";
        private const string APIKEYHEADER = "x-api-key";
        private const string AUTHENTICATIONSCHEME = "bearer";
        private bool _isInitialized = false;
        private string _authToken = string.Empty;

        public string TenantSecretKey { get; }
        public string RsaKey { get; }
        public string ApiKey { get; }

        /// <summary>
        /// Creates an EnvisoClient with an existing authenticationtoken.
        /// </summary>
        /// <param name="apiKey">the apikey to use for further requests</param>
        /// <param name="authToken">the authtoken to use for further requests (authorisation) </param>
        /// <param name="tenantSecretKey">the tenantsecretkey to use for further requests (authentication)</param>
        public EnvisoClient(string apiKey, string rsaKey, string tenantSecretKey)
        {
            ApiKey = apiKey;
            RsaKey = rsaKey;
            TenantSecretKey = tenantSecretKey;
        }

        /// <summary>
        /// Initializing the client will login into Enviso.
        /// </summary>
        public async Task Initialize()
        {
            Library.LoginGenerator generator = new Library.LoginGenerator();
            var loginRequest = generator.GenerateLogin(ApiKey, RsaKey);

            var serializedRequest = Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest);

            // we shouldn't create a HTTPclient for each call but share this accross different calls. 
            // For simplicity of the sample code this  is kept as an optimization.
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                var loginResponse = await httpClient.PostAsync(URI.LOGIN, content);
                var loginResponseDTO = await ReadResponseAndDeserialize<LoginResponseDTO>(loginResponse);
                this._authToken = loginResponseDTO.AuthToken;

            }
            _isInitialized = true;
        }

        public async Task<TResponse> GetAsync<TResponse>(string uri)
        {
            if (!_isInitialized) throw new System.InvalidOperationException("Please call initialize first before executing future calls");

            // we shouldn't create a HTTPclient for each call but share this accross different calls. 
            // For simplicity of the sample code this  is kept as an optimization.
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AUTHENTICATIONSCHEME, _authToken);
                httpClient.DefaultRequestHeaders.Add(TENANTSECRETHEADER, TenantSecretKey);
                httpClient.DefaultRequestHeaders.Add(APIKEYHEADER, ApiKey);
                var getRawResult = await httpClient.GetAsync(uri);
                if (getRawResult.Content == null) return default(TResponse);

                // Get result
                return await ReadResponseAndDeserialize<TResponse>(getRawResult);
            }
        }

        private async Task<TResponse> ReadResponseAndDeserialize<TResponse>(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return Deserialize<TResponse>(responseContent);
            }
            else
            {
                if (responseMessage.Content != null)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    throw new System.Exception($"Call was not succesfull.{Environment.NewLine}Status: {responseMessage.StatusCode}{Environment.NewLine}Response: '{responseContent}'");
                }
                throw new System.Exception($"Call was not succesfull.{Environment.NewLine}Status: {responseMessage.StatusCode}");
            }
        }

        private TResponse Deserialize<TResponse>(string value)
        {
            try
            {
                // Deserialize to the desired type
                var responseObj = JsonConvert.DeserializeObject<TResponse>(value);
                return responseObj;
            }
            catch (JsonSerializationException)
            {
                // do exception handling over here
                throw;
            }
        }

        public static class URI
        {
            public const string LOGIN = "https://api.staging-enviso.io/resellingapi/v1/apis/login";
            public const string VENUES = "https://api.staging-enviso.io/resellingapi/v1/venues";
        }
    }
}