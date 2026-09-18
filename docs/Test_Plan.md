# Test plan — Smart Dairy Cooperative Management System (MCSP-232)

Testing follows the same order as construction: **unit → integration → system → UAT**. Results in this folder are from the frozen application after checklist A (quality rejection, shift close, reconciliation report, operator audit).

## 1. Unit tests

**Technique:** xUnit. Pure helpers plus service tests on in-memory fakes (no SQL Server).

**Coverage (B1):**

| ID | Target | Cases |
|---|---|---|
| UT-R1 | Milk rate lookup / `MilkRateService` | Latest `EffectiveFrom`, inactive ignored, no matching band |
| UT-C1 | `MilkCollectionService` | Amount = qty × rate (snapshot), duplicate farmer+date+shift, locked edit blocked, closed shift blocked |
| UT-C2 | Collection form annotations | Quantity 0.5–500, fat 2.5–9.0, SNF 7.5–11.0 |
| UT-P1 | `PaymentService` | Gross − feed − medicine − other = net; generate locks collections/issues; duplicate period; admin unlock with reason; farmer isolation |
| UT-D1 | `DispatchService` | Variance over 5% requires reason |
| UT-F1 | `FeedIssueService` | Stock decrement, never negative (insufficient stock), RowVersion mapped to business error |
| UT-I1 | Farmer isolation | Farmer A cannot load farmer B collection or settlement |
| UT-Q1 | Quality / PDF | Rejected amount 0; receipt PDF header |

**How to run:** `dotnet test DairyManagementSystem.Tests/DairyManagementSystem.Tests.csproj`

**Evidence:** [`test-evidence/dotnet-test-output.txt`](test-evidence/dotnet-test-output.txt)

## 2. Integration tests

**Technique:** `WebApplicationFactory<Program>` with EF Core InMemory, real Identity, real MVC pipeline.

| ID | Case |
|---|---|
| IT-1 | Operator cannot open Admin dashboard; farmer cannot open operator collection |
| IT-2 | Farmer receipt URL for another farmer’s collection returns 404 |
| IT-3 | Operator collection receipt returns `application/pdf` |
| IT-4 | POST login without antiforgery token → 400 |
| IT-5 | Duplicate morning collection for the same farmer returns the “already exists” message |
| IT-6 | Admin can open the union dashboard after login |

## 3. System / UAT

Manual walkthrough of the three roles. Checklist and expected results: [`UAT_Checklist.md`](UAT_Checklist.md). Fill Pass/Fail and attach screenshots in the project report Testing chapter.

## 4. Debugging and code improvement (for the report)

Record defects found while testing here as you go. Starters from this test build:

| Defect | Action |
|---|---|
| Login rate limiter (5/min/IP) would flake HTTP tests | Rate limiter skipped only when environment is `Testing` |
| Audit `SearchAsync` optional `societyId` stole the `CancellationToken` argument | Admin caller now passes `societyId: null` explicitly |
| Rejected collections still in dispatch totals would distort reconciliation | `GetTotalQuantityBySocietyAndDateAsync` excludes `IsRejected` |

Add further rows when UAT finds UI issues.
