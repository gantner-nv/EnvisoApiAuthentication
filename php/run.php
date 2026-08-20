<?php

declare(strict_types=1);

require __DIR__ . '/vendor/autoload.php';

use Enviso\ApiClient;
use Illuminate\Http\Client\Factory as HttpFactory;
use Illuminate\Support\Facades\Facade;

/**
 * Stand-in for Laravel's cache store — just enough for ApiClient's
 * Cache::has()/get()/put() calls, so this sample can run outside a full Laravel app
 * without pulling in illuminate/cache and a config repository.
 */
final class InMemoryCache
{
    private array $values = [];
    private array $expiresAt = [];

    public function has(string $key): bool
    {
        if (!array_key_exists($key, $this->values)) {
            return false;
        }

        if ($this->expiresAt[$key] !== null && $this->expiresAt[$key] < time()) {
            unset($this->values[$key], $this->expiresAt[$key]);
            return false;
        }

        return true;
    }

    public function get(string $key, mixed $default = null): mixed
    {
        return $this->has($key) ? $this->values[$key] : $default;
    }

    public function put(string $key, mixed $value, ?int $ttlSeconds = null): bool
    {
        $this->values[$key] = $value;
        $this->expiresAt[$key] = $ttlSeconds === null ? null : time() + $ttlSeconds;
        return true;
    }
}

/** Minimal ArrayAccess "app" so Facade::setFacadeApplication() has somewhere to resolve from. */
final class DemoApp implements ArrayAccess
{
    public function __construct(private array $bindings)
    {
    }

    public function offsetExists(mixed $key): bool
    {
        return isset($this->bindings[$key]);
    }

    public function offsetGet(mixed $key): mixed
    {
        return $this->bindings[$key];
    }

    public function offsetSet(mixed $key, mixed $value): void
    {
        $this->bindings[$key] = $value;
    }

    public function offsetUnset(mixed $key): void
    {
        unset($this->bindings[$key]);
    }
}

Facade::setFacadeApplication(new DemoApp([
    'cache' => new InMemoryCache(),
    HttpFactory::class => new HttpFactory(),
]));

function requiredEnv(string $name): string
{
    $value = getenv($name);
    if ($value === false || $value === '') {
        fwrite(STDERR, "Missing required env var: {$name}\n");
        exit(1);
    }

    return $value;
}

$apiClient = new ApiClient(
    baseUrl: getenv('ENVISO_BASE_URL') ?: 'https://api.staging-enviso.io/',
    tenantSecret: requiredEnv('ENVISO_TENANT_SECRET'),
    apiKey: requiredEnv('ENVISO_API_KEY'),
    publicKey: requiredEnv('ENVISO_PUBLIC_KEY'),
);

echo "Requesting Enviso auth token...\n";
echo "Auth token: " . $apiClient->getAuthToken() . "\n";
