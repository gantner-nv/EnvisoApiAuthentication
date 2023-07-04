<?php

namespace Enviso;

use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Http;
use phpseclib3\Crypt\PublicKeyLoader;
use phpseclib3\Crypt\RSA;
use phpseclib3\Crypt\RSA\PublicKey;

class ApiClient
{
    private string $baseUrl;
    private string $tenantSecret;
    private string $apiKey;
    private string $publicKey;

    public function __construct(
        string $baseUrl,
        string $tenantSecret,
        string $apiKey,
        string $publicKey,
    )
    {
        $this->baseUrl = $baseUrl;
        $this->tenantSecret = $tenantSecret;
        $this->apiKey = $apiKey;
        $this->publicKey = $publicKey;
    }

    // If no token exists, create one. If one exists but the 60min long lived cache entry doesn't indicate its active anymore, renew it.
    private function getAuthToken(): string
    {
        if (!Cache::has('envisopay.authToken')) {
            $this->createNewToken();
        } elseif (!Cache::has('envisopay.authTokenStillActive')) {
            $this->renewExistingToken();
        }

        return Cache::get("envisopay.authToken");
    }

    private function createNewToken(): void
    {

        $timestamp = Carbon::now(new \DateTimeZone('UTC'))->isoFormat('YYYY-MM-DDTHH:mm:ss.SSS\Z');

        $hash = hash('sha256', $this->apiKey . '_' . $timestamp);

        $rsa = PublicKeyLoader::load($this->publicKey)->withPadding(RSA::ENCRYPTION_PKCS1);

        $signature = base64_encode($rsa->encrypt($hash));
        $authEndpoint = $this->baseUrl . "authenticationapi/v1/login";


        $response = Http::withHeaders(['x-api-key' => $this->apiKey])
            ->post($authEndpoint, [
                'apikey' => $this->apiKey,
                'timestamp' => $timestamp,
                'signature' => $signature
            ]);

        if ($response->ok()) {
            $result = $response->json();
            Cache::put("envisopay.authToken", $result["authToken"], 60 * 60 * 24 * 30); // Cached for 30 days
            Cache::put("envisopay.refreshToken", $result["refreshToken"], 60 * 60 * 24 * 30); // Cached for 30 days
        }

    }

    private function renewExistingToken(): void
    {
        $authToken = Cache::get("envisopay.authToken");
        $refreshToken = Cache::get("envisopay.refreshToken");

        $authEndpoint = $this->baseUrl . "authenticationapi/v1/renew";

        $renewal = Http::withHeaders([
            'x-api-key' => $this->apiKey,
            'Authorization' => 'bearer ' . $authToken
        ])
            ->post($authEndpoint,
                [
                    'refreshToken' => $refreshToken,
                ]
            );

        if ($renewal->ok()) {
            Cache::put("envisopay.authTokenStillActive", true, 60 * 60); // If one hour has passed, its due to renewal
        } else {
            $this->createNewToken();
        }

    }
}