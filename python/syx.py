import base64
import datetime
import hashlib

from datetime import datetime, timezone
from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_v1_5
from Crypto.Cipher import PKCS1_OAEP

import requests

pub_key = """-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCToCIGT0CRz+8ZRfU105P2aM8t
Ndp1zBuQxyQ7FGVmswd2h72cafutkX0PL5GbcgqZTzhms3QkpzYu29YDOJKp4h1Q
8xkLaHQfgHmXg6BA+HNg3r2vIlsc0tGMDC7UrGF40FKEIVo2oIIQv0AygYo+BxAm
zrCnQXkdGo8FBq2JMQIDAQAB
-----END PUBLIC KEY-----"""
api_key = 'MPM7O0JOYkqHzfj3JtgLxg=='
url = 'https://api.test-enviso.io/authenticationapi/v1/login/'

timestamp = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ')[:-4]+'Z'

print("TIMESTAMP: ", timestamp, "\n")
key = api_key + '_' + timestamp

print("MESSAGE: ", key, "\n")

sha_hash = hashlib.sha256(key.encode()).hexdigest()

print("SHA256 HASH: ", sha_hash, "\n")

rsa_key = RSA.importKey(pub_key)
cipher = PKCS1_v1_5.new(rsa_key)

signature = base64.b64encode(cipher.encrypt(sha_hash.encode()))

print("SIGNATURE: ", signature, "\n")

response = requests.post(
    url,
    json={
        'apikey': api_key,
        'timestamp': timestamp,
        'signature': signature.decode(),
    }, headers = {'x-api-key': api_key, 'Content-Type': 'application/json',}
)

print ("RESPONSE: ", response.json(), "\n")
