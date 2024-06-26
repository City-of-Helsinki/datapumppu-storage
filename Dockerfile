#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bookworm-slim AS base
WORKDIR /app

LABEL io.openshift.expose-services="8080:http"
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080


FROM mcr.microsoft.com/dotnet/sdk:6.0-bookworm-slim AS build
WORKDIR /src
COPY ["./Storage/Storage.csproj", "./"]
RUN dotnet restore "Storage.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "Storage/Storage.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Storage/Storage.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Storage.dll"]