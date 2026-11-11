# syntax=docker/dockerfile:1

# ---------- Build stage: compiles and publishes the MVC app ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first so the restore layer is cached
COPY RaceDay.MVC.csproj ./
RUN dotnet restore RaceDay.MVC.csproj

# Then copy the rest of the source and publish
COPY . .
RUN dotnet publish RaceDay.MVC.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage: small image with only what is needed to run ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# The website listens on port 8080. ApiBaseUrl points at the RaceDay API running on the host machine.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ApiBaseUrl=http://host.docker.internal:5107/
EXPOSE 8080

COPY --from=build /app/publish .

# Run as the non-root user that ships with the .NET images
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
