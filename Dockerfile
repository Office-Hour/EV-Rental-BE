# syntax=docker/dockerfile:1
############################################
# Base runtime image
############################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
# Add DefaultConnection (empty by default; override at runtime)
# ENV 
# ConnectionStrings__DefaultConnection=""
# Jwt__Key=""
# Jwt__AccessTokenExpiration="10"
# Jwt__RefreshTokenExpiration="7"
# Jwt__REFRESH_TOKEN_CLAIM_TYPE="RefreshToken"
# Jwt__REFRESH_TOKEN_EXPIRATION_CLAIM_TYPE="RefreshTokenExpiration"

############################################
# Build & publish
############################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["WebAPI/WebAPI.csproj", "./"]
RUN dotnet restore "WebAPI.csproj"
COPY . .
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

############################################
# Final image
############################################
FROM base AS final
RUN addgroup --system app && adduser --system --ingroup app appuser
WORKDIR /app
COPY --from=publish /app/publish .
RUN chown -R appuser:app /app
USER appuser
ENTRYPOINT ["dotnet", "EV-Rental-BE.dll"]