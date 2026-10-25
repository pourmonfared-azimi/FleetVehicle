# syntax=docker/dockerfile:1
# Gateway image. Build context: repository root.
#
# The build is also the composition check: it exports every subgraph's source schema,
# composes the supergraph with the Nitro CLI and fails if composition fails.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY .config/ .config/
COPY src/ src/
RUN dotnet tool restore

# 1. Export the source schemas (schema.graphqls + schema-settings.json per subgraph).
RUN set -e; \
    for s in Vehicles Service Parts; do \
      mkdir -p /schemas/$s; \
      cp src/$s/FleetGraph.$s.Api/schema-settings.json /schemas/$s/schema-settings.json; \
      dotnet run --project src/$s/FleetGraph.$s.Api -c Release --no-launch-profile -- \
        schema export --output /schemas/$s/schema.graphqls; \
    done

# 2. Compose the supergraph for the "Docker" environment (subgraph URLs = compose service names).
RUN dotnet nitro fusion compose \
      --source-schema-file /schemas/Vehicles/schema.graphqls \
      --source-schema-file /schemas/Service/schema.graphqls \
      --source-schema-file /schemas/Parts/schema.graphqls \
      --archive /schemas/gateway.far \
      --environment Docker \
 && dotnet nitro fusion settings set default-list-size 25 \
      --archive /schemas/gateway.far \
      --environment Docker

# 3. Publish the gateway.
RUN dotnet publish src/Gateway/FleetGraph.Gateway/FleetGraph.Gateway.csproj \
      -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ENV ASPNETCORE_HTTP_PORTS=8080
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /schemas/gateway.far ./gateway.far
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "FleetGraph.Gateway.dll"]
