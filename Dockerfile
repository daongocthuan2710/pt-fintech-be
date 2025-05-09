# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Task-Management-BE.sln", "./"]
COPY ["Task-Management-BE.csproj", "./"]
RUN dotnet restore "Task-Management-BE.csproj"
COPY . .
WORKDIR /src
RUN dotnet publish "Task-Management-BE.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement_BE.dll"]
