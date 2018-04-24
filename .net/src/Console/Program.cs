using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Library;
using Library.Models;
using Newtonsoft.Json;

namespace EnvisoConsole
{
    class Program
    {
        private const string defaultTenantSecretKey = "mosIgBkcR0qKeZenWmpE/A==";
        private const string defaultApiKey = "L5MhJYSCp06SpYlI2cjbHg==";
        private const string defaultPublicRsaKey = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQChrE9eekbvWaz7Rv80UWpq7lwz
zQlQOQoTU7OxEFDVsftVyHus/MLQbCbIgZoo3i16ocY5VKKjqP8EiCORP+CU5SBA
oLGfsgIRLqzPT+6DcWZckmkpZRfKd51O/6QByIFCwQKWYcrqrZDzJCGBiZSuv8rd
85RRfYuXSHNyachyvwIDAQAB
-----END PUBLIC KEY-----";

        static void Main(string[] args)
        {
            Console.WriteLine("Creating a login request for enviso.");

            Console.WriteLine($"Please fill in your APIKEY: {Environment.NewLine} eg:{Environment.NewLine}{defaultApiKey}");
            var apikey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(apikey))
            {
                apikey = defaultApiKey;
            };

            Console.WriteLine($"Please fill in your Public RSA Key: {Environment.NewLine}eg: {Environment.NewLine}{defaultPublicRsaKey}");
            var rsaKey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rsaKey))
            {
                rsaKey = defaultPublicRsaKey;
            }

            Console.WriteLine($"Please fill in your tenantsecret key: {Environment.NewLine}eg: {Environment.NewLine}{defaultTenantSecretKey}");
            var tenantSecretKey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(tenantSecretKey))
            {
                tenantSecretKey = defaultTenantSecretKey;
            }

            ExecuteSimpleCall(apikey, rsaKey, tenantSecretKey);

            Console.ReadLine();            
        }

        public static void ExecuteSimpleCall(string apiKey, string rsaKey, string tenantSecretKey)
        {
            var envisoClient = new EnvisoClient(apiKey, rsaKey, tenantSecretKey);
            var initializeTask = Task.Run(async () => await envisoClient.Initialize());
            initializeTask.Wait();
            var getVenuesTask = Task.Run(async () => await envisoClient.GetAsync<IEnumerable<VenueModelDTO>>(EnvisoClient.URI.VENUES));
            
            getVenuesTask.Wait();
            Console.WriteLine($"Result of get venues is {JsonConvert.SerializeObject(getVenuesTask.Result)}");
        }
    }
}
