using System;
using System.IO;
using System.Text;
using Library.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace Library
{
    public class LoginSignatureService
    {
        public string CreateSignature(string publicKey, string data, string rsaEncryptionPadding)
        {
            if (rsaEncryptionPadding == nameof(RsaEncryptionPadding.Pkcs1))
            {
                return CreatePkcs1Signature(publicKey, data);
            }

            if (rsaEncryptionPadding == nameof(RsaEncryptionPadding.OaepSha256))
            {
                return CreateOaepSha256Signature(publicKey, data);
            }

            throw new ArgumentOutOfRangeException(
                nameof(rsaEncryptionPadding), rsaEncryptionPadding, "Unsupported RSA encryption padding.");
        }

        private static string CreatePkcs1Signature(string publicKey, string data)
        {
            // PKCS#1 uses the uppercase hexadecimal hash text encoded as UTF-8.
            var bytesToEncrypt = Encoding.UTF8.GetBytes(ToHex(CreateSha256Hash(data)));
            return Encrypt(publicKey, bytesToEncrypt, new Pkcs1Encoding(new RsaEngine()));
        }

        private static string CreateOaepSha256Signature(string publicKey, string data)
        {
            // OAEP-SHA256 uses the raw hash bytes.
            var bytesToEncrypt = CreateSha256Hash(data);
            var encryptEngine = new OaepEncoding(
                new RsaEngine(), new Sha256Digest(), new Sha256Digest(), null);
            return Encrypt(publicKey, bytesToEncrypt, encryptEngine);
        }

        private static byte[] CreateSha256Hash(string data)
        {
            var input = Encoding.UTF8.GetBytes(data);
            var digest = new Sha256Digest();
            digest.BlockUpdate(input, 0, input.Length);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            return hash;
        }

        private static string Encrypt(string publicKey, byte[] bytesToEncrypt, IAsymmetricBlockCipher encryptEngine)
        {
            encryptEngine.Init(true, ReadPublicKey(publicKey));
            var encryptedBytes = encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static AsymmetricKeyParameter ReadPublicKey(string publicKey)
        {
            using (var reader = new StringReader(publicKey))
            {
                var keyObject = new PemReader(reader).ReadObject();
                if (keyObject is AsymmetricKeyParameter keyParameter)
                {
                    return keyParameter;
                }

                if (keyObject is AsymmetricCipherKeyPair keyPair)
                {
                    return keyPair.Public;
                }

                throw new InvalidOperationException("Invalid RSA public key.");
            }
        }

        private static string ToHex(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);
            foreach (var value in bytes)
            {
                result.Append(value.ToString("X2"));
            }

            return result.ToString();
        }

    }
}
