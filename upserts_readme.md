# PostgreSQL Upsert Patterns with Npgsql

This repository demonstrates multiple ways to perform **upserts** in PostgreSQL using Npgsql in .NET.

The goal is to show how the same logical operation—"insert or update"—can be implemented with increasing sophistication depending on performance and business rules.

---

## What This Repo Covers

Three progressively more advanced patterns:

### 1. Basic Upsert (Row-by-Row)
- Uses `INSERT ... ON CONFLICT DO UPDATE`
- Executes one command per row
- Easiest to understand

**Use case:** simple applications, low volume

---

### 2. Bulk Upsert
- Uses `unnest(...)` with array parameters
- Executes a single command for many rows
- Much more efficient (fewer round trips)

**Use case:** batch processing, higher throughput scenarios

---

### 3. Conditional Upsert (Business Rule Enforcement)
- Uses `ON CONFLICT ... DO UPDATE ... WHERE`
- Prevents updates when certain conditions are not met
- Demonstrates enforcing rules at the database level

**Example rule:**
> Do not allow a user's name to change once created

---

## Core SQL Patterns

### Basic Upsert

```sql
INSERT INTO user_preferences (id, name, food)
VALUES (@id, @name, @food)
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name,
    food = EXCLUDED.food;
```

---

### Bulk Upsert

```sql
INSERT INTO user_preferences (id, name, food)
SELECT *
FROM unnest(@ids, @names, @foods)
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name,
    food = EXCLUDED.food;
```

---

### Conditional Upsert

```sql
INSERT INTO user_preferences (id, name, food)
SELECT *
FROM unnest(@ids, @names, @foods)
ON CONFLICT (id)
DO UPDATE SET
    food = EXCLUDED.food
WHERE user_preferences.name = EXCLUDED.name;
```

If the condition fails, the update is skipped and the existing row remains unchanged.

---

## Project Structure

```
Upserts.sln

/Shared
  UserFoodPreference.cs
  DemoTableHelpers.cs

/BasicUpsert
  Program.cs

/BulkUpsert
  Program.cs

/ConditionalUpsert
  Program.cs
```

Each project is intentionally small and self-contained so the pattern is easy to understand.

---

## Running the Examples

### 1. Set your connection string

PowerShell:

```powershell
$env:PG_CONNECTION_STRING="Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
```

Bash:

```bash
export PG_CONNECTION_STRING="Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
```

---

### 2. Run a project

```bash
dotnet run --project BasicUpsert
```

or

```bash
dotnet run --project BulkUpsert
```

or

```bash
dotnet run --project ConditionalUpsert
```

---

## Key Takeaways

- `ON CONFLICT` is the core PostgreSQL mechanism for upserts
- `EXCLUDED` provides access to incoming values during updates
- `unnest` enables efficient bulk operations
- `WHERE` inside `DO UPDATE` allows business rule enforcement

---

## Why This Matters

Upserts are a common requirement in real systems. Understanding these patterns helps you:

- avoid race conditions
- reduce database round trips
- enforce data integrity at the database level

---

## License

MIT

