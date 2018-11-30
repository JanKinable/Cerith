FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY Cerith.Example/Cerith.Example.csproj Cerith.Example/
RUN dotnet restore Cerith.Example/Cerith.Example.csproj
COPY . .
WORKDIR /src/Cerith.Example
RUN dotnet build Cerith.Example.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Cerith.Example.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Cerith.Example.dll"]
