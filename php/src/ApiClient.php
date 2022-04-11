<?php

namespace Enviso;

use Illuminate\Http\Client\Response;
use Carbon\Carbon;
use Illuminate\Support\Facades\Http;
use phpseclib3\Crypt\RSA;
use phpseclib3\Crypt\RSA\PublicKey;

class ApiClient
{
    private string $baseUrl;
    private string $apiKey;
    private string $publicKey;

    public function __construct(
        string $baseUrl,
        string $apiKey,
        string $publicKey,
    )
    {
        $this->baseUrl = $baseUrl;
        $this->apiKey = $apiKey;
        $this->publicKey = $publicKey;
    }

    private function getAuthToken(): string
    {
        $timestamp = Carbon::now(new \DateTimeZone('UTC'))->isoFormat('YYYY-MM-DDTHH:mm:ss.SSS\Z');

        $hash = hash('sha256', $this->apiKey . '_' . $timestamp);

        /** @var PublicKey $rsa */
        $rsa = PublicKey::loadPublicKey($this->publicKey)->withPadding(RSA::ENCRYPTION_PKCS1);

        $signature = base64_encode($rsa->encrypt($hash));

        $response = Http::acceptJson()->post($this->baseUrl . "resellingapi/v1/apis/login", [
            'apikey' => $this->apiKey,
            'timestamp' => $timestamp,
            'signature' => $signature
        ]);

        if ($response->ok()) {
            return $response->json('authToken', '');
        }

        return '';
    }
}
