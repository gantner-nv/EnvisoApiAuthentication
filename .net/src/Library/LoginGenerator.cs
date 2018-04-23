using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace Library
{
    public class LoginGenerator
    {
        public static readonly string API_LOGIN_TIMESTAMP_FORMAT = "yyyy'-'MM'-'ddTHH\\:mm\\:ss.fffZ";

        public LoginDTO GenerateLogin(string apiKey, string publicKey)
        {
            var currentTimeStamp = DateTime.UtcNow.ToString(API_LOGIN_TIMESTAMP_FORMAT);

            // Create the data to encrypt
            var data = CreateDataToEncrypt(apiKey, currentTimeStamp);

            // encrypt the data
            var encrypted = CreateSHA256Hash(data);

            // create a signature by encrypting that data.
            var signature = EncryptWithPublicKey(publicKey, encrypted);

            return new LoginDTO(apiKey, currentTimeStamp, signature);
        }

        private string CreateDataToEncrypt(string apikey, string currentTimeStamp)
        {
            return $"{apikey}_{currentTimeStamp}";
        }

        private string CreateSHA256Hash(string data)
        {
            var encData = Encoding.UTF8.GetBytes(data);
            Org.BouncyCastle.Crypto.Digests.Sha256Digest myHash = new Org.BouncyCastle.Crypto.Digests.Sha256Digest();
            myHash.BlockUpdate(encData, 0, encData.Length);
            byte[] compArr = new byte[myHash.GetDigestSize()];
            myHash.DoFinal(compArr, 0);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < compArr.Length; i++)
            {
                result.Append(compArr[i].ToString("X2"));
            }
            return result.ToString();
        }

        /// <summary>
        /// Encrypts data with the given public part of the asymmetric keypair using the RSA algorithm
        /// </summary>
        /// <param name="publicKey">the public part of the asymetric keypair</param>
        /// <param name="data">the original data to encrypt</param>
        /// <returns>the encrypted data in a base 64 string</returns>
        private string EncryptWithPublicKey(string publicKey, string data)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(data);
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new Org.BouncyCastle.OpenSsl.PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }
    }
}
