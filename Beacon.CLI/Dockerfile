FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Beacon.CLI/Beacon.CLI.csproj", "Beacon.CLI/"]
RUN dotnet restore "Beacon.CLI/Beacon.CLI.csproj"
COPY . .
WORKDIR "/src/Beacon.CLI"
RUN dotnet build "Beacon.CLI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Beacon.CLI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Beacon.CLI.dll"]
