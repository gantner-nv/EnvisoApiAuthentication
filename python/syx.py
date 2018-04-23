import base64
import datetime
import hashlib

from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_v1_5
from Crypto.Cipher import PKCS1_OAEP
import requests

pub_key = """-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCrtiJ5z5yHWJP7DKjhRjnQbkyY
e1Ijn1wP2UzSBAfxEBekDbMvw2T+BK6TZhxHzKX9IsTF8Vo8PUmNbomW6Qohd321
hgairRZ+BG0d7UQVdzB63r2QSbiNmvoayNB93LcITCOxrkXB3fyK7Edv66jF9pTs
l0mXSGZ0K7UzB28yHQIDAQAB
-----END PUBLIC KEY-----"""
api_key = 'KZ9vFiwKh0mlZrq+sxSEbg=='

timestamp = datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%S.%fZ')[:-4]+'Z'

print "timestamp:", timestamp
key = api_key + '_' + timestamp

print "message", key

sha_hash = hashlib.sha256(key).hexdigest()

print "sha256 hash:", sha_hash


rsa_key = RSA.importKey(pub_key)
cipher = PKCS1_v1_5.new(rsa_key)
signature = base64.encodestring(cipher.encrypt(sha_hash))

print "signature:", signature

response = requests.post(
    'https://api.staging-enviso.io/resellingapi/v1/apis/login',
    json={
        'apikey': api_key,
        'timestamp': timestamp,
        'signature': signature,
    },
)
print response.json()
