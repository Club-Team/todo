# ========================================
# STEP 1 — Build stage
# ========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Todo.Api/*.csproj ./Todo.Api/
RUN dotnet restore Todo.Api/Todo.Api.csproj

# Copy the full source and build
COPY . .
WORKDIR /src/Todo.Api
RUN dotnet publish -c Release -o /app/publish

# ========================================
# STEP 2 — Runtime stage
# ========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose default port
EXPOSE 8080
EXPOSE 8081

# Set environment (optional but useful)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the app
ENTRYPOINT ["dotnet", "Todo.Api.dll"]
