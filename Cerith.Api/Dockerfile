FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 55195
EXPOSE 44378

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY Cerith.Api/Cerith.Api.csproj Cerith.Api/
RUN dotnet restore Cerith.Api/Cerith.Api.csproj
COPY . .
WORKDIR /src/Cerith.Api
RUN dotnet build Cerith.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Cerith.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Cerith.Api.dll"]
