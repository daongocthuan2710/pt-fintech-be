# Task Management Backend

## 🚀 Getting Started

### Prerequisites

- Install .NET SDK 8.0: [Download .NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Install PostgreSQL: [Download PostgreSQL](https://www.postgresql.org/download/)
- Install Git: [Download Git](https://git-scm.com/downloads)

### ⚡ Clone the Repository

```bash
git clone <Your-GitHub-Repository-URL>
cd Task-Management-BE
```

### 📌 Configuration

Create a file appsettings.json in the root directory:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=task_management_db;Username=your_db_user;Password=your_db_password"
  },
  "JwtSettings": {
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "SecretKey": "YourSecretKey"
  }
}
```

### 🚀 Run PostgreSQL

1. Start PostgreSQL server.

2. Create a database named `task_management_db`.

### ⚡ Run the Application

```bash
# Restore dependencies

dotnet restore

# Apply migrations (Ensure PostgreSQL is running)

dotnet ef database update

# Run the application

dotnet run
```

### 🌐 Accessing the Application

- The backend will be available at: http://localhost:5000

- Swagger API documentation will be available at: http://localhost:5000/swagger

### 📌 Troubleshooting

If you encounter any issues, please make sure:

- PostgreSQL is running and the database is created.

- Your connection string in appsettings.json is correct.

- You have .NET SDK 8.0 installed and properly configured.

### 📌 Useful Commands

- Run the project:

```bash
dotnet run
```

- Apply database migrations:

```bash
dotnet ef database update
```
