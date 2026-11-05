FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CreditApplication.Domain/CreditApplication.Domain.csproj src/CreditApplication.Domain/
COPY src/CreditApplication.Application/CreditApplication.Application.csproj src/CreditApplication.Application/
COPY src/CreditApplication.Infrastructure/CreditApplication.Infrastructure.csproj src/CreditApplication.Infrastructure/
COPY src/CreditApplication.Api/CreditApplication.Api.csproj src/CreditApplication.Api/
RUN dotnet restore src/CreditApplication.Api/CreditApplication.Api.csproj

COPY src/ src/
RUN dotnet publish src/CreditApplication.Api/CreditApplication.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CreditApplication.Api.dll"]
