FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore RateRelay.sln
RUN dotnet build RateRelay.API/RateRelay.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish RateRelay.API/RateRelay.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RateRelay.API.dll"]