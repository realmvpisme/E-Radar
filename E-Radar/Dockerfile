#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["E-Radar/E-Radar.csproj", "E-Radar/"]
COPY ["E-Radar.Data/E-Radar.Data.csproj", "E-Radar.Data/"]
RUN dotnet restore "E-Radar/E-Radar.csproj"
COPY . .
WORKDIR "/src/E-Radar"
RUN dotnet build "E-Radar.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "E-Radar.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "E-Radar.dll"]