FROM php:8.3-cli AS vendor
COPY --from=composer:2 /usr/bin/composer /usr/bin/composer
# unzip is needed for Composer's package downloader; the base php:8.3-cli image has neither
# the zip extension nor a CLI unzip tool installed.
RUN apt-get update && apt-get install -y --no-install-recommends unzip \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY composer.json .
# Composer must resolve dependencies under the same PHP version the runtime stage uses below
# (php:8.3-cli) -- running it via the generic composer:2 image picks package versions
# requiring whatever (newer) PHP that image bundles, which then fail to load at runtime here.
RUN composer install --no-dev --no-interaction --prefer-dist --no-scripts

FROM php:8.3-cli
WORKDIR /app
COPY --from=vendor /app/vendor vendor
COPY src/ src/
COPY run.php .

# Override with real credentials, e.g.:
#   docker run --rm -e ENVISO_API_KEY=... -e ENVISO_TENANT_SECRET=... -e ENVISO_PUBLIC_KEY="$(cat key.pem)" envisoapiauthentication-php
ENTRYPOINT ["php", "run.php"]
