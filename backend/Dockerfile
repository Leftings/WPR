FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
 
WORKDIR /src
COPY bin/Debug/net8.0/publish .
 
ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
 
ENTRYPOINT ["dotnet", "backend.dll"]
