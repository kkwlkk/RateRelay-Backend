FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RateRelay.sln ./
COPY RateRelay.API/RateRelay.API.csproj RateRelay.API/
COPY RateRelay.Application/RateRelay.Application.csproj RateRelay.Application/
COPY RateRelay.Infrastructure/RateRelay.Infrastructure.csproj RateRelay.Infrastructure/
COPY RateRelay.Domain/RateRelay.Domain.csproj RateRelay.Domain/

RUN dotnet restore

COPY . .

RUN dotnet publish "RateRelay.API/RateRelay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RateRelay.API.dll"]