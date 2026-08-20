import base64
import hashlib
import os
import sys

from datetime import datetime, timezone
from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_v1_5

import requests


def required_env(name):
    value = os.environ.get(name)
    if not value:
        print(f"Missing required env var: {name}", file=sys.stderr)
        sys.exit(1)
    return value


pub_key = required_env('ENVISO_PUBLIC_KEY')
api_key = required_env('ENVISO_API_KEY')
url = os.environ.get('ENVISO_LOGIN_URL', 'https://api.staging-enviso.io/authenticationapi/v1/login/')

# %f always zero-pads to 6 digits (microseconds), so trimming the last 4 chars leaves exactly
# 3 digits (milliseconds) before the re-appended 'Z' -> yyyy-MM-ddTHH:mm:ss.fffZ.
timestamp = datetime.now(timezone.utc).strftime('%Y-%m-%dT%H:%M:%S.%fZ')[:-4] + 'Z'
print(f"TIMESTAMP: {timestamp}\n")

message = f"{api_key}_{timestamp}"  # wire scheme: {ApiKey}_{TimestampUtc}
print(f"MESSAGE: {message}\n")

sha_hash = hashlib.sha256(message.encode()).hexdigest()
print(f"SHA256 HASH: {sha_hash}\n")

# RSA/PKCS#1-v1.5 *encrypts* the hash with Enviso's public key - despite the wire field name
# "signature", this is envelope encryption, not a digital signature: Enviso decrypts it with
# the matching private key to verify the hash, we never sign with a key of our own.
rsa_key = RSA.importKey(pub_key)
cipher = PKCS1_v1_5.new(rsa_key)
signature = base64.b64encode(cipher.encrypt(sha_hash.encode())).decode()
print(f"SIGNATURE: {signature}\n")

response = requests.post(
    url,
    json={
        'apikey': api_key,
        'timestamp': timestamp,
        'signature': signature,
    },
    headers={'x-api-key': api_key, 'Content-Type': 'application/json'},
    timeout=10,
)

response.raise_for_status()
print(f"RESPONSE: {response.json()}\n")
