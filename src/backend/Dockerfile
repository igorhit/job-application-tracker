FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ApplicationTracker.Domain/ApplicationTracker.Domain.csproj ApplicationTracker.Domain/
COPY ApplicationTracker.Application/ApplicationTracker.Application.csproj ApplicationTracker.Application/
COPY ApplicationTracker.Infrastructure/ApplicationTracker.Infrastructure.csproj ApplicationTracker.Infrastructure/
COPY ApplicationTracker.API/ApplicationTracker.API.csproj ApplicationTracker.API/

RUN dotnet restore ApplicationTracker.API/ApplicationTracker.API.csproj

COPY . .

RUN dotnet publish ApplicationTracker.API/ApplicationTracker.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --gecos "" appuser && \
    mkdir -p /app/runtime && chown -R appuser:appuser /app

COPY --from=build /app/publish .

USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

EXPOSE 5001

ENTRYPOINT ["dotnet", "ApplicationTracker.API.dll"]
