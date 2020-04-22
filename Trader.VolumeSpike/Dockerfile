#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
#WORKDIR /src
#COPY ["Trader.VolumeSpike/Trader.VolumeSpike.csproj", "Trader.VolumeSpike/"]
#RUN dotnet restore "Trader.VolumeSpike/Trader.VolumeSpike.csproj"
#COPY . .
#WORKDIR "/src/Trader.VolumeSpike"
#RUN dotnet build "Trader.VolumeSpike.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Trader.VolumeSpike.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Trader.VolumeSpike.dll"]



#Use this for builing docker hub image  
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Trader.VolumeSpike.dll"]
#