using System;
using Library.Models;

namespace Library
{
    public class LoginGenerator
    {
        public static readonly string API_LOGIN_TIMESTAMP_FORMAT = "yyyy'-'MM'-'ddTHH\\:mm\\:ss.fffZ";

        private readonly LoginSignatureService _loginSignatureService = new LoginSignatureService();

        public LoginRequestDTO GenerateLogin(string apiKey, string publicKey, string rsaEncryptionPadding)
        {
            var currentTimeStamp = DateTime.UtcNow.ToString(API_LOGIN_TIMESTAMP_FORMAT);
            var data = $"{apiKey}_{currentTimeStamp}";
            var signature = _loginSignatureService.CreateSignature(publicKey, data, rsaEncryptionPadding);

            return new LoginRequestDTO(apiKey, currentTimeStamp, signature, rsaEncryptionPadding);
        }
    }
}
