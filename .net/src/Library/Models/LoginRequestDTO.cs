namespace Library;

/// <param name="ApiKey">the apikey the login request is for</param>
/// <param name="Timestamp">the UTC timestamp the signature was generated at</param>
/// <param name="Signature">
/// The SHA-256 hash of "{ApiKey}_{Timestamp}", RSA/PKCS#1-v1.5 encrypted with Enviso's
/// public key and base64-encoded. Named "Signature" to match the wire contract, but this
/// is envelope encryption, not a digital signature: Enviso decrypts it with the matching
/// private key to verify the hash, the caller never signs with a key of its own. See
/// <see cref="LoginGenerator"/>.
/// </param>
public record LoginRequestDTO(string ApiKey, string Timestamp, string Signature);
