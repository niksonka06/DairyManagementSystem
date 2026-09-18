-- Least-privilege SQL login for the Smart Dairy Cooperative Management System.
-- Run this on the demo/production SQL Server AFTER the database and schema exist.
-- Do not use sa or db_owner for the application connection string.

-- 1. Create a SQL login (change the password before running).
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'dairy_app')
BEGIN
    CREATE LOGIN [dairy_app] WITH PASSWORD = N'ChangeThisPassword_BeforeUse!',
        CHECK_POLICY = ON,
        CHECK_EXPIRATION = OFF;
END
GO

USE [DairyManagementSystem]; -- change if your database name differs
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'dairy_app')
BEGIN
    CREATE USER [dairy_app] FOR LOGIN [dairy_app];
END
GO

-- 2. Application needs DML on all tables, not DDL.
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [dairy_app];
GO

-- Identity/EF uses these for migrations only. The app user must NOT own schema.
-- Keep db_ddladmin / db_owner off this login. Run migrations as a DBA/dev login.

-- 3. Deny objects the app never needs.
DENY ALTER, CONTROL, TAKE OWNERSHIP ON SCHEMA::dbo TO [dairy_app];
GO

-- 4. Optional: if you add stored procedures later, grant EXECUTE on those only.
-- GRANT EXECUTE ON OBJECT::dbo.usp_Example TO [dairy_app];

-- 5. Connection string in appsettings (Production):
-- "DefaultConnection": "Server=...;Database=DairyManagementSystem;User Id=dairy_app;Password=...;TrustServerCertificate=True"
