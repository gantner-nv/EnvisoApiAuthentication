import base64
import datetime
import hashlib

from datetime import datetime, timezone
from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_v1_5
from Crypto.Cipher import PKCS1_OAEP

import requests

pub_key = """-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDRhyZgq+5gKBOoD5JAstVS5UM3
/nyl3JAubhEngWvYdzr7+0CSs4gevv+r4CLhkyWZuBNf/B7kXtg0SPJ/Lyp+diJI
4G0igA/ZT9jpdiT6GIugIwggeuIv0/ykEJMQBeSWQ29DgPf/hwO/PtRWGcOx2NxN
wh0ULIebM5cEzr3v4QIDAQAB
-----END PUBLIC KEY-----"""
api_key = 'h6ISMteE8k2+0thnmHJihg=='
url = 'https://api.staging-enviso.io/authenticationapi/v1/login/'

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
