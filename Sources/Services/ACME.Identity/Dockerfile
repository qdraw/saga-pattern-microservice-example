#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
RUN apt-get update 
RUN apt-get install -y wget curl nano

EXPOSE 8080
EXPOSE 9443

# Stage 1 Front-end
# # docker buildx imagetools inspect node:18-alpine
FROM node:18-alpine as react-build
ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /Services/ACME.Identity/ClientApp
ENV CI=true
ENV INLINE_RUNTIME_CHUNK=false
ENV IMAGE_INLINE_SIZE_LIMIT=1
ENV DISABLE_ESLINT_PLUGIN=true
COPY Services/ACME.Identity/ClientApp/. ./

RUN npm install -g pnpm --no-fund
RUN npx --yes next telemetry disable
RUN pnpm install --ignore-scripts --prefer-offline
RUN pnpm run build


RUN echo "react done"


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Services/ACME.Identity/ACME.Identity.csproj", "IDS/ACME.Identity/"]
RUN dotnet restore "IDS/ACME.Identity/ACME.Identity.csproj"
COPY . .
WORKDIR "/src/Services/ACME.Identity"
RUN dotnet build "ACME.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ACME.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=react-build /Services/ACME.Identity/ClientApp/out ./ClientApp/out

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ACME.Identity.dll"]