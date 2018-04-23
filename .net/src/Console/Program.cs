using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnvisoConsole
{
    class Program
    {
        private const string defaultApiKey = "KZ9vFiwKh0mlZrq+sxSEbg==";
        private const string defaultPublicRsaKey = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCrtiJ5z5yHWJP7DKjhRjnQbkyY
e1Ijn1wP2UzSBAfxEBekDbMvw2T+BK6TZhxHzKX9IsTF8Vo8PUmNbomW6Qohd321
hgairRZ+BG0d7UQVdzB63r2QSbiNmvoayNB93LcITCOxrkXB3fyK7Edv66jF9pTs
l0mXSGZ0K7UzB28yHQIDAQAB
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

            Library.LoginGenerator generator = new Library.LoginGenerator();
            var loginRequest = generator.GenerateLogin(apikey, rsaKey);
            Console.WriteLine("The Loginrequest is : ");
            var serializedRequest = Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest);
            Console.WriteLine(serializedRequest);

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                Console.WriteLine("Executing login: . . . ");
                var postTask = Task.Run(async () => await httpClient.PostAsync("https://api.staging-enviso.io/resellingapi/v1/apis/login", content));
                postTask.Wait(TimeSpan.FromSeconds(3));
                if (postTask.IsCompletedSuccessfully)
                {
                    var httpResponseMessage = postTask.Result;
                    Console.WriteLine($"{httpResponseMessage.StatusCode.ToString()} - {httpResponseMessage.Content.ReadAsStringAsync().Result}");
                }
                else
                {
                    Console.WriteLine($"An error occured: {postTask.Exception.ToString()}");
                }
            }

            Console.WriteLine("Press any key to close . . .");
            Console.ReadLine();
        }
    }
}
