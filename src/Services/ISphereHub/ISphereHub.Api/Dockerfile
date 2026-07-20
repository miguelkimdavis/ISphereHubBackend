FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/ISphereHub/ISphereHub.Api/ISphereHub.Api.csproj", "src/Services/ISphereHub/ISphereHub.Api/"]
COPY ["src/BuildingBlocks/ISphereHub.BuildingBlocks/ISphereHub.BuildingBlocks.csproj", "src/BuildingBlocks/ISphereHub.BuildingBlocks/"]
COPY ["src/Services/ISphereHub/ISphereHub.Application/ISphereHub.Application.csproj", "src/Services/ISphereHub/ISphereHub.Application/"]
COPY ["src/Services/ISphereHub/ISphereHub.Domain/ISphereHub.Domain.csproj", "src/Services/ISphereHub/ISphereHub.Domain/"]
COPY ["src/Services/ISphereHub/ISphereHub.Infrastructure/ISphereHub.Infrastructure.csproj", "src/Services/ISphereHub/ISphereHub.Infrastructure/"]
RUN dotnet restore "./src/Services/ISphereHub/ISphereHub.Api/ISphereHub.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/ISphereHub/ISphereHub.Api"
RUN dotnet build "./ISphereHub.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./ISphereHub.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "ISphereHub.Api.dll"]