#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
RUN apt-get update 
RUN apt-get install -y wget curl nano

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/ACME.API.Notifications/ACME.API.Notifications.csproj", "Services/ACME.API.Notifications/"]
RUN dotnet restore "Services/ACME.API.Notifications/ACME.API.Notifications.csproj"
COPY . .
WORKDIR "/src/Services/ACME.API.Notifications"
RUN dotnet build "ACME.API.Notifications.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ACME.API.Notifications.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ACME.API.Notifications.dll"]