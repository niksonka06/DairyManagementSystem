# Smart Dairy Cooperative Management System

IGNOU MCSP-232 project for milk collection and weekly farmer settlement.

| | |
|---|---|
| Course | MCA_NEW — MCSP-232 |
| Student | Nikson K A |
| Enrolment No. | 2501616219 |
| Database | `DairyManagement` (SQL Server) |
| Application | ASP.NET Core MVC (.NET 8) |

The written report is `docs/report_doc/MCSP232_Smart_Dairy_Cooperative_Management_System_Report.docx`.

## What you need

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or a newer SDK that can build `net8.0`)
- SQL Server Express **or** SQL Server LocalDB (installed with Visual Studio)
- Visual Studio 2022, or a terminal

## 1. Restore the database

The submission copy is `Database/DairyManagement.bak`. It already contains the schema and the demo data below.

From the project folder, in PowerShell (LocalDB):

```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -E -Q "RESTORE DATABASE [DairyManagement] FROM DISK = N'$PWD\Database\DairyManagement.bak' WITH MOVE N'DairyManagement' TO N'$env:USERPROFILE\DairyManagement.mdf', MOVE N'DairyManagement_log' TO N'$env:USERPROFILE\DairyManagement_log.ldf', REPLACE"
```

If SQL Server is a named instance, change `(localdb)\mssqllocaldb` to that server name (for example `localhost\SQLEXPRESS`).

## 2. Point the app at that database

The connection string is not stored in `appsettings.json`. Set it once with User Secrets, from the project folder:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=DairyManagement;Trusted_Connection=True;TrustServerCertificate=True"
```

Use the same server name you restored onto.

## 3. Run

Visual Studio: open `DairyManagementSystem.sln`, select the **https** profile, and press F5.

Or from the project folder:

```powershell
dotnet run
```

Open https://localhost:7140 (or http://localhost:5140).

On first start the app only adds anything that is missing (roles and the admin account). It does not wipe the restored data.

## Demo logins

Every account uses the password **Dairy@123**. No password change is required on first login.

| Role | Email | Scope |
|---|---|---|
| Admin | admin@dairysystem.local | Both societies |
| Operator (OPR001) | rajan.operator@dairysystem.local | St. Mary's Milk Producers Society, Ettumanoor |
| Operator (OPR002) | suresh.operator@dairysystem.local | Palai Dairy Cooperative Society |
| Farmer | anil.farmer@dairysystem.local | Ettumanoor, code F001 |
| Farmer | bindu.farmer@dairysystem.local | Ettumanoor, code F002 |
| Farmer | jose.farmer@dairysystem.local | Ettumanoor, code F003 |
| Farmer | latha.farmer@dairysystem.local | Ettumanoor, code F004 |
| Farmer | mini.farmer@dairysystem.local | Palai, code F001 |
| Farmer | prakash.farmer@dairysystem.local | Palai, code F002 |
| Farmer | reena.farmer@dairysystem.local | Palai, code F003 |
| Farmer | vinu.farmer@dairysystem.local | Palai, code F004 |

## What the demo data shows

- Two societies, eight farmers, fat/SNF/CLR rate charts, and feed and medicine stock
- Fourteen days of milk collections, daily dispatches, feed issues, and advances
- Last completed week: Anil Kumar’s settlement is **Paid**; Bindu Thomas’s settlement is **Generated** and still unpaid
- The current week’s collections are still unlocked, so a new settlement can be generated during the demo

## Rebuild the same demo data

This deletes every row in `DairyManagement` and loads the dataset above again. The schema is left in place.

```powershell
dotnet run -- --reset-submission-data
```

Stop the command when it prints `Submission data ready`. Then start the app normally with `dotnet run`.

## Tests

```powershell
dotnet test DairyManagementSystem.sln
```
