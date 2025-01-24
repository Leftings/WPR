FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/WPR.csproj ./backend/
RUN dotnet restore ./backend/WPR.csproj
COPY . .
RUN dotnet publish ./backend/WPR.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WPR.dll"]
