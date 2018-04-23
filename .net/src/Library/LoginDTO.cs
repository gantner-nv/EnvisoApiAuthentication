using System;

namespace Library
{
    public class LoginDTO
    {
        public LoginDTO(string apiKey, string timestamp, string signature)
        {
            Signature = signature;
            Timestamp = timestamp;
            ApiKey = apiKey;
        }

        public string ApiKey { get; }

        public string Timestamp { get; }

        public string Signature { get; }
    }
}
