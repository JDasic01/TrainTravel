FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /Blazor

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet add package Newtonsoft.Json --version 13.0.3

RUN dotnet build -c Release -o out

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /Blazor
COPY --from=build-env /Blazor/out .

EXPOSE 8081
ENV ASPNETCORE_URLS=http://*:8081

ENTRYPOINT ["dotnet", "Blazor.dll"]