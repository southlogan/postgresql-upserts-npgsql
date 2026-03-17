# PostgreSQL Upsert Demo (Npgsql)

Simple console application demonstrating how to perform **upserts** in PostgreSQL using:

* `ON CONFLICT DO UPDATE`
* Npgsql (.NET PostgreSQL driver)

The example shows how to insert new rows and update existing rows based on a primary key conflict.

---

## What This Demonstrates

* Basic usage of Npgsql with PostgreSQL
* Parameterized SQL commands
* The `INSERT ... ON CONFLICT DO UPDATE` pattern
* How PostgreSQL handles primary key conflicts

This is intentionally a **minimal, focused example**. It does not cover bulk operations, ORMs, or advanced patterns.

---

## How It Works

The core operation:

```sql
INSERT INTO user_preferences (id, name, food)
VALUES (@id, @name, @food)
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name,
    food = EXCLUDED.food;
```

Behavior:

* If `id` does **not** exist → row is inserted
* If `id` **already exists** → row is updated

---

## Running the Demo

### 1. Set your connection string

Use an environment variable:

#### PowerShell

```powershell
$env:PG_CONNECTION_STRING="Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
```

#### Bash

```bash
export PG_CONNECTION_STRING="Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
```

---

### 2. Run the app

```bash
dotnet run
```

---

## Notes

* The demo resets the table on each run to keep behavior deterministic.
* This example performs **row-by-row upserts** for clarity.
* For production workloads, consider bulk approaches (e.g., `COPY`, `unnest`).

---

## Project Structure

* `Program.cs` – main entry point and demo logic
* `UserFoodPreference` – simple data model used for inserts/updates

---

## Why This Exists

This repo is intended as a **clear, minimal reference** for:

* Understanding PostgreSQL upsert semantics
* Seeing how to execute parameterized commands with Npgsql
* Avoiding common mistakes when using `ON CONFLICT`

---

## License

Copyright 2026 Brian Bell

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: \
\
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. \
\
THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \
MIT (or your preferred license)
