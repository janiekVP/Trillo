# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything from root context
COPY . .

# Fix: reference the full relative path from /src
RUN dotnet restore "BoardService/BoardService.csproj"
RUN dotnet publish "BoardService/BoardService.csproj" -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "BoardService.dll"]
