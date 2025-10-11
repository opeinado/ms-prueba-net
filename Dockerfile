# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY CreditPro.sln ./
COPY CreditPro.Api/CreditPro.Api.csproj CreditPro.Api/
COPY CreditPro.Application/CreditPro.Application.csproj CreditPro.Application/
COPY CreditPro.Infrastructure/CreditPro.Infrastructure.csproj CreditPro.Infrastructure/
COPY CreditPro.Domain/CreditPro.Domain.csproj CreditPro.Domain/
COPY CreditPro.Tests/CreditPro.Tests.csproj CreditPro.Tests/

RUN dotnet restore CreditPro.Api/CreditPro.Api.csproj

COPY . .

RUN dotnet publish CreditPro.Api/CreditPro.Api.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "CreditPro.Api.dll"]
