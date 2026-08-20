# EnvisoApiAuthentication
Repository with sample code that demonstrates how to authenticate towards enviso.io using an apikey.

Currently, this includes sample code for .net, javascript, python and Postman.

## Signing mechanism

The login request is built as:

1. `{ApiKey}_{TimestampUtc}`, timestamp formatted `yyyy-MM-ddTHH:mm:ss.fffZ`.
2. SHA-256 hash it, hex-encoded.
3. RSA/PKCS#1-v1.5 *encrypt* that hex string with the public key Enviso issues for the
   tenant, base64-encoded, sent as the `Signature` field.

Despite the field name, step 3 is envelope encryption, not a digital signature — Enviso's
login endpoint decrypts it with the matching private key to verify the hash; the caller
never signs anything with a key of its own. Only the `.net` sample's naming/docs have been
updated to make this explicit so far — javascript/php/python/Postman still use the original
terminology and haven't been audited for drift from this description.