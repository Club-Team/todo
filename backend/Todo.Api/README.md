

### Getting Started with **TodoWebApi (.NET 8 + PostgreSQL)**

This document explains how to **set up, configure, migrate, and run** the TodoWebApi project locally.

---

## 🧱 1. Prerequisites

Make sure the following are installed:

| Tool                                                                   | Required Version | Download |
| ---------------------------------------------------------------------- | ---------------- | -------- |
| [.NET SDK](https://dotnet.microsoft.com/en-us/download)                | 8.0 or higher    | ✅        |
| [PostgreSQL](https://www.postgresql.org/download/)                     | 14+              | ✅        |
| [dotnet-ef tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | Latest           | ✅        |

To check:

```bash
dotnet --version
psql --version
```

If not installed yet:

```bash
# Install EF Core CLI tool (once globally)
dotnet tool install --global dotnet-ef
```

---

## 🧩 2. Project Structure

```
TodoWebApi/
├─ HELP.md
├─ TodoWebApi.sln
└─ Todo.Api/
   ├─ Controllers/
   │   └─ TodoController.cs
   ├─ Data/
   │   └─ AppDbContext.cs
   ├─ Models/
   │   └─ TodoItem.cs
   ├─ Program.cs
   ├─ appsettings.json
   └─ Todo.Api.csproj
```

This is a **single-project Web API** that uses **Entity Framework Core (EF Core)** with a PostgreSQL database.

---

## ⚙️ 3. Database Setup

### Option A — Use local PostgreSQL installation

1. Make sure PostgreSQL is running.
2. Create a new database (example name: `todo_db`):

```bash
psql -U postgres
CREATE DATABASE todo_db;
```

3. In `Todo.Api/appsettings.json`, configure your connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=todo_db;Username=postgres;Password=your_password"
}
```

---

### Option B — Use Docker (recommended for dev)

If you prefer not to install PostgreSQL manually:

```bash
docker run --name postgres-todo -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=todo_db -p 5432:5432 -d postgres:16-alpine
```

✅ PostgreSQL will now be available at:

```
Host: localhost
Port: 5432
User: postgres
Password: postgres
Database: todo_db
```

---

## 🧮 4. Apply Migrations

If the database doesn’t exist yet or is empty, you need to apply migrations.

Run these from the project root:

```bash
# Navigate to solution root if not already there
cd TodoWebApi

# Create migration (if none exist)
dotnet ef migrations add InitialCreate -p Todo.Api -s Todo.Api

# Apply migration to the database
dotnet ef database update -p Todo.Api -s Todo.Api
```

> 💡 The app also applies migrations automatically on startup (`db.Database.Migrate()` in `Program.cs`),
> but it’s best to create the migration manually once after pulling the repo.

---

## 🚀 5. Run the Web API

From the solution root:

```bash
dotnet run --project Todo.Api
```

You’ll see output like:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

---

## 🔍 6. Test the API

Open your browser at:

👉 **[https://localhost:5001/swagger](https://localhost:5001/swagger)**

You’ll see Swagger UI with these endpoints:

| Method | Endpoint         | Description     |
| -----: | ---------------- | --------------- |
|    GET | `/api/todo`      | Get all todos   |
|    GET | `/api/todo/{id}` | Get todo by id  |
|   POST | `/api/todo`      | Create new todo |
|    PUT | `/api/todo/{id}` | Update a todo   |
| DELETE | `/api/todo/{id}` | Delete a todo   |

---

## ✳️ 7. Example Request (POST /api/todo)

**Request body:**

```json
{
  "title": "Buy groceries",
  "description": "Milk, eggs, and bread"
}
```

**Response:**

```json
{
  "id": "9e9a1a24-4e7c-4b9e-b9e8-61208b8e5f14",
  "title": "Buy groceries",
  "description": "Milk, eggs, and bread",
  "isCompleted": false,
  "createdAt": "2025-10-07T09:30:00Z",
  "updatedAt": null
}
```

---

## 🧹 8. Common Commands

| Action           | Command                                                          |
| ---------------- | ---------------------------------------------------------------- |
| Run the API      | `dotnet run --project Todo.Api`                                  |
| Build solution   | `dotnet build`                                                   |
| Create migration | `dotnet ef migrations add MigrationName -p Todo.Api -s Todo.Api` |
| Update DB        | `dotnet ef database update -p Todo.Api -s Todo.Api`              |
| List migrations  | `dotnet ef migrations list -p Todo.Api -s Todo.Api`              |
| Remove migration | `dotnet ef migrations remove -p Todo.Api -s Todo.Api`            |

---

## 🧑‍💻 9. Troubleshooting

| Problem                                 | Possible Fix                                                       |
| --------------------------------------- | ------------------------------------------------------------------ |
| ❌ “Unable to connect to database”       | Check Postgres credentials and port in `appsettings.json`          |
| ❌ “dotnet-ef not found”                 | Run `dotnet tool install --global dotnet-ef`                       |
| ❌ Migration error                       | Try deleting the `Migrations/` folder and running migrations again |
| ❌ SSL error when connecting to Postgres | Add `Trust Server Certificate=true` to your connection string      |

---

## ✅ 10. Next Steps (for later)

Once the project is running, you can expand it gradually to:

* Add DTOs and validation
* Introduce Application / Domain / Infrastructure layers (DDD)
* Add Unit Tests
* Add Docker Compose for automatic startup (API + DB)

---
