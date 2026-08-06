# Stage 1: Build CSS with Node.js
FROM node:20 AS tailwind-build
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm install
COPY wwwroot/css/site.css ./wwwroot/css/
COPY tailwind.config.js ./
# Depending on what tailwind scans, we might need the Views too, but let's copy everything just in case
COPY . .
RUN npm run css:build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskMonitoringApp.csproj", "."]
RUN dotnet restore "./TaskMonitoringApp.csproj"
COPY . .
# Copy the compiled CSS from the tailwind-build stage
COPY --from=tailwind-build /src/wwwroot/css/styles.css ./wwwroot/css/styles.css
RUN dotnet build "./TaskMonitoringApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish .NET application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskMonitoringApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskMonitoringApp.dll"]
