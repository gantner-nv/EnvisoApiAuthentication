using System;

namespace Library
{
    public class LoginRequestDTO
    {
        public LoginRequestDTO(string apiKey, string timestamp, string signature, string rsaEncryptionPadding)
        {
            Signature = signature;
            RsaEncryptionPadding = rsaEncryptionPadding;
            Timestamp = timestamp;
            ApiKey = apiKey;
        }

        public string ApiKey { get; }

        public string Timestamp { get; }

        public string Signature { get; }
        
        public string RsaEncryptionPadding { get; set; }
    }
}
