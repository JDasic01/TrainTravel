FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /Blazor
EXPOSE 8081
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Blazor.csproj", "."]
RUN dotnet restore "Blazor.csproj"
COPY . .
RUN dotnet build "Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blazor.dll"]