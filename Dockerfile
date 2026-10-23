# Stage 1: build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY DigitalWardrobe.slnx ./
COPY src/DigitalWardrobe.Core/DigitalWardrobe.Core.csproj src/DigitalWardrobe.Core/
COPY src/DigitalWardrobe.Data/DigitalWardrobe.Data.csproj src/DigitalWardrobe.Data/
COPY src/DigitalWardrobe.Web/DigitalWardrobe.Web.csproj src/DigitalWardrobe.Web/
RUN dotnet restore src/DigitalWardrobe.Web/DigitalWardrobe.Web.csproj

COPY src/ src/
RUN dotnet publish src/DigitalWardrobe.Web/DigitalWardrobe.Web.csproj -c Release -o /app --no-restore

# Stage 2: run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "DigitalWardrobe.Web.dll"]