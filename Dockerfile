# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish "./Kozma.net.csproj" -c Release -o out --no-restore

COPY .env ./out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "Kozma.net.dll"]
