FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ThAmCo.Auth/ThAmCo.Auth.csproj", "ThAmCo.Auth/"]
RUN dotnet restore "ThAmCo.Auth/ThAmCo.Auth.csproj"
COPY . .
WORKDIR "/src/ThAmCo.Auth"
RUN dotnet build "ThAmCo.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ThAmCo.Auth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThAmCo.Auth.dll"]