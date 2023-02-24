ARG IMG_NAME=7.0-alpine

# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim-amd64 AS build-env
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ./src ./
RUN dotnet restore

# Copy everything else and build
WORKDIR /src/ComaxRpUI
RUN dotnet publish -c Release -o ../out
WORKDIR /src/out

# Build runtime image
FROM vertechcon/comax-runtime:${IMG_NAME}
WORKDIR /app
COPY --from=build-env src/out .

EXPOSE 80

RUN chown 1000: ./
RUN chmod -R u+x ./
USER 1000
ENTRYPOINT ["dotnet", "/app/ComaxRpUI.dll"]
