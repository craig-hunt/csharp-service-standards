# One build stage feeds two runtime images: the API and the migrator that runs
# ahead of it. Both land on chiseled bases, which carry no shell and no package
# manager, so the image holds the runtime and the app and nothing an attacker
# can pivot through.

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /source

# Restore resolves from the pinned versions in Directory.Packages.props, and the
# editorconfig ships because EnforceCodeStyleInBuild reads it during compilation.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY analyzers/ analyzers/
COPY src/ src/

RUN dotnet publish src/Web/Web.csproj -c Release -o /app/web
RUN dotnet publish src/Migrator/Migrator.csproj -c Release -o /app/migrator

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS web
WORKDIR /app
COPY --from=build /app/web ./
# APP_UID arrives from the base image and names a non-root account.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["./ServiceStandards.Web"]

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS migrator
WORKDIR /app
COPY --from=build /app/migrator ./
USER $APP_UID
ENTRYPOINT ["./ServiceStandards.Migrator"]
