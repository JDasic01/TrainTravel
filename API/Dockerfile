# Use the official .NET SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /API

# Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining files
COPY . ./

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 7.0.8

# Add EF Core tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet add package Npgsql --version 7.0.0
RUN dotnet add package RabbitMQ.Client --version 6.8.1
RUN dotnet add package Newtonsoft.Json --version 13.0.3
RUN dotnet add package Microsoft.EntityFrameworkCore --version 7.0.8
RUN dotnet add package Microsoft.EntityFrameworkCore.Design --version 7.0.8
RUN dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 7.0.0
RUN dotnet add package CsvHelper --version 27.1.1
RUN dotnet add package Neo4jClient --version 4.1.1

# Build the application
RUN dotnet build -c Release -o out

# Install Npgsql.EntityFrameworkCore.PostgreSQL for the tools
RUN dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 7.0.0

# Publish the application
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /API
COPY --from=build-env /API/out .

EXPOSE 8082
ENV ASPNETCORE_URLS=http://*:8082

ENTRYPOINT ["dotnet", "API.dll"]
