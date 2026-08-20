using System.Security.Cryptography;
using System.Text;

namespace Library;

public class LoginGenerator
{
    public const string ApiLoginTimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public LoginRequestDTO GenerateLogin(string apiKey, string rsaPublicKeyPem)
    {
        var timestamp = DateTime.UtcNow.ToString(ApiLoginTimestampFormat);
        var hash = CreateSha256Hash(CreateDataToEncrypt(apiKey, timestamp));
        var signature = EncryptWithPublicKey(rsaPublicKeyPem, hash);
        return new LoginRequestDTO(apiKey, timestamp, signature);
    }

    private static string CreateDataToEncrypt(string apiKey, string currentTimeStamp) =>
        $"{apiKey}_{currentTimeStamp}";

    private static string CreateSha256Hash(string data) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();

    /// <summary>
    /// Encrypts <paramref name="data"/> with the public part of Enviso's asymmetric keypair
    /// using RSA/PKCS#1 v1.5. This is envelope encryption, not a digital signature — see
    /// <see cref="LoginRequestDTO.Signature"/> for why the wire field is still called that.
    /// </summary>
    /// <param name="rsaPublicKeyPem">the PEM-encoded public key Enviso issued for the tenant</param>
    /// <param name="data">the original data to encrypt</param>
    /// <returns>the encrypted data, base64-encoded</returns>
    private static string EncryptWithPublicKey(string rsaPublicKeyPem, string data)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(rsaPublicKeyPem);

        var encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        return Convert.ToBase64String(encrypted);
    }
}
