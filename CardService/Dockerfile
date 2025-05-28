# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy full source tree
COPY . .

# Restore using full path to the project
RUN dotnet restore "CardService/CardService.csproj"

# Build and publish
RUN dotnet publish "CardService/CardService.csproj" -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CardService.dll"]


