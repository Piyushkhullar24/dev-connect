FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DevConnectDataService/DevConnectDataService.csproj", "DevConnectDataService/"]
COPY ["DocumentDbRepository/DocumentDbRepository.csproj", "DocumentDbRepository/"]
RUN dotnet restore "DevConnectDataService/DevConnectDataService.csproj"
COPY . .
WORKDIR "/src/DevConnectDataService"
RUN dotnet build "DevConnectDataService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DevConnectDataService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevConnectDataService.dll"]