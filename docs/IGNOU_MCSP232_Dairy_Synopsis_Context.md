# IGNOU MCSP-232 — Project Synopsis Context

> **Source:** `IGNOU_MCSP232_Dairy_Synopsis_Final - model uncut.docx`  
> **Course:** MCSP-232 — Master of Computer Applications (MCA_NEW)  
> **Academic Year:** 2025–2026  
> **Purpose:** Authoritative project synopsis reference for development, documentation, and AI context.

**Related file:** For day-to-day Cursor/Claude development rules and UI priorities, also read [`Smart_Dairy_Cursor_Context.md`](./Smart_Dairy_Cursor_Context.md).

---

## Document Title

**Smart Dairy Cooperative Management System for Milk Collection and Farmer Settlement using ASP.NET Core MVC**dotne

---

## Abstract

Local dairy cooperative societies in India continue to manage milk collection, farmer settlements, feed deductions, and dispatch records through manual registers and paper-based calculations. This leads to payment inaccuracies, delayed settlements, poor record-keeping, lack of transparency between farmers and society operators, and inefficient reporting.

The objective is to design and develop a **Smart Dairy Cooperative Management System** — a centralised web application that digitises the complete operational workflow of a dairy cooperative society:

- Daily milk collection (morning and evening shifts)
- Automatic fat/SNF/CLR payment calculation
- Feed and medicine deductions
- Weekly farmer settlements
- Milk dispatch tracking to higher dairy unions
- Reports, analytics, and a farmer portal

**Stack:** ASP.NET Core MVC (.NET 8), Razor Views, Entity Framework Core, SQL Server, ASP.NET Core Identity.

**Roles:** Admin (higher society / dairy union), Society Operator, Farmer.

Inspired by real-world cooperative operations (e.g. Milma — Kerala Cooperative Milk Marketing Federation).

---

## 1. Introduction

The dairy cooperative sector is vital to India's rural economy. Despite scale at federations like MILMA, most **local societies** still use manual registers, paper calculations, and physical ledgers for:

- Milk collection records
- Farmer payments
- Feed distribution
- Dispatch to higher unions

### Problems with manual operations

- Calculation errors in payment amounts
- Delayed weekly settlements
- Difficulty generating accurate reports
- No centralised, searchable records
- Poor farmer transparency
- Error-prone feed/medicine deduction tracking

### Proposed solution

A centralised ASP.NET Core MVC web application digitising society operations from daily collection through weekly settlement and dispatch, with a dedicated farmer portal.

---

## 2. Objectives

1. Digitise dairy cooperative society operations; eliminate paper-based records and manual calculations.
2. Automate daily milk collection for morning/evening shifts (quantity, fat %, SNF, CLR).
3. Implement a configurable fat/SNF/CLR rate chart for automatic payment calculation.
4. Calculate weekly farmer settlements after feed, medicine, and other deductions.
5. Manage feed/medicine inventory with farmer-wise distribution and automatic settlement deductions.
6. Track milk dispatch to higher dairy unions (vehicle, destination, timing).
7. Generate society-level and farmer-level reports and analytics.
8. Provide a farmer portal for transparent access to milk records, rates, settlements, and deductions.
9. Implement RBAC (Admin, Operator, Farmer) with full audit trail.

---

## 3. Project Category (MCSP-232)

| Category | Justification |
|----------|---------------|
| **RDBMS** | SQL Server, 3NF, PK/FK, CHECK constraints, referential integrity |
| **OOPS** | C#, classes, interfaces, Repository + Service patterns, MVC architecture |
| **Web Application** | Full-stack ASP.NET Core MVC with Razor Views for three roles |
| **Financial / Management** | Milk payment calculation, settlements, deductions, payment audit |
| **DBMS** | Centralised DB replacing manual registers; ER, DFD, normalisation |

**Primary category:** Web Application with RDBMS and Financial Management System.

---

## 4. Technology Stack

| Layer | Technology | Version | Purpose |
|-------|----------|---------|---------|
| Frontend | Razor Views (ASP.NET MVC) | ASP.NET 8.0 | Admin, Operator, Farmer UI |
| CSS | Bootstrap | 5.3 | Responsive layout |
| Backend | ASP.NET Core MVC | 8.0 LTS | Request handling, business logic |
| ORM | Entity Framework Core | 8.x | Code-first DB operations |
| Database | SQL Server | 2019+ | Relational storage |
| Auth | ASP.NET Core Identity | 8.x | Login, password hashing, RBAC |
| Charts | Chart.js | Latest | Dashboard analytics |
| IDE | Visual Studio 2022 | 2022 | Development |
| Reporting | iTextSharp / RDLC | Latest | PDF reports |

### Hardware (summary)

| Component | Server | Client |
|-----------|--------|--------|
| Processor | Intel Core i5 / 4 cores min | Any device with browser |
| RAM | 8 GB min (16 GB recommended) | Browser-based |
| Storage | 100 GB | No local storage |
| Network | Broadband / LAN | Wi-Fi or mobile data |
| OS | Windows Server 2019 / Win 10+ | Any OS with modern browser |

---

## 5. Requirements

### 5.1 Problem definition

Manual rate lookups, arithmetic, register consolidation, and handwritten settlements cause errors, delays, disputes, and reporting difficulty. The system provides a central digital platform for Admin, Operator, and Farmer stakeholders.

### 5.2 Functional requirements

#### Admin / Higher Society

- Manage society profiles (add, edit, view)
- Monitor consolidated milk procurement across societies
- System-wide analytics and central dashboard
- Manage operators and access rights
- Monitor dispatch records to dairy union

#### Society Operator

- Register/manage farmers, bank details, society association
- Record daily milk collection: quantity, fat %, SNF, CLR, auto rate, quality rejection handling
- Manage feed inventory, stock additions, farmer-wise distribution, low-stock alerts
- Record medicine/capsule distribution
- Generate weekly settlements with deductions; prevent duplicate settlements
- Record milk dispatch (quantity, vehicle, destination, timing)
- Generate collection receipts, society reports, audit tracking

#### Farmer Portal

- Login with operator-created credentials
- View daily milk history: date, shift, quantity, fat %, SNF, CLR, rate, amount
- View weekly settlements: gross, deductions, net payment
- View feed/medicine deductions per period
- View payment history and status
- Change password on first login and thereafter

### 5.3 Non-functional requirements

| Area | Requirement |
|------|-------------|
| Performance | Page load within 3 seconds under normal usage |
| Security | HTTPS, BCrypt password hashing, RBAC, input validation, parameterised EF queries |
| Data integrity | FK constraints, validation checks, duplicate collection prevention |
| Audit trail | Log all create/update/delete with user and timestamp |
| Usability | Simple Bootstrap UI for operators |
| Reliability | DB transactions for settlement/payment operations |
| Concurrency | Optimistic concurrency via `RowVersion` |

### 5.4 Literature review (summary)

Enterprise systems (AMUL, NDDB) are not suited to small village societies. Digital systems reduce errors and improve transparency. This project targets an **affordable integrated solution** for small societies covering collection, rates, deductions, settlements, and farmer portal.

### 5.5 Project planning

- **14-week schedule** with Gantt and PERT charts (see original synopsis figures)
- **6 implementation phases:** schema → auth/CRUD → collection → feed/settlement → reports/portal → testing/docs

---

## 6. Scope

### In scope

- Society-level morning/evening milk collection with **shift closing and finalization**
- Automatic fat/SNF/CLR rate calculation (configurable rate charts)
- Farmer profiles, bank details, society mapping, secure portal access
- Feed and medicine inventory with farmer-wise deduction tracking
- Automated weekly settlement generation
- Milk dispatch tracking with vehicle, quantity, reconciliation
- Reporting and analytics (daily collection, weekly settlement, dispatch, stock, farmer-wise)
- Complete audit trail (user ID + timestamp)
- Role-based access control

### Out of scope (future enhancements)

- Mobile app (Android/iOS)
- SMS / WhatsApp notifications
- Online payment / direct bank transfer
- QR-code farmer identification at collection
- Multi-society cloud central portal
- Offline-first sync (described in future scope section)

> **Note:** The system focuses on **milk procurement and farmer settlement**, not retail milk sales to consumers. Milk type (cow vs buffalo) is not explicitly classified; quality is based on Fat %, SNF, CLR.

---

## 7. Analysis — Diagrams (reference)

The original synopsis includes these figures (not embedded here):

| Figure | Description |
|--------|-------------|
| Fig 1 | Gantt Chart — 14-week schedule |
| Fig 2 | PERT Chart — critical path |
| Fig 3 | ER Diagram — PK/FK, cardinality |
| Fig 4 | DFD Level 0 — context diagram |
| Fig 5 | DFD Level 1 — processes P1–P9, data stores DS1–DS7 |
| Fig 6 | DFD Level 2 — milk collection and rate calculation |
| Fig 7 | Activity diagram — settlement lifecycle |
| Fig 8 | Class diagram — domain, services, repositories |
| Fig 9 | State diagram — settlement status transitions |

---

## 8. Database Design

Designed in **Third Normal Form (3NF)** with PK/FK, NOT NULL, CHECK constraints, and EF Core code-first migrations.

### Table: Users

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| UserID | INT IDENTITY | PK, NOT NULL | Auto-increment user ID |
| FullName | NVARCHAR(100) | NOT NULL | Full name |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE | Login email |
| PasswordHash | NVARCHAR(512) | NOT NULL | Hashed password |
| Role | NVARCHAR(20) | NOT NULL, CHECK IN ('Admin','Operator','Farmer') | RBAC role |
| SocietyID | INT | FK → Societies, NULL | Society (null for Admin) |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Soft-delete flag |
| MustChangePassword | BIT | NOT NULL, DEFAULT 1 | Force change on first login |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Account creation |

> **Implementation note:** The codebase uses ASP.NET Identity (`ApplicationUser`, `AspNetUsers`) rather than a custom `Users` table with a `Role` column. Roles are stored in Identity's role tables.

### Table: Societies

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| SocietyID | INT IDENTITY | PK | Unique society ID |
| SocietyName | NVARCHAR(200) | NOT NULL | Society name |
| RegistrationNo | NVARCHAR(50) | NOT NULL, UNIQUE | Official registration |
| Address | NVARCHAR(300) | NOT NULL | Address |
| ContactPhone | NVARCHAR(15) | NOT NULL | Contact phone |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active flag |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |

### Table: Farmers

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| FarmerID | INT IDENTITY | PK | Unique farmer ID |
| SocietyID | INT | FK → Societies, NOT NULL | Society |
| UserID | INT | FK → Users, NOT NULL, UNIQUE | Portal login |
| FarmerCode | NVARCHAR(20) | NOT NULL, UNIQUE per society | Society farmer code |
| FullName | NVARCHAR(100) | NOT NULL | Name |
| Phone | NVARCHAR(15) | NOT NULL | Mobile |
| Address | NVARCHAR(300) | NOT NULL | Address |
| BankAccountNo | NVARCHAR(20) | NOT NULL | Settlement account |
| BankName | NVARCHAR(100) | NOT NULL | Bank name |
| IFSCCode | NVARCHAR(11) | NOT NULL | IFSC |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active flag |

### Table: MilkCollections

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| CollectionID | INT IDENTITY | PK | Collection record ID |
| FarmerID | INT | FK → Farmers, NOT NULL | Supplier |
| SocietyID | INT | FK → Societies, NOT NULL | Recording society |
| CollectionDate | DATE | NOT NULL | Collection date |
| Shift | NVARCHAR(10) | NOT NULL, CHECK IN ('Morning','Evening') | Shift |
| Quantity | DECIMAL(8,2) | NOT NULL, CHECK 0.5–500 | Litres |
| FatPercent | DECIMAL(4,2) | NOT NULL, CHECK 2.5–9.0 | Fat % |
| SNF | DECIMAL(4,2) | CHECK 7.5–11.0, NULL | Solid Not Fat |
| CLR | DECIMAL(5,2) | NULL | Corrected Lactometer Reading |
| RatePerLitre | DECIMAL(8,2) | NOT NULL, CHECK > 0 | Rate from MilkRates (snapshot) |
| Amount | DECIMAL(10,2) | NOT NULL | Qty × Rate |
| RecordedBy | INT | FK → Users, NOT NULL | Operator |
| CreatedAt | DATETIME2 | NOT NULL | Entry timestamp |
| **IsLocked** | BIT | NOT NULL, DEFAULT 0 | Locked after settlement |
| **LockedBySettlementID** | INT | NULL, FK → Payments | Settlement that locked record |

**Business rules:**

- `Amount = Quantity × RatePerLitre`
- Rate is looked up from the fat/SNF/CLR chart at entry time and **snapshotted** (never recalculated retroactively)
- Duplicate prevention: one entry per farmer + date + shift
- Locked records cannot be edited except via admin unlock (audited)

### Table: MilkRates

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| RateID | INT IDENTITY | PK | Rate entry ID |
| SocietyID | INT | FK → Societies, NOT NULL | Society |
| FatPercentFrom | DECIMAL(4,2) | NOT NULL, CHECK 2.5–9.0 | Fat % band start |
| FatPercentTo | DECIMAL(4,2) | NOT NULL, CHECK 2.5–9.0 | Fat % band end |
| SnfPercentFrom | DECIMAL(4,2) | NOT NULL, CHECK 7.5–11.0 | SNF band start |
| SnfPercentTo | DECIMAL(4,2) | NOT NULL, CHECK 7.5–11.0 | SNF band end |
| ClrFrom | DECIMAL(5,2) | NOT NULL, CHECK 0–50 | CLR band start |
| ClrTo | DECIMAL(5,2) | NOT NULL, CHECK 0–50 | CLR band end |
| RatePerLitre | DECIMAL(8,2) | NOT NULL, CHECK > 0 | INR per litre |
| EffectiveFrom | DATE | NOT NULL | Valid from date |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active flag |

### Table: FeedInventory

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| FeedItemID | INT IDENTITY | PK | Item ID |
| SocietyID | INT | FK → Societies, NOT NULL | Society |
| FeedName | NVARCHAR(100) | NOT NULL | Feed or medicine name |
| Unit | NVARCHAR(20) | NOT NULL | kg, packet, etc. |
| PricePerUnit | DECIMAL(8,2) | NOT NULL, CHECK > 0 | Cost per unit |
| StockQuantity | DECIMAL(10,2) | NOT NULL, DEFAULT 0, CHECK >= 0 | Current stock |

### Table: Payments (Weekly Settlements)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| PaymentID | INT IDENTITY | PK | Settlement ID |
| FarmerID | INT | FK → Farmers, NOT NULL | Farmer |
| SocietyID | INT | FK → Societies, NOT NULL | Society |
| PeriodStart | DATE | NOT NULL | Period start |
| PeriodEnd | DATE | NOT NULL | Period end |
| GrossAmount | DECIMAL(10,2) | NOT NULL, CHECK >= 0 | Total milk income |
| FeedDeduction | DECIMAL(10,2) | NOT NULL, DEFAULT 0 | Feed deductions |
| MedicineDeduction | DECIMAL(10,2) | NOT NULL, DEFAULT 0 | Medicine deductions |

**Additional settlement fields** (from data structures / process logic in synopsis):

| Field | Description |
|-------|-------------|
| OtherDeductions | Loan, insurance, society fee, etc. |
| OpeningBalance | Unpaid balance carried from previous cycle |
| PreviousDue | Amount owed from prior period |
| AdvancePaid | Advance payments applied |
| ClosingBalance | `NetAmount − AdvancePaid + PreviousDue` |
| NetAmount | Final payable after all adjustments |
| Status | Draft → Generated → Paid (and Cancelled) |

**Settlement formula (conceptual):**

```
Gross Milk Amount
− Feed Deduction
− Medicine Deduction
− Other Deductions
+/− Opening Balance / Previous Due / Advance adjustments
= Net Amount
```

Duplicate settlement generation must be prevented. Settlement operations use a **database transaction**.

### Table: Dispatch (with reconciliation)

| Field | Type | Description |
|-------|------|-------------|
| TotalCollected | DECIMAL(10,2) NOT NULL | Total collected that day (all shifts) |
| TotalDispatched | DECIMAL(10,2) NOT NULL | Quantity dispatched to union |
| Variance | DECIMAL(10,2) computed | TotalCollected − TotalDispatched |
| VarianceReason | NVARCHAR(300) NULL | Required when variance exceeds threshold |

Also: dispatch date/time, vehicle, destination, society, recorded by.

### Table: AuditLogs

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| LogID | BIGINT IDENTITY | PK | Log entry ID |
| EntityType | NVARCHAR(50) | NOT NULL | e.g. MilkCollections |
| EntityID | INT | NOT NULL | Affected record PK |
| Action | NVARCHAR(100) | NOT NULL | Created, Updated, Deleted |
| OldValue | NVARCHAR(MAX) | NULL | Previous state (JSON) |
| NewValue | NVARCHAR(MAX) | NULL | New state (JSON) |
| PerformedBy | INT | FK → Users, NOT NULL | Actor |
| Timestamp | DATETIME2 | NOT NULL | Action time |

---

## 9. Modules and Effort Estimate

| # | Module | Description | Effort |
|---|--------|-------------|--------|
| 1 | Authentication & RBAC | Login, hashing, Admin/Operator/Farmer roles | 1 week |
| 2 | Society Management | Society profiles, registration, contact | 0.5 weeks |
| 3 | Farmer Management | Profiles, bank details, portal account creation | 1.5 weeks |
| 4 | Milk Collection | Morning/evening entry, rate lookup, duplicates, receipts | 2.5 weeks |
| 5 | Milk Rate Management | Configurable rate chart, effective dates | 0.5 weeks |
| 6 | Feed & Medicine | Stock, farmer-wise issues, deductions | 1.5 weeks |
| 7 | Weekly Settlement | Auto generation, deductions, payment status | 2 weeks |
| 8 | Dispatch Management | Dispatch entry, vehicle, destination, reports | 0.5 weeks |
| 9 | Reports & Analytics | Dashboards, charts, PDF, fat analysis, trends | 1.5 weeks |
| 10 | Farmer Portal | Login, records, settlements, deductions | 1 week |
| 11 | Audit Trail | Log all create/update/delete across modules | 0.5 weeks |

### Key process logic

#### Weekly settlement generation (Module 7)

1. Calculate total milk collection for selected period
2. Deduct feed, medicine, and other charges
3. Compute net payable
4. Create payment record with status **Generated**
5. Log to audit trail
6. All steps in a **single database transaction**

#### Dispatch reconciliation (Module 8 extended)

1. Compare daily total collected vs total dispatched
2. Calculate variance
3. If variance exceeds threshold → operator must enter **VarianceReason**
4. Generate daily reconciliation report

#### Farmer portal access (Module 2 / 10)

1. Operator creates farmer account and shares credentials
2. First login forces password change (Identity)
3. Farmer sees **only own records** — service layer filters by `FarmerID`

---

## 10. Data Structures (C# / ViewModels)

| Structure | Purpose |
|-----------|---------|
| **MilkCollectionViewModel** | `{ FarmerID, Date, Shift, Quantity, FatPercent, SNF, CLR, RatePerLitre, Amount }` |
| **RateChartEntry** | `{ FatPercentFrom, FatPercentTo, SnfPercentFrom, SnfPercentTo, ClrFrom, ClrTo, RatePerLitre, EffectiveFrom }` |
| **SettlementDTO** | `{ FarmerID, PeriodStart, PeriodEnd, GrossAmount, FeedDeduction, MedicineDeduction, OtherDeductions, OpeningBalance, PreviousDue, AdvancePaid, ClosingBalance, NetAmount, Status }` |
| **FarmerPortalSummary** | `{ FarmerName, TotalMilkThisMonth, AverageFat, LastSettlementAmount, PendingDeductions }` |
| **AuditEntry** | `{ EntityType, EntityID, Action, OldValue, NewValue, PerformedBy, Timestamp }` |
| **CollectionReport** | `{ SocietyID, Date, TotalQuantity, TotalAmount, FarmerCount, AverageFat }` |

---

## 11. Reports

| Report | Trigger | Key metrics |
|--------|---------|-------------|
| Daily Milk Collection | Daily / on-demand | Total qty, amount, farmer count, avg fat %, shift split |
| Weekly Collection Summary | Weekly / on-demand | Per-farmer qty, fat avg, gross amount |
| Farmer Settlement | On settlement generation | Gross, itemised deductions, net per farmer |
| Feed Stock | On-demand | Stock levels, recent issues, farmer deductions |
| Dispatch | On-demand | Date, qty, vehicle, destination, operator |
| Fat Analysis | Monthly / on-demand | Avg fat %, distribution, quality trend |
| Top Suppliers | Monthly | Top 10 by quantity and by income |

---

## 12. Network Architecture

Three-tier web architecture:

| Tier | Description |
|------|-------------|
| **Presentation** | Browsers on operator PCs, farmer phones, admin workstations — no client install |
| **Application** | ASP.NET Core MVC 8 on IIS or Kestrel — business logic, calculations, reports |
| **Data** | SQL Server — internal network only, no direct external DB access |

---

## 13. Security Mechanisms

| Layer | Mechanism | Threat mitigated |
|-------|-----------|------------------|
| Transport | HTTPS / TLS, HSTS | MITM, eavesdropping |
| Authentication | ASP.NET Core Identity, password hashing | Brute force, credential theft |
| Authorisation | RBAC via `[Authorize(Roles)]` | Privilege escalation |
| Input validation | Data annotations, server-side range checks | Invalid data |
| SQL injection | EF Core parameterised queries | SQL injection |
| CSRF | Anti-forgery tokens on POST | CSRF |
| XSS | Razor auto-encoding | XSS |
| Audit trail | AuditLogs with actor + old/new JSON | Disputes, accountability |
| Data isolation | Farmer queries filtered by logged-in FarmerID | Cross-farmer access |
| DB security | App server only, least-privilege DB user | Direct DB attacks |
| Session | HttpOnly, SameSite=Strict cookies | Session hijacking |

### Settlement locking

When a settlement reaches **Generated** (or higher):

- Related `MilkCollection` rows set `IsLocked = 1`, `LockedBySettlementID = PaymentID`
- Locked records are read-only
- Corrections require **admin unlock** with reason, fully audited in `AuditLogs`

---

## 14. Future Scope (from synopsis)

1. **SMS on settlement generation** — notify farmer when settlement is ready
2. **QR code farmer ID** — faster collection queue, fewer ID errors
3. **Mobile app for farmers** — Flutter/React Native for records and notifications
4. **Online payment / NEFT** — direct bank transfer on settlement approval
5. **Multi-society cloud portal** — union/federation central dashboard
6. **Offline-first entry** — local store + sync when connectivity returns

---

## 15. Bibliography (from synopsis)

- Microsoft — ASP.NET Core MVC, EF Core, Identity, SQL Server documentation
- MILMA — https://milma.com/
- NDDB — https://www.nddb.coop/
- Fowler — *Patterns of Enterprise Application Architecture*
- Date — *An Introduction to Database Systems*
- Connolly & Begg — *Database Systems: A Practical Approach*
- OWASP Top Ten
- Pressman — *Software Engineering: A Practitioner's Approach*
- Bootstrap 5, Chart.js documentation

---

## 16. Core Workflow (one line)

**Farmer → Local Dairy Society → Higher Dairy Union**

Digitise collection and quality, auto-calculate fat/SNF/CLR payments, manage deductions, generate weekly settlements, track dispatch, and give farmers transparent access to their own records.

---

## 17. Using this file in development

1. Treat this document as the **academic / requirements source of truth** from the IGNOU synopsis.
2. Use [`Smart_Dairy_Cursor_Context.md`](./Smart_Dairy_Cursor_Context.md) for **concise AI coding rules**, UI/UX direction, and "do not invent" boundaries.
3. When the codebase and synopsis differ, **the implemented code is the runtime truth** — update this doc or the cursor context if requirements change deliberately.
4. Do not add out-of-scope features (mobile app, QR, online payments, etc.) unless explicitly requested.
