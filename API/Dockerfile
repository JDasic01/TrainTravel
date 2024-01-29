# Use the official .NET SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /API

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining files
COPY . ./

RUN dotnet add package RabbitMQ.Client --version 6.8.1
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet add package CsvHelper --version 27.1.1
RUN dotnet add package Neo4jClient --version 4.1.0.49

# Build the application
RUN dotnet build -c Release -o out

# Publish the application
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /API
COPY --from=build-env /API/out .

EXPOSE 8082
ENV ASPNETCORE_URLS=http://*:8082

ENTRYPOINT ["dotnet", "API.dll"]
