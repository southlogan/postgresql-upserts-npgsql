# PostgreSQL Upsert Patterns with Npgsql

This repo shows a few practical ways to do upserts in PostgreSQL using Npgsql.

I started with the simplest approach and then built up to more realistic patterns:

* row-by-row upserts
* bulk upserts
* enforcing business rules in the database

Each example is small and focused so you can see exactly what’s going on.

---

## Examples

### BasicUpsert

One row at a time using `ON CONFLICT DO UPDATE`.

### BulkUpsert

Uses `unnest(...)` to upsert many rows in a single query (fewer round trips, much faster for batches).

### ConditionalUpsert

Same as bulk upsert, but only updates rows when a condition is met (in this case, the name must match the existing row).

---

## Core Pattern

```sql
INSERT INTO user_preferences (id, name, food)
VALUES (@id, @name, @food)
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name,
    food = EXCLUDED.food;
```

Bulk version uses arrays + `unnest`:

```sql
INSERT INTO user_preferences (id, name, food)
SELECT *
FROM unnest(@ids, @names, @foods)
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name,
    food = EXCLUDED.food;
```

Conditional version adds a guard to the update:

```sql
INSERT INTO user_preferences (id, name, food)
SELECT *
FROM unnest(@ids, @names, @foods)
ON CONFLICT (id)
DO UPDATE SET
    food = EXCLUDED.food
WHERE user_preferences.name = EXCLUDED.name;
```

If the condition fails, the update is skipped and the existing row is left unchanged.

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

Each project is intentionally small and self-contained.

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

## Notes

* The demo resets the table on each run so results are predictable
* The bulk version reduces database round trips significantly
* The conditional version shows how to enforce rules without extra queries

---

## License

MIT
