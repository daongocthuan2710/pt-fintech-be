# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY ["Task-Management-BE.sln", "./"]
COPY ["Task-Management-BE.csproj", "./"]
RUN dotnet restore "Task-Management-BE.csproj"
COPY . .
RUN dotnet publish "Task-Management-BE.csproj" -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TaskManagement_BE.dll"]
