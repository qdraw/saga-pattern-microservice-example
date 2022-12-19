ARG BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:6.0
ARG BUILD_IMAGE=mcr.microsoft.com/dotnet/sdk:6.0

#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


# Build
FROM ${BUILD_IMAGE} as build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Services/ACME.API.Registration/ACME.API.Registration.csproj", "Services/ACME.API.Registration/"]
RUN dotnet restore "Services/ACME.API.Registration/ACME.API.Registration.csproj"
COPY . .
WORKDIR "/src/Services/ACME.API.Registration"

RUN dotnet build "ACME.API.Registration.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ACME.API.Registration.csproj" -c Release -o /app/publish

# Runtime Image
FROM ${BASE_IMAGE} AS final
ENV TZ=Europe/Amsterdam

# handy tools
RUN apt-get update 
RUN apt-get install -y wget curl nano lsof

WORKDIR /app

# Copy solution website files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ACME.API.Registration.dll"]