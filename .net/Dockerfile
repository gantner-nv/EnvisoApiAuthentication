FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Console/Console.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Override with real credentials, e.g.:
#   docker run --rm -e ENVISO_API_KEY=... -e ENVISO_TENANT_SECRET=... -e ENVISO_PUBLIC_KEY="$(cat key.pem)" envisoapiauthentication-net
ENTRYPOINT ["dotnet", "Console.dll"]
