FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ReservasApi/ReservasApi.csproj", "ReservasApi/"]
RUN dotnet restore "ReservasApi/ReservasApi.csproj"
COPY . .
WORKDIR "/src/ReservasApi"
RUN dotnet build "ReservasApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReservasApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReservasApi.dll"]
