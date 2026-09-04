## INDIRA GANDHI NATIONAL OPEN UNIVERSITY

School of Computer and Information Sciences

Maidan Garhi, New Delhi – 110068

## PROJECT SYNOPSIS MCSP – 232

## Smart Dairy Cooperative Management System for Milk Collection and Farmer Settlement

## Study Centre: IGNOU Study Centre 1302 Regional Centre: BANGALORE Enrolment No.: 2501616219 Submitted by: NIKSON K A

## Under the Guidance of:

Teacher

IGNOU Study Centre 1302,

Room No. 006, Basement Floor, Maffei Block, Light House Hill Road, Hampankatta,

Mangaluru, Karnataka – 575003

## Academic Year: 2025–2026

## Master of Computer Applications (MCA_NEW)


## TABLE OF CONTENTS

|   | S. No Title / Description | Page No. |
| --- | --- | --- |
|   | ABSTRACT | i |
| 1 | TITLE OF THE PROJECT | 1 |
| 2 | INTRODUCTION AND OBJECTIVES OF THE PROJECT | 2 |
| 2.1 | Introduction | 2 |
| 2.2 | Objectives | 3 |
| 3 | PROJECT CATEGORY | 4 |
| 4 | TOOLS / PLATFORMS, HARDWARE AND SOFTWARE REQUIREMENTS | 5 |
| 4.1 | Technology Stack | 5 |
| 4.2 | Hardware Requirements | 6 |
| 5 | REQUIREMENTS AND ANALYSIS | 7 |
| 5.1 | Problem Definition | 7 |
| 5.2 | Functional Requirements | 8 |
| 5.3 | Non-Functional Requirements | 10 |
| 5.4 | Literature Review | 11 |
| 5.5 | Project Planning and Scheduling | 12 |
| 6 | SCOPE OF THE SOLUTION | 13 |
| 7 | ANALYSIS – DATA MODELS AND SYSTEM DIAGRAMS | 14 |
| 8 | COMPLETE DATABASE DESIGN AND TABLE STRUCTURES | 18 |
| 9 | COMPLETE STRUCTURE – MODULES, DATA STRUCTURES AND PROCESS LOGIC | 22 |
| 10 | OVERALL NETWORK ARCHITECTURE | 25 |
| 11 | IMPLEMENTATION OF SECURITY MECHANISMS | 26 |
| 12 | FUTURE SCOPE AND FURTHER ENHANCEMENTS | 27 |
| 13 | BIBLIOGRAPHY | 28 |


## ABSTRACT

Local dairy cooperative societies in India continue to manage milk collection, farmer settlements, feed deductions, and dispatch records through manual registers and paper-based calculations. This approach leads to payment inaccuracies, delayed settlements, poor record-keeping, lack of transparency between farmers and society operators, and inefficient reporting — all of which reduce operational efficiency and farmer trust.

The objective of this project is to design and develop a Smart Dairy Cooperative Management System, a centralised web-based application that digitises the complete operational workflow of a dairy cooperative society. The system automates daily milk collection recording for morning and evening shifts, calculates milk payments automatically using a fat-based rate chart, manages feed and medicine deductions, generates weekly farmer settlements, tracks milk dispatch to higher dairy unions, and provides comprehensive reports and analytics.

The proposed solution is built using ASP.NET Core MVC (.NET 8) with Razor Views for the user interface, Entity Framework Core for database operations with SQL Server as the backend, and ASP.NET Core Identity for role-based authentication. The system supports three distinct user roles: Admin (higher society / dairy union), Society Operator, and Farmer. A dedicated Farmer Portal allows farmers to log in and transparently view their own daily milk records, fat percentages, applicable milk rates, settlement summaries, and feed deduction details — eliminating the need to visit the society office for routine queries.

The system implements a configurable fat-based rate chart, input validation for all milk quality parameters, an audit trail for all data modifications, and secure role-based access control. The solution is designed for local milk societies, cooperative dairy organisations, and regional dairy unions inspired by real-world operations such as those followed by Milma (Kerala Cooperative Milk Marketing Federation).

The system significantly reduces manual workload, eliminates calculation errors, accelerates settlement generation, and improves transparency and trust between dairy societies and the farmers they serve.


## (i) Title of the Project

SMART DAIRY COOPERATIVE MANAGEMENT SYSTEM FOR MILK COLLECTION AND

FARMER SETTLEMENT USING ASP.NET CORE MVC

## (ii) Introduction and Objectives of the Project

## Introduction

The dairy cooperative sector is a vital component of India's rural economy, with millions of farmers depending on local milk societies for daily income through milk supply. Organisations such as MILMA (Kerala Cooperative Milk Marketing Federation) demonstrate the scale and impact of cooperative dairy operations — from village-level collection societies to district unions and state federations.

Despite this importance, most local dairy societies still operate using manual registers, paper-based calculations, and physical ledgers to manage milk collection records, farmer payments, feed distribution, and dispatch to higher unions. Society operators manually calculate milk amounts based on quantity and fat percentage, manually deduct feed costs from farmer payments, and generate handwritten weekly settlement sheets.

This manual approach creates several critical problems: calculation errors in payment amounts, delayed weekly settlements, difficulty in generating accurate reports, lack of centralised records, poor transparency for farmers who cannot easily verify their own milk and payment data, and inefficient tracking of feed deductions and dispatch quantities.

The proposed Smart Dairy Cooperative Management System addresses these problems through a centralised web application built on ASP.NET Core MVC. The system digitises every aspect of dairy society operations — from daily milk collection entry to weekly settlement generation and milk dispatch tracking — and provides a dedicated farmer portal for transparent access to personal records.

## Objectives

- Digitise dairy cooperative society operations to eliminate paper-based records and manual calculations.

- Automate daily milk collection management for morning and evening shifts with quantity, fat percentage, SNF, and CLR recording.

- Implement a fat-based configurable rate chart to automatically calculate milk payment amounts per collection entry.

- Calculate farmer weekly settlements automatically after applying feed, medicine, and other deductions.

- Manage feed and medicine inventory with farmer-wise distribution tracking and automatic deduction from settlements.

- Track milk dispatch from society to higher dairy unions with vehicle and destination details.

- Generate comprehensive society-level and farmer-level reports and analytics.

- Provide a dedicated farmer portal for transparent access to personal milk records, fat details, rates, settlement history, and deduction information.


- Implement role-based access control for Admin, Society Operator, and Farmer roles with full audit trail for all data changes.

## (iii) Project Category

This project falls under the following categories as defined in the MCSP-232 project guidelines:

| Category | Justification |
| --- | --- |
| RDBMS | SQL Server with normalised schema (3NF), PK/FK constraints, CHECK constraints for milk quality validation, and referential integrity across all tables. |
| OOPS | Backend in C# using classes, interfaces, Repository Pattern, Service Layer, and ASP.NET Core MVC architectural pattern with full object-oriented design. |
| Web Application Development | Full-stack ASP.NET Core MVC application with Razor Views providing server-side rendered interfaces for three distinct user roles. |
| Financial / Management System | Core domain is financial — automated milk payment calculation, weekly farmer settlements, deduction management, and payment audit trails. |
| Database Management System | Centralised SQL Server database replacing distributed manual registers; includes data dictionary, ER design, DFDs, and full normalisation. |

## Primary Category: Web Application with RDBMS and Financial Management System.

## (iv) Tools / Platform, Hardware and Software Requirement Specifications

## Technology Stack

| Layer | Technology | Version | Purpose |
| --- | --- | --- | --- |
| Frontend | Razor Views (ASP.NET MVC) | ASP.NET 8.0 | Operator, Admin, and Farmer portal UI |
| CSS Framework | Bootstrap 5 | 5.3 | Responsive layout and UI components |
| Backend | ASP.NET Core MVC | 8.0 LTS | Request handling and business logic |
| ORM | Entity Framework Core | 8.x | Database operations (code-first) |
| Database | SQL Server | 2019+ | Relational data storage |
| Authentication | ASP.NET Core Identity | 8.x | User login, password hashing, RBAC |
| Charts | Chart.js | Latest | Dashboard analytics and trend charts |
| IDE | Visual Studio 2022 | 2022 | Development environment |
| Reporting | iTextSharp / RDLC | Latest | PDF report generation |


## Hardware Requirements

| Component | Server Requirement | Client (Operator / Farmer) |
| --- | --- | --- |
| Processor | Intel Core i5 / 4 cores minimum | Any PC or smartphone with a browser |
| RAM | 8 GB minimum (16 GB recommended) | No specific requirement (browser-based) |
| Storage | 100 GB HDD/SSD | No local storage required |
| Network | Broadband internet or LAN | Wi-Fi or mobile data connectivity |
| OS | Windows Server 2019 / Windows 10+ | Any OS with modern browser |

## (v) Problem Definition, Requirements, Literature Review, and Project Planning

## Problem Definition

Most local dairy cooperative societies in India maintain all milk collection and settlement records manually. Society operators record daily milk quantity and fat percentage from each farmer in physical registers, manually calculate milk amounts using a rate chart, manually compute feed deductions, and generate handwritten weekly settlement sheets. This manual process creates multiple failure points:

- Calculation errors in milk payment amounts due to manual rate chart lookups and arithmetic.

- Delayed weekly settlements because consolidating records from daily registers is time- consuming.

- No centralised records — individual registers are difficult to search, backup, or audit.

- Poor transparency — farmers cannot independently verify their daily milk quantities, fat percentages, or deductions without visiting the society office and consulting the operator.

- Difficulty generating reports — compiling collection totals, feed usage, and dispatch summaries from manual registers takes significant effort.

- Feed and medicine deduction tracking is error-prone, leading to disputes between farmers and operators.

The proposed system solves each of these problems through a centralised digital platform accessible to all three stakeholder groups — Admin, Society Operator, and Farmer — according to their respective roles and access rights.

## Functional Requirements

## Admin / Higher Society Module

- Manage society profiles — add, edit, and view registered societies.

- Monitor consolidated milk procurement across all societies.

- View system-wide analytics and reports on a central dashboard.

- Manage system users (operators) and their access rights.


- Monitor dispatch records from society to dairy union.

## Society Operator Module

- Register and manage farmer profiles, bank details, and society associations.

- Record daily milk collection with quantity, fat %, SNF, CLR, automatic rate calculation, and quality rejection handling.

- Manage feed inventory, stock additions, farmer-wise feed distribution, stock validation, and low- stock alerts.

- Record medicine and capsule distribution with farmer-wise tracking.

- Generate weekly settlements with deduction calculations and duplicate settlement prevention.

- Record milk dispatch details including quantity, vehicle, destination, and dispatch timing.

- Generate collection receipts, society reports, and maintain complete audit tracking for all transactions.

## Farmer Portal Module

- Farmer logs in using credentials created by the society operator during registration.

- View daily milk collection history — date, shift, quantity, fat %, SNF, rate, and amount.

- View weekly settlement summaries with gross income, deductions, and net payment.

- View feed and medicine deduction details per settlement period.

- View payment history and status.

- Change login password on first login and thereafter.

## Non-Functional Requirements

- Performance: Page responses should load within 3 seconds under normal usage.

- Security: HTTPS, BCrypt password hashing, role-based access control, input validation, and secure EF Core parameterised queries.

- Data Integrity: Foreign key constraints, validation checks, and duplicate collection prevention.

- Audit Trail: All create, update, and delete actions are logged with user and timestamp details.

- Usability: Simple Bootstrap-based interface designed for easy use by society operators.

- Reliability: Settlement and payment operations use database transactions for safe processing.

- Concurrency: Optimistic concurrency control using RowVersion to prevent simultaneous update conflicts.

## Literature Review

Most dairy management systems used by organisations such as AMUL and NDDB are enterprise-level solutions not suitable for small village societies. Many local societies still depend on manual processes for milk collection and settlement management. Studies show that digital systems reduce calculation errors and improve settlement efficiency while increasing transparency for farmers. Currently, there is no affordable integrated solution covering milk collection, rate calculation, deduction management, settlement generation, and farmer portal access for small dairy societies, which this project aims to address.

## Project Planning and Scheduling

*Figure 1: Gantt Chart — 14-Week Project Schedule*


*Figure 2: PERT Chart — Critical Path Network*

## (vi) Scope of the Solution


## In Scope

- Society-level milk collection management for morning and evening shifts with shift closing and collection finalization.

- Automatic fat-based milk rate calculation using configurable rate charts.

- Farmer profile management including bank details, society mapping, and secure farmer portal access.

- Feed and medicine inventory management with farmer-wise deduction tracking.

- Automated weekly settlement generation with all applicable deductions.

- Milk dispatch tracking to dairy unions with vehicle, quantity, and reconciliation details.

- Reporting and analytics including daily collection, weekly settlement, dispatch, stock, and farmer-wise reports through the admin dashboard.

- Complete audit trail for all data changes with user ID and timestamp tracking.

## Out of Scope (Future Enhancements)

- Mobile application (Android / iOS) for farmers or operators.

- SMS or WhatsApp notification integration for settlement alerts.

- Online payment or direct bank transfer integration.

- QR code-based farmer identification at collection point.

- Multi-society cloud-based centralised management portal.

The modular ASP.NET Core MVC architecture ensures all out-of-scope features can be integrated in future iterations without redesigning the core system.


## (vii) Analysis — Data Models and System Diagrams

## ER Diagram (Entity-Relationship with Cardinality)

*Figure 3: Complete ER Diagram — PK (underlined), FK (dashed), Cardinality shown for all relationships*


## DFD Level 0 — Context Diagram

*Figure 4: DFD Level 0 — System as single process with external entities*

## DFD Level 1 — System Sub-Processes

*Figure 5: DFD Level 1 — P1 to P9 with Data Stores DS1–DS7*


## DFD Level 2 — Milk Collection and Rate Calculation (P3 and P4 Expanded)

*Figure 6: DFD Level 2 — Detail of Milk Collection Entry and Rate Calculation*

## Activity Diagram — Weekly Settlement Lifecycle

*Figure 7: Activity Diagram — Settlement from Collection to Payment*


## Class Diagram

*Figure 8: Class Diagram — Domain model, service classes, repository interfaces*

## State Diagram — Farmer Settlement Status Transitions

*Figure 9: State Diagram — Settlement status from draft to paid*


## (viii) Complete Database and Tables Detail

Database designed in Third Normal Form (3NF) using SQL Server. All tables have Primary Keys, Foreign Key references, NOT NULL constraints, CHECK constraints for domain validation, and DEFAULT values. The schema is created using Entity Framework Core code-first migrations.

*Table: Users*

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| UserID | INT IDENTITY | PK, NOT NULL | Auto-increment unique user ID |
| FullName | NVARCHAR(100) NOT NULL |   | Full name of user |
| Email |   | NVARCHAR(255) NOT NULL, UNIQUE | Login email — must be unique |
| PasswordHash | NVARCHAR(512) NOT NULL |   | BCrypt-hashed password |
| Role | NVARCHAR(20) | NOT NULL, CHECK IN ('Admin','Operator','Farmer') | Role for RBAC |
| SocietyID | INT | FK → Societies, NULL | Society this user belongs to |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Soft-delete flag |
| MustChangePassword BIT |   | NOT NULL, DEFAULT 1 | Force password change on first login |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Account creation timestamp |

## Table: Societies

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| SocietyID | INT IDENTITY | PK, NOT NULL | Unique society identifier |
| SocietyName | NVARCHAR(200) | NOT NULL | Full name of the society |
| RegistrationNo | NVARCHAR(50) | NOT NULL, UNIQUE | Official registration number |
| Address | NVARCHAR(300) | NOT NULL | Full address of society |
| ContactPhone | NVARCHAR(15) | NOT NULL | Primary contact phone number |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active/inactive flag |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Record creation timestamp |

## Table: Farmers

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| FarmerID | INT IDENTITY | PK, NOT NULL | Unique farmer identifier |
| SocietyID | INT | FK → Societies, NOT NULL | Society the farmer belongs to |


| UserID | INT | FK → Users, NOT NULL, UNIQUE | Login account for farmer portal |
| --- | --- | --- | --- |
| FarmerCode | NVARCHAR(20) | NOT NULL, UNIQUE per society | Society-assigned farmer code |
| FullName | NVARCHAR(100) | NOT NULL | Farmer full name |
| Phone | NVARCHAR(15) | NOT NULL | Mobile number |
| Address | NVARCHAR(300) | NOT NULL | Full address |
| BankAccountNo | NVARCHAR(20) | NOT NULL | Bank account for settlement |
| BankName | NVARCHAR(100) | NOT NULL | Bank name |
| IFSCCode | NVARCHAR(11) | NOT NULL | Bank IFSC code |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active farmer flag |

## Table: MilkCollections

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| CollectionID | INT IDENTITY | PK, NOT NULL | Unique collection record ID |
| FarmerID | INT | FK → Farmers, NOT NULL | Farmer who supplied milk |
| SocietyID | INT | FK → Societies, NOT NULL | Society recording the collection |
| CollectionDate | DATE | NOT NULL | Date of milk collection |
| Shift | NVARCHAR(10) | NOT NULL, CHECK IN ('Morning','Evening') | Collection shift |
| Quantity | DECIMAL(8,2) | NOT NULL, CHECK(0.5–500) Milk quantity in litres |   |
| FatPercent | DECIMAL(4,2) | NOT NULL, CHECK(2.5–9.0) Milk fat percentage |   |
| SNF | DECIMAL(4,2) | CHECK(7.5–11.0), NULL | Solid Not Fat value |
| CLR | DECIMAL(5,2) | NULL | Corrected Lactometer Reading |
| RatePerLitre | DECIMAL(8,2) | NOT NULL, CHECK(>0) | Rate looked up from MilkRates |
| Amount | DECIMAL(10,2) | NOT NULL, computed: Qty × Rate | Total payment for this entry |
| RecordedBy | INT | FK → Users, NOT NULL | Operator who recorded entry |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Entry timestamp |

## Table: MilkRates

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| RateID | INT IDENTITY | PK, NOT NULL | Unique rate entry ID |
| SocietyID | INT | FK → Societies, NOT NULL | Society this rate applies to |
| FatPercent | DECIMAL(4,2) | NOT NULL, CHECK(2.5–9.0) | Fat percentage for this row |


| RatePerLitre | DECIMAL(8,2) | NOT NULL, CHECK(>0) | Rate in INR per litre for this fat % |
| --- | --- | --- | --- |
| EffectiveFrom | DATE | NOT NULL | Date from which this rate is valid |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Current active rate flag |

## Table: FeedInventory

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| FeedItemID | INT IDENTITY | PK, NOT NULL | Unique feed item ID |
| SocietyID | INT | FK → Societies, NOT NULL | Society holding this feed stock |
| FeedName | NVARCHAR(100) | NOT NULL | Name of feed or medicine item |
| Unit | NVARCHAR(20) | NOT NULL | Unit of measurement (kg, packet) |
| PricePerUnit | DECIMAL(8,2) | NOT NULL, CHECK(>0) | Cost per unit for deduction calc |
| StockQuantity | DECIMAL(10,2) | NOT NULL, DEFAULT 0, CHECK(>=0) | Current stock on hand |

## Table: Payments (Weekly Settlements)

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| PaymentID | INT IDENTITY | PK, NOT NULL | Unique settlement record ID |
| FarmerID | INT | FK → Farmers, NOT NULL | Farmer receiving settlement |
| SocietyID | INT | FK → Societies, NOT NULL | Society generating settlement |
| PeriodStart | DATE | NOT NULL | Settlement period start date |
| PeriodEnd | DATE | NOT NULL | Settlement period end date |
| GrossAmount | DECIMAL(10,2) | NOT NULL, CHECK(>=0) | Total milk income for period |
| FeedDeduction | DECIMAL(10,2) | NOT NULL, DEFAULT 0, CHECK(>=0) | Total feed cost deducted |
| MedicineDeduction | DECIMAL(10,2) | NOT NULL, DEFAULT 0, CHECK(>=0) | Total medicine cost deducted |
| OtherDeductions | DECIMAL(10,2) | NOT NULL, DEFAULT 0 | Other miscellaneous deductions |
| NetAmount | DECIMAL(10,2) | NOT NULL, computed | Gross − all deductions |

## Table: AuditLogs

| Field Name | Data Type | Constraints | Description |
| --- | --- | --- | --- |
| LogID | BIGINT IDENTITY PK, NOT NULL |   | Auto-increment log entry ID |


| EntityType | NVARCHAR(50) | NOT NULL | Table affected e.g. MilkCollections |
| --- | --- | --- | --- |
| EntityID | INT | NOT NULL | PK of affected record |
| Action | NVARCHAR(100) | NOT NULL | e.g. Created, Updated, Deleted |
| OldValue | NVARCHAR(MAX) NULL |   | Previous state as JSON |
| NewValue | NVARCHAR(MAX) NULL |   | New state as JSON |
| PerformedBy | INT | FK → Users, NOT NULL | User who triggered action |
| Timestamp | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Exact action timestamp |

## (ix) Complete Structure — Modules, Data Structures, and Process Logic

## Number of Modules and Estimated Effort

| # | Module | Description | Effort |
| --- | --- | --- | --- |
| 1 | Authentication & RBAC | Login, password hashing, role-based access for Admin, Operator, Farmer | 1 week |
| 2 | Society Management | Add/edit society profiles, registration details, contact info | 0.5 weeks |
| 3 | Farmer Management | Register farmers, manage profiles, bank details, society association, farmer portal account creation | 1.5 weeks |
|   | 4 Milk Collection | Daily entry (morning/evening), fat-based rate lookup, amount calculation, duplicate prevention, receipt generation | 2.5 weeks |
|   | 5 Milk Rate Management | Configurable rate chart per society, fat-to-rate mapping, effective date management | 0.5 weeks |
| 6 | Feed & Medicine Mgmt | Feed stock management, farmer-wise issue tracking, deduction calculation, stock updates | 1.5 weeks |
| 7 | Weekly Settlement | Automated settlement generation, deduction adjustment, payment status tracking, settlement reports | 2 weeks |
| 8 | Dispatch Management | Dispatch quantity entry, vehicle and destination details, dispatch date/time, dispatch reports | 0.5 weeks |
| 9 | Reports & Analytics | Society and farmer reports, dashboard charts, PDF generation, fat analysis, collection trends | 1.5 weeks |
| 10 | Farmer Portal | Farmer login, personal milk records, rate view, settlement history, deduction transparency | 1 week |
| 11 | Audit Trail | AuditLog service intercepting all create/update/delete operations across all modules | 0.5 weeks |

## Process Logic of Key Modules

## Module 7: Weekly Settlement Generation

The system generates the weekly settlement by calculating the total milk collection amount for the selected period, deducting feed, medicine, and other charges, and computing the net payable amount. A


payment record is then created with status as “Generated”, and the activity is recorded in the audit log. All operations are performed within a single database transaction to ensure data consistency.

## Module 8 Extended: Dispatch Reconciliation

The system performs daily reconciliation by comparing the total milk collected with the total milk dispatched to the dairy union. It calculates the variance and, if the allowed threshold is exceeded, the operator must provide a variance reason before saving the dispatch record. The system also generates a daily reconciliation report showing collection totals, dispatch totals, variance, and variance reasons for each day.

## Additional Fields — Dispatch Table

| Field Name | Data Type | Description |
| --- | --- | --- |
| TotalCollected | DECIMAL(10,2) NOT NULL | Total milk collected that day (all shifts) |
| TotalDispatched | DECIMAL(10,2) NOT NULL | Total quantity dispatched to union |
| Variance | DECIMAL(10,2) NOT NULL, computed | TotalCollected minus TotalDispatched |
| VarianceReason | NVARCHAR(300) NULL | Mandatory when variance exceeds threshold |

## Module 2: Farmer Portal Access

The operator creates the farmer account and shares login credentials with the farmer. During the first login, the system validates credentials using ASP.NET Core Identity and forces the farmer to change the password before accessing the portal. After login, farmers can only view their own records, with all data access restricted using FarmerID-based service-layer filtering.

## Data Structures

| Structure | Type | Purpose |
| --- | --- | --- |
| MilkCollectionViewModel | C# class | { FarmerID, Date, Shift, Quantity, FatPercent, SNF, CLR, RatePerLitre, Amount } — used for collection entry form |
| RateChartEntry | C# class | { FatPercent, RatePerLitre, EffectiveFrom } — used by MilkRateService for lookup |
| SettlementDTO | C# class | { FarmerID, PeriodStart, PeriodEnd, GrossAmount, FeedDeduction, MedicineDeduction, OtherDeductions, OpeningBalance, PreviousDue, AdvancePaid, ClosingBalance, NetAmount, Status } — OpeningBalance carries forward any unpaid balance from the previous settlement cycle; ClosingBalance = NetAmount − AdvancePaid + PreviousDue. |
| FarmerPortalSummary | ViewModel | { FarmerName, TotalMilkThisMonth, AverageFat, LastSettlementAmount, PendingDeductions } |
| AuditEntry | C# class | { EntityType, EntityID, Action, OldValue (JSON), NewValue (JSON), PerformedBy, Timestamp } |


## Implementation Methodology

The project follows an iterative development approach across 6 phases: (1) Database schema design and Entity Framework Core migrations. (2) Authentication, role management, and society/farmer CRUD modules. (3) Core milk collection module with rate calculation logic and validation. (4) Feed management and settlement generation modules. (5) Reports, analytics dashboard, and farmer portal. (6) Testing (unit tests, integration tests, user acceptance testing) and documentation. Special attention is given throughout all phases to financial data consistency, transactional integrity, operational locking, reconciliation workflows, and optimistic concurrency management — ensuring the system reflects real-world dairy cooperative operational practices and is resilient to multi-user access, data correction scenarios, and settlement disputes.

## List of Reports to be Generated

| Report | Trigger | Key Metrics |
| --- | --- | --- |
| Daily Milk Collection Report | Daily / On-demand | Total quantity, total amount, farmer count, average fat%, shift-wise split |
| Weekly Collection Summary | Weekly / On-demand | Per-farmer totals for quantity, fat avg, and gross amount for the week |
| Farmer Settlement Report | On settlement generation | Gross income, all deductions itemised, net payment per farmer |
| Feed Stock Report | On-demand | Current stock levels, recent issues, farmer-wise deduction totals |
| Dispatch Report | On-demand | Dispatch date, quantity, vehicle, destination union, operator name |
| Fat Analysis Report | Monthly / On- demand | Farmer-wise average fat%, fat distribution histogram, quality trend over time |
| Top Suppliers Report | Monthly | Top 10 farmers by total quantity and by total income contributed |

## (ix) Overall Network Architecture

The system follows a standard three-tier web architecture.

- Presentation Tier: Web browsers on operator PCs, farmer smartphones, and admin workstations access the application through any modern browser. No client-side installation is required.

- Application Tier: ASP.NET Core MVC 8.0 application server hosted on IIS (Windows Server) or Kestrel. All business logic, rate calculations, settlement generation, and report assembly are performed here.

- Data Tier: SQL Server database accessible only from the application server on the internal network. No direct external access to the database is permitted.

## (x) Implementation of Security Mechanisms

| Security Layer | Mechanism Implemented | Threat Mitigated |
| --- | --- | --- |


| Transport | HTTPS / TLS enforced; HSTS headers | Man-in-the-middle, eavesdropping |
| --- | --- | --- |
| Authentication | ASP.NET Core Identity; BCrypt password hashing | Brute force, credential theft |
| Authorisation | Role-Based Access Control (RBAC) using [Authorize(Roles)] attribute | Privilege escalation — farmers cannot see other farmers' data |
| Input Validation | Data Annotations on models; server-side validation for fat% and quantity ranges | Invalid data entry, boundary violations |
| SQL Injection | Entity Framework Core parameterised queries exclusively | SQL injection attacks |
| CSRF | Anti-forgery tokens on all POST forms via ValidateAntiForgeryToken | Cross-site request forgery |
| XSS | Razor Views auto-encode all output by default | Cross-site scripting attacks |
| Audit Trail | AuditLogs table recording actor UserID + timestamp + old/new values for every data modification | Unauthorised changes, payment disputes, accountability gaps |
| Data Isolation | Farmer portal service layer filters all queries by logged-in FarmerID | Cross-farmer data access |
| Database Security | SQL Server accessible only from application server on internal network; dedicated application DB user with minimum required permissions | Direct database attacks, credential exposure |
| Session Security | HttpOnly and SameSite=Strict cookie flags on authentication cookies | Session hijacking, cookie theft |

## Settlement Locking Mechanism

To maintain financial integrity, all milk collection records associated with a generated settlement are automatically locked from further modification. Once a settlement transitions to Generated or higher status, the related MilkCollection entries become read-only (IsLocked = 1, LockedBySettlementID = <PaymentID>). Any correction after locking requires an administrator to explicitly unlock the record, and the unlock action — including the reason provided — is fully captured in the AuditLogs table. This prevents retroactive manipulation of collection data after settlements have been issued to farmers.

## Additional Fields — MilkCollections Table

| Field Name | Data Type | Description |
| --- | --- | --- |
| IsLocked | BIT NOT NULL DEFAULT 0 | Prevents edits once settlement is generated |
| LockedBySettlementID | INT NULL FK → Payments | References the settlement that locked this record |

## (xi) Future Scope and Further Enhancement

## SMS Notification on Settlement Generation


## QR Code Farmer Identification at Collection Point

The current system requires operators to manually enter or search for the farmer ID during each collection entry. Issuing each farmer a QR code card that can be scanned at the collection counter would significantly speed up the morning and evening collection queue and eliminate farmer ID entry errors.

## Mobile Application for Farmers

Rural farmers prefer smartphone-based access over desktop portals. A lightweight Flutter or React Native mobile application would allow farmers to view milk records, check settlement status, and receive push notifications directly on their phone, improving portal adoption rates in areas where desktop access is limited.

## Online Payment and Direct Bank Transfer Integration

The current system records settlement amounts but the actual bank transfer is done manually outside the system. Integrating with NPCI's IMPS/NEFT API or a payment gateway would allow the system to initiate direct bank transfers to farmers' accounts upon settlement approval, completing the end-to-end digital payment workflow.

## Multi-Society Centralised Cloud Management

The current system operates at the individual society level. Deploying the system on a cloud platform (Azure or AWS) with a multi-tenant architecture would allow dairy unions and federations to monitor all affiliated societies from a single centralised dashboard — enabling regional procurement analytics, inter- society performance benchmarking, and consolidated dispatch tracking across the union.

## Offline-First Data Entry Support

Offline-first data entry support can be implemented for societies operating in areas with unstable internet connectivity. Milk collection entries would be temporarily stored locally on the operator device using browser-based IndexedDB or a lightweight SQLite store and synchronised automatically with the central SQL Server database once connectivity is restored. Conflict resolution logic ensures that entries recorded offline do not overwrite concurrently recorded online entries, and any conflicts are flagged for administrator review before being committed.

## (xii) Bibliography

- Microsoft Corporation. (2024). ASP.NET Core MVC Documentation. https://docs.microsoft.com/en-us/aspnet/core/mvc/

- Microsoft Corporation. (2024). Entity Framework Core Documentation. https://docs.microsoft.com/en-us/ef/core/

- Microsoft Corporation. (2024). ASP.NET Core Identity. https://docs.microsoft.com/en- us/aspnet/core/security/authentication/identity

- Microsoft Corporation. (2024). SQL Server Documentation. https://docs.microsoft.com/en-us/sql/sql-server/

- MILMA — Kerala Cooperative Milk Marketing Federation. (2024). https://milma.com/

- National Dairy Development Board (NDDB). (2024). https://www.nddb.coop/


- Fowler, M. (2002). Patterns of Enterprise Application Architecture. Addison-Wesley.

- Date, C. J. (2003). An Introduction to Database Systems, 8th Ed. Addison-Wesley.

- Connolly, T. & Begg, C. (2014). Database Systems: A Practical Approach, 6th Ed. Pearson.

- OWASP Foundation. (2024). OWASP Top Ten. https://owasp.org/www-project-top-ten/

- Pressman, R. S. (2014). Software Engineering: A Practitioner's Approach, 8th Ed. McGraw-Hill.

- Bootstrap. (2024). Bootstrap 5 Documentation. https://getbootstrap.com/docs/5.3/

- Chart.js. (2024). Chart.js Documentation. https://www.chartjs.org/docs/

- GeeksForGeeks. (2024). MVC Design Pattern. https://www.geeksforgeeks.org/mvc- design-pattern/
