# Use official .NET SDK 8.0 image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Use runtime image for better performance
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the application port (5000)
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "TaskManagement_BE.dll"]
