# syntax=docker/dockerfile:1

# ----- Build stage -----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy sln and restore as distinct layers
COPY IndeConnect-Back.sln ./
COPY IndeConnect-Back/*.csproj IndeConnect-Back/
COPY IndeConnect-Back.Application/*.csproj IndeConnect-Back.Application/
COPY IndeConnect-Back.Domain/*.csproj IndeConnect-Back.Domain/
COPY IndeConnect-Back.Infrastructure/*.csproj IndeConnect-Back.Infrastructure/
COPY IndeConnect-Back.Web/*.csproj IndeConnect-Back.Web/

RUN dotnet restore ./IndeConnect-Back.sln

# Copy everything and build
COPY . .
RUN dotnet publish IndeConnect-Back.Web/IndeConnect-Back.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# ----- Runtime stage -----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Non-root user for better security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "IndeConnect-Back.Web.dll"]
