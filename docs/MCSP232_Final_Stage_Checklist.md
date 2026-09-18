# MCSP-232 Final Stage Checklist

Use this file to finish the Smart Dairy Cooperative Management System and then write the project report. Tick items as you complete them. Do **not** add out-of-scope features (mobile app, QR IDs, NEFT, WhatsApp, multi-tenant cloud, offline-first).

**Project title (keep this exact wording everywhere):**  
Smart Dairy Cooperative Management System for Milk Collection and Farmer Settlement using ASP.NET Core MVC

---

## Where you stand

| Area | Status |
|---|---|
| 11 synopsis modules (auth, society, farmer, collection, rates, feed/medicine, settlement, dispatch, reports, farmer portal, audit) | Built |
| 7 promised reports + PDF + Chart.js | Built |
| Security (Identity + BCrypt, RBAC, CSRF, HSTS, cookies, CSP, rate limit, RowVersion, locking) | Built |
| Phase 6 testing (unit / integration / UAT with evidence) | Thin — main software gap |
| Project report (~100–125 pages, excluding code) | Not started — main academic gap |
| Submission pack (bound report, CD, certificates, screenshots) | Not started |

---

## How to use this list

1. Freeze new features except items in **A**.
2. Finish **A** (or explicitly drop them from “implemented” wording).
3. Finish **B** (tests + evidence).
4. Product freeze. Walk **B.3** with screenshots.
5. Write **C** (report) from the frozen system.
6. Assemble **D** (print, CD, viva pack).

Suggested order is also listed at the bottom.

---

## A. Software still to finish

Decide for A1 and A2: **implement** them, or **drop them from the report’s implemented list**. Do not claim them in viva if they are not in the product.

### A1. Quality rejection handling

- [x] Synopsis: operator can record collection with quality rejection.
- [x] Today: no rejected/accepted status; unmatched rate bands only block save.
- [x] If implementing: add a clear reject path (reason, no payment, still visible to farmer).
- [ ] If skipping: never list this as implemented in the report.

### A2. Shift close / collection finalization

- [x] In-scope in synopsis: morning/evening close so the shift cannot be edited before settlement.
- [x] Today: locking only happens when a settlement is **Generated**.
- [x] If implementing: add shift-close (or equivalent finalization).
- [ ] If skipping: document settlement-lock as the substitute and **do not** claim shift-close.

### A3. Daily dispatch reconciliation report

- [x] Dispatch already has variance + required reason over threshold.
- [x] Synopsis also wants a daily reconciliation layout: collected vs dispatched vs variance vs reason.
- [x] Either add that report **or** make the existing Dispatch report show those columns clearly.
- [ ] Screenshot it and use that screenshot as the reconciliation report in the project report.

### A4. Operator-visible audit (read-only)

- [x] Synopsis: operators maintain complete audit tracking.
- [x] Today: Audit UI is Admin-only.
- [x] Either add a society-scoped read-only audit list for operators.
- [ ] Or state in the report that audit viewing is admin-only by design.

### A5. SQL GRANT/REVOKE / least-privilege DB user

- [x] Guidelines ask for access rights in the coding chapter.
- [x] App RBAC already exists; no GRANT/REVOKE UI is needed.
- [x] Create/document a least-privilege SQL login for the app (not `db_owner`).
- [ ] Apply it on the demo database and describe it in the report.

### Nice to have (not required for freeze)

- [ ] Dedicated medicine module UI — **skip if** the report says medicine is inventory `ItemType`.
- [ ] Real SMS on — **leave disabled**; treat SMS as future scope.
- [ ] Proof that pages load under 3 seconds (timed screenshots or a short note).

---

## B. Testing still to build

Today: helper unit tests only (rate lookup, settlement formula, dispatch variance, BCrypt hasher, PDF smoke, dates, DB exception mapping, a few UAT formula stand-ins).  
Missing: service tests, integration tests, recorded system-test results.

### B1. Unit tests (expand)

Cover calculators **and** services (in-memory DB or mocked repositories).

- [x] `MilkRateService` / lookup: band match, latest `EffectiveFrom`, inactive ignored, no-match / reject
- [x] `MilkCollectionService`: amount = qty × rate, snapshot rate, duplicate farmer+date+shift, locked cannot edit
- [x] `PaymentService`: gross − feed − medicine − other ± balances; duplicate period; transaction; lock collections/issues; cancel/unlock with reason
- [x] `DispatchService`: variance %; reason required over threshold
- [x] `FeedIssueService`: stock decrement; never negative; RowVersion conflict
- [x] Farmer isolation: farmer A cannot load farmer B’s collection/settlement
- [x] Validation: quantity 0.5–500, fat 2.5–9.0, SNF 7.5–11.0

### B2. Integration tests (currently none)

Use `WebApplicationFactory` + test SQL or InMemory.

- [x] Login as Admin / Operator / Farmer; forbidden cross-role URLs
- [x] Create farmer → collection → feed issue → draft → generate → paid
- [x] Duplicate collection returns error
- [x] Farmer portal filtered by `FarmerID`
- [x] PDF endpoints return `application/pdf`
- [x] Antiforgery on POST

### B3. System / UAT (manual, then write it up)

Checklist file: [`docs/UAT_Checklist.md`](UAT_Checklist.md). Tick Pass/Fail and add screenshots for the report.

- [ ] Operator: farmer, rate chart, morning/evening collection, receipt PDF
- [ ] Feed/medicine issue, low-stock alert
- [ ] Weekly settlement generate, lock, farmer sees net
- [ ] Dispatch variance reason required
- [ ] Admin: society, users, union dashboard, unlock with reason, audit old/new JSON
- [ ] Farmer: first-login password change, own records only, settlement PDF
- [ ] Negative cases: duplicate collection, edit after lock, wrong role, empty rate chart

### B4. Test artefacts for the report

- [x] Test plan (unit → integration → system → UAT) — [`docs/Test_Plan.md`](Test_Plan.md)
- [x] Unit test cases + results — automated in `DairyManagementSystem.Tests`; log in `docs/test-evidence/`
- [x] System test cases + results — cases in UAT file; fill results during walkthrough
- [x] Screenshot / log of `dotnet test` output
- [x] Short “debugging and code improvement” notes (bugs found and fixed)

---

## C. Project report (start after freeze, or in parallel for diagrams)

Guidelines: about **100–125 pages excluding code**, project-specific, **same title as synopsis**.

### C1. Front matter (mandatory — Regional Centre can return the report without these)

- [ ] Cover page (sample format in MCSP-232 guidelines)
- [ ] Original **approved proforma + synopsis**
- [ ] Guide biodata (signed)
- [ ] **Certificate of Originality** (student + guide; signatures must match the proposal)
- [ ] Table of contents with page numbers

### C2. Body chapters

- [ ] **Introduction / objectives** — this dairy problem, not generic SDLC
- [ ] **System analysis**
  - [ ] Need identification
  - [ ] Planning: **Gantt + PERT** (both required) + **cost/effort** (Function Point or similar)
  - [ ] **SRS** (IEEE-style, your modules)
  - [ ] Paradigm: iterative / MVC
  - [ ] Diagrams for **this** system: DFD 0, 1, 2; ER with cardinality; activity (settlement); class; state (Draft → Generated → Paid → Cancelled); use-case; sequence (collection + settlement)
  - [ ] **Data dictionary** (entities + DFD flows)
- [ ] **System design**
  - [ ] Module breakdown + process logic
  - [ ] DB design, **3NF**, PK/FK/CHECK, referential diagram
  - [ ] UI design (wireframes or annotated screens)
  - [ ] Input validation
  - [ ] Procedural design (flowchart/pseudocode for rate lookup and settlement)
  - [ ] Architecture (3-tier)
  - [ ] Unit + system test cases (design here, results in Testing)
- [ ] **Coding**
  - [ ] SQL for schema/constraints (from migrations)
  - [ ] Sample well-commented C# (rate lookup, settlement transaction, locking) — **not a dump of all code**
  - [ ] Naming standards, error handling, parameter passing
  - [ ] Access rights (roles + SQL user)
- [ ] **Testing** — plan, techniques, **your** results, debugging
- [ ] **Security** — table matching synopsis (HTTPS, BCrypt, RBAC, CSRF, XSS, audit, isolation, cookies)
- [ ] **Reports** — sample PDF/layouts of all 7 reports + receipts
- [ ] **Screen layouts** — ordered dumps: login, admin, operator, farmer
- [ ] **Future scope** — items you did **not** build (from synopsis)
- [ ] **Bibliography**, glossary, appendices (user manual, test logs, CD contents)

Start diagrams from `docs/mermaid.txt` if useful.

### C3. Truth in the report (code is source of truth)

Describe what the code actually does. Do not copy unimplemented synopsis wording.

- [ ] Users/roles = ASP.NET Identity, not a custom `Users.Role` column
- [ ] PDF = **QuestPDF**, not iText/RDLC
- [ ] Medicine = inventory `ItemType`, not a separate table
- [ ] Settlement statuses include **Cancelled** and admin unlock
- [ ] SMS is configurable and off by default
- [ ] Do not claim shift close / quality reject / iText unless they are really implemented

---

## D. CD / viva / submission pack

- [ ] Bound original report
- [ ] CD **attached on last page**: published **executable** (or `dotnet publish` output + README how to run), not source-only
- [ ] Identical CD + photocopy kept by you for viva
- [ ] Demo database seed (`DemoOperationsSeeder`) + default logins in the user manual
- [ ] Envelope marked **MCA PROJECT REPORT (MCSP-232)**
- [ ] Practice viva: live demo + be ready to write portions of code on paper

---

## Suggested order

1. [ ] Decide A1 and A2: implement or drop from “implemented”.
2. [ ] A4 operator audit (or document admin-only).
3. [ ] A3 reconciliation columns/report.
4. [ ] A5 least-privilege SQL user (can be documentation-only until deploy).
5. [ ] B1 + B2 tests; keep `dotnet test` log.
6. [ ] Freeze the product. No more features.
7. [ ] B3 UAT with screenshots.
8. [ ] Draw C2 diagrams.
9. [ ] Write the full report from the frozen system.
10. [ ] D print, bind, burn CD.

---

## Out of scope (do not build)

- Mobile application (Android / iOS)
- QR-code farmer identification
- Online payment / NEFT / IMPS
- WhatsApp notifications
- Multi-society cloud central portal
- Offline-first sync
- Enabling production SMS (optional future scope)

---

## Related docs

| File | Purpose |
|---|---|
| `docs/MCSP 232 - Project Guidelines - MCA_NEW (1) - Copy.md` | Official IGNOU MCSP-232 guidelines |
| `docs/IGNOU_MCSP232_Dairy_Synopsis_Final.md` | Submitted synopsis |
| `docs/IGNOU_MCSP232_Dairy_Synopsis_Context.md` | Synopsis as development source of truth |
| `docs/Smart_Dairy_Cursor_Context.md` | Compact coding / UI rules |
| `docs/mermaid.txt` | Starting point for report diagrams |
