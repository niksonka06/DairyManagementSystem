# Smart Dairy Cooperative Management System --- Cursor Context

> **Full synopsis reference:** See [`IGNOU_MCSP232_Dairy_Synopsis_Context.md`](./IGNOU_MCSP232_Dairy_Synopsis_Context.md) for the complete IGNOU MCSP-232 project synopsis (requirements, database tables, modules, security). This file is the **compact** development context for Cursor/Claude.

## 1. Project Identity

**Project:** Smart Dairy Cooperative Management System for Milk
Collection and Farmer Settlement\
**Full title:** Smart Dairy Cooperative Management System for Milk
Collection and Farmer Settlement using ASP.NET Core MVC

This is a web-based dairy cooperative management system for local milk
societies. Its main purpose is to replace manual registers and
calculations with a centralized system for milk collection, farmer
payment calculation, deductions, weekly settlements, dispatch tracking,
reporting, and farmer transparency.

**Core workflow:**

`Farmer → Local Dairy Society → Higher Dairy Union`

The system is focused on **milk procurement/collection and farmer
settlement**, not retail milk sales.

------------------------------------------------------------------------

## 2. Main Problem

Local dairy societies may manually record: - Farmer milk quantity - Fat
percentage - Milk rates - Farmer payments - Feed/medicine deductions -
Weekly settlements - Milk dispatch

Manual work can cause calculation errors, delayed settlements, poor
searching/auditing, lack of farmer transparency, and reporting
difficulties.

The system centralizes these operations.

------------------------------------------------------------------------

## 3. Technology Stack

-   ASP.NET Core MVC / .NET 8
-   C#
-   Razor Views
-   Bootstrap 5.3
-   Entity Framework Core 8
-   SQL Server 2019+
-   ASP.NET Core Identity
-   Role-Based Access Control (RBAC)
-   Chart.js
-   PDF reporting (iTextSharp / RDLC as applicable)
-   Visual Studio 2022

------------------------------------------------------------------------

## 4. User Roles

### Admin / Higher Society

-   Manage societies
-   Manage operators/users and access
-   Monitor consolidated milk procurement
-   Monitor dispatch
-   View system-wide analytics/reports

### Society Operator

-   Manage farmers
-   Record milk collection
-   Manage milk rates
-   Manage feed/medicine inventory
-   Issue feed/medicine to farmers
-   Generate weekly settlements
-   Record milk dispatch
-   Generate receipts/reports
-   View audit information

### Farmer

-   Login to own portal
-   View own milk collection history
-   View quantity, shift, fat, SNF, CLR, rate and amount
-   View weekly settlements
-   View gross amount, deductions and net payment
-   View feed/medicine deductions
-   View payment history/status
-   Change password

Farmers must only access their own records.

------------------------------------------------------------------------

## 5. Core Modules

1.  Authentication & RBAC
2.  Society Management
3.  Farmer Management
4.  Milk Collection
5.  Milk Rate Management
6.  Feed & Medicine Management
7.  Weekly Settlement
8.  Milk Dispatch & Reconciliation
9.  Reports & Analytics
10. Farmer Portal
11. Audit Trail

------------------------------------------------------------------------

## 6. Milk Collection

Collection is recorded for **morning and evening shifts**.

Typical collection data: - Farmer - Date - Shift - Quantity (litres) -
Fat % - SNF - CLR - Rate per litre - Amount - Quality/rejection status
where supported

### Payment calculation

The rate is obtained from the configurable fat/SNF/CLR rate chart.

Basic calculation:

`Amount = Quantity × RatePerLitre`

Example:

`5 litres × ₹40/litre = ₹200`

The system should automatically calculate the rate and amount rather
than relying on manual arithmetic.

Duplicate collection entries should be prevented.

Collection receipts can be generated.

------------------------------------------------------------------------

## 7. Milk Rate Management

Each society can have a configurable rate chart.

Concept:

`Fat % + SNF + CLR → Rate per litre`

Rate entries contain: - Fat from–to - SNF from–to - CLR from–to - Rate
per litre - Effective date

The matching band on the latest effective chart is used when calculating
milk payment.

------------------------------------------------------------------------

## 8. Farmer Management

Farmer records include relevant: - Profile information - Contact
information - Bank details - Society association - Portal/login account

The operator creates the farmer account.

Farmer portal access is restricted by FarmerID/service-layer filtering
so a farmer cannot see another farmer's information.

------------------------------------------------------------------------

## 9. Feed & Medicine

The society manages: - Feed stock - Stock additions - Farmer-wise feed
distribution - Medicine/capsule distribution - Quantity - Cost -
Deduction information - Low-stock alerts where supported

Feed and medicine costs can become deductions in the farmer settlement.

------------------------------------------------------------------------

## 10. Weekly Settlement

For a selected settlement period, the system calculates the farmer's
financial result.

Concept:

`Gross Milk Amount` `- Feed Deduction` `- Medicine Deduction`
`- Other Deductions`
`+/- Applicable Previous Balance/Advance adjustments` `= Net Amount`

The project data structure supports fields such as: - FarmerID -
PeriodStart - PeriodEnd - GrossAmount - FeedDeduction -
MedicineDeduction - OtherDeductions - OpeningBalance - PreviousDue -
AdvancePaid - ClosingBalance - NetAmount - Status

Duplicate settlement generation must be prevented.

Settlement/payment operations should use a database transaction for
consistency.

Typical status can include `Generated` and payment status as implemented
by the application.

------------------------------------------------------------------------

## 11. Milk Dispatch

The society records milk dispatched to the higher dairy union.

Typical data: - Date/time - Quantity - Vehicle - Destination - Total
collected - Total dispatched - Variance - Variance reason

### Reconciliation

`Variance = TotalCollected - TotalDispatched`

If variance exceeds the configured threshold, a variance reason is
required.

The system can produce a daily reconciliation report showing: -
Collection total - Dispatch total - Variance - Variance reason

------------------------------------------------------------------------

## 12. Reports & Analytics

Important reports: - Daily milk collection - Weekly collection -
Farmer-wise collection - Farmer settlement - Payment history - Feed
stock - Feed/medicine distribution - Milk dispatch - Dispatch
reconciliation - Fat/quality analysis - Collection trends - Top
suppliers - Society-level reports

Dashboard analytics can use Chart.js.

------------------------------------------------------------------------

## 13. Audit Trail

All important create/update/delete actions should be auditable.

Audit information: - EntityType - EntityID - Action - OldValue (JSON
where used) - NewValue (JSON where used) - PerformedBy - Timestamp

Typical actions: - Created - Updated - Deleted

Do not remove or bypass audit logging when changing UI functionality.

------------------------------------------------------------------------

## 14. Important Data/ViewModel Concepts

### MilkCollectionViewModel

-   FarmerID
-   Date
-   Shift
-   Quantity
-   FatPercent
-   SNF
-   CLR
-   RatePerLitre
-   Amount

### RateChartEntry

-   FatPercent
-   RatePerLitre
-   EffectiveFrom

### SettlementDTO

-   FarmerID
-   PeriodStart
-   PeriodEnd
-   GrossAmount
-   FeedDeduction
-   MedicineDeduction
-   OtherDeductions
-   OpeningBalance
-   PreviousDue
-   AdvancePaid
-   ClosingBalance
-   NetAmount
-   Status

### FarmerPortalSummary

-   FarmerName
-   TotalMilkThisMonth
-   AverageFat
-   LastSettlementAmount
-   PendingDeductions

### AuditEntry

-   EntityType
-   EntityID
-   Action
-   OldValue
-   NewValue
-   PerformedBy
-   Timestamp

### CollectionReport

-   SocietyID
-   Date
-   TotalQuantity
-   TotalAmount
-   FarmerCount
-   AverageFat

------------------------------------------------------------------------

## 15. Scope --- IN SCOPE

-   Society-level morning/evening milk collection
-   Shift closing/finalization
-   Fat/SNF/CLR configurable milk rates
-   Farmer management
-   Farmer bank details
-   Farmer portal
-   Feed and medicine inventory
-   Farmer-wise deductions
-   Weekly settlements
-   Milk dispatch to higher dairy union
-   Dispatch reconciliation
-   Reports and analytics
-   Audit trail
-   Role-based access

------------------------------------------------------------------------

## 16. OUT OF SCOPE / DO NOT INVENT

These are future enhancements, not current core functionality: -
Android/iOS mobile app - SMS/WhatsApp notifications - Online
payment/direct bank transfer integration - QR-code farmer
identification - Multi-society cloud central portal

Also, the current project does **not explicitly implement direct local
retail milk sales to consumers**.

The current project does **not explicitly classify milk as cow milk vs
buffalo milk**. Milk collection is based on quantity and quality
parameters such as Fat %, SNF and CLR.

Do not add these as existing functionality unless the actual code
already contains them.

------------------------------------------------------------------------

## 17. UI/UX Direction

The application should look like a real commercial dairy ERP, not a
student project.

Design goals: - Clean - Professional - Modern - Practical - Responsive -
Easy for society operators - Simple for farmers - Financially
trustworthy

Preferred visual language: - Dairy-inspired green accent - White/light
neutral backgrounds - Dark readable text - Subtle borders - Soft
shadows - Moderate rounded corners - Consistent typography - Consistent
icon library

Avoid: - Excessive gradients - Excessive animations - Cartoon-style cow
graphics - Too many colors - Huge dashboard cards - Generic/default
Bootstrap appearance

------------------------------------------------------------------------

## 18. UI Layout

Desktop: - Left collapsible sidebar - Top navbar - Breadcrumb/page
heading - Main content area

Operator UI should prioritize speed and minimum clicks.

Farmer UI should be simpler and mobile-friendly.

Major screens: - Login - Admin Dashboard - Operator Dashboard - Farmer
Dashboard - Farmer List/Create/Edit/View - Milk Collection - Collection
History - Milk Rate Chart - Feed Inventory - Medicine/Feed
Distribution - Weekly Settlement - Settlement Details - Payment
History - Milk Dispatch - Dispatch Reconciliation - Reports -
Analytics - Audit Logs - Profile - Settings

------------------------------------------------------------------------

## 19. Milk Collection UI Priority

This is one of the most important operational screens.

Operator should be able to: 1. Select farmer 2. Select date/shift 3.
Enter quantity 4. Enter Fat/SNF/CLR 5. Automatically see rate 6.
Automatically see amount 7. Save collection 8. Immediately see today's
collection entries

Show calculation clearly:

`Quantity × Rate/Litre = Amount`

Provide: - Search - Date filter - Shift filter - Farmer filter -
Validation - Duplicate prevention feedback - Success/error
notification - Compact table

------------------------------------------------------------------------

## 20. Settlement UI Priority

Make financial calculations extremely clear.

Example:

Gross Milk Amount: ₹8,500 Feed Deduction: -₹500 Medicine Deduction:
-₹150 Other Deductions: -₹100 Net Payable: ₹7,750

The Net Payable amount should be visually prominent.

------------------------------------------------------------------------

## 21. Security & Data Integrity

Required concepts: - ASP.NET Core Identity - Role-based authorization -
HTTPS - BCrypt password hashing - Input validation - Parameterized EF
Core queries - Foreign keys - Validation/check constraints - Duplicate
collection prevention - Audit logging - Database transactions for
settlement/payment operations - Optimistic concurrency/RowVersion where
implemented

Never expose another farmer's private records.

Never expose raw exceptions to normal users.

------------------------------------------------------------------------

## 22. Development Rules for Cursor/Claude

This file is a compact project context reference. **Read this instead of
repeatedly reading the full synopsis.**

When modifying the existing project:

1.  Inspect existing code before changing it.
2.  Reuse existing Controllers, Models, ViewModels, Services and routes.
3.  Do not invent business rules that are not supported by the project.
4.  Do not unnecessarily change database schema.
5.  Do not break authentication/RBAC.
6.  Do not break existing calculations.
7.  Do not remove existing functionality.
8.  Keep Razor Views maintainable.
9.  Prefer reusable partials/components/CSS classes.
10. Avoid duplicated styling.
11. After changes, verify build and existing workflows.
12. Keep the UI consistent across Admin, Operator and Farmer areas.

### Core principle

**Improve the UI and UX around the existing system; do not redesign the
business logic unless explicitly requested.**

------------------------------------------------------------------------

## 23. One-Line Project Summary

**A web-based dairy cooperative ERP that records farmers' milk
collection and quality, automatically calculates milk payments using
fat, SNF and CLR rate bands, manages feed/medicine deductions, generates weekly
settlements, tracks dispatch to the higher dairy union, and gives
farmers transparent access to their own records.**
