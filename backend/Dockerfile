
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY backend/WPR.csproj ./WPR/

RUN dotnet restore ./WPR/WPR.csproj

COPY backend/. .

RUN dotnet publish ./WPR/WPR.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "WPR.dll"]
