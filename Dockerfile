FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
# ENV ASPNETCORE_URLS=http://+:8080 \
#     DOTNET_EnableDiagnostics=0

COPY publish/ .

ENTRYPOINT ["dotnet", "WebAPI.dll"]
