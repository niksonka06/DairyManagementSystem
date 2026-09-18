# Database least-privilege access (MCSP-232 A5)

The web app uses **role-based access** (Admin / Operator / Farmer) in ASP.NET Core Identity. That is application authorisation, not SQL Server GRANT/REVOKE.

For the **database**, the application must connect as a dedicated login that can only read and write data — not change schema, drop tables, or act as `db_owner`.

## What to run

1. Create the database and apply EF Core migrations with a **developer/admin** SQL login (the one you use in Visual Studio).
2. Run [`sql/CreateAppLogin.sql`](sql/CreateAppLogin.sql) on SQL Server (change the password and database name first).
3. Point **Production** `ConnectionStrings:DefaultConnection` at `User Id=dairy_app`.

## Rights granted

| Right | Purpose |
|---|---|
| `SELECT`, `INSERT`, `UPDATE`, `DELETE` on `dbo` | Normal EF Core CRUD |
| No `db_owner` / `db_ddladmin` | Cannot create/drop tables or run migrations as the app |
| `DENY ALTER`, `CONTROL`, `TAKE OWNERSHIP` | Extra guard against schema change |

## What this is not

- There is no GRANT/REVOKE screen in the UI. User access is Identity roles.
- Farmers never connect to SQL Server. They use the farmer portal only.
- SMS/email credentials stay in configuration / user secrets, not in this SQL login.
