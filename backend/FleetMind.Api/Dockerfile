FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FleetMind.Api.csproj", "."]
RUN dotnet restore "FleetMind.Api.csproj"

COPY . .
RUN dotnet publish "FleetMind.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port configured in Compose
EXPOSE 8080

ENTRYPOINT ["dotnet", "FleetMind.Api.dll"]
