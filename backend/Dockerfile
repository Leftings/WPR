#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /app
#COPY . .
#RUN dotnet restore
#RUN dotnet publish -c Release -o /app/out

#FROM mcr.microsoft.com/dotnet/aspnet:8.0
#WORKDIR /app
#COPY --from=build /app/out .
#EXPOSE 5000
#CMD ["dotnet", "WPR.dll"]

# Use the official .NET image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./WPR.csproj ./ 
RUN dotnet restore "./WPR.csproj" 
WORKDIR /src/
RUN dotnet build "WPR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WPR.csproj" -c Release -o /app/publish

# Use the ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WPR.dll"]

