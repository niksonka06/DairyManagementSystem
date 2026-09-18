# System / UAT checklist (MCSP-232)

Walk these as Admin, Society Operator, and Farmer. Tick Pass/Fail, add screenshot file names for the report.

Default seeded admin (if unchanged): `admin@dairysystem.local` / `ChangeMe!123` (must change password on first login unless already cleared).

| ID | Role | Step | Expected | Pass? | Screenshot |
|---|---|---|---|---|---|
| UAT-01 | Operator | Register farmer with bank details; farmer gets login | Farmer appears in register; credentials shown | | |
| UAT-02 | Operator | Maintain fat/SNF/CLR rate chart | New band saves; collection uses it | | |
| UAT-03 | Operator | Record morning and evening collection | Amount = qty × chart rate; one row per farmer per shift | | |
| UAT-04 | Operator | Download collection receipt PDF | PDF opens with qty, fat, rate, amount | | |
| UAT-05 | Operator | Reject a pour (quality) with reason | Amount ₹0; farmer still sees the row as Rejected | | |
| UAT-06 | Operator | Close morning shift | Cannot add/edit morning until reopen | | |
| UAT-07 | Operator | Issue feed and medicine; stock falls | Issue listed; low-stock badge if at threshold | | |
| UAT-08 | Operator | Generate weekly settlement | Gross, deductions, net; collections lock | | |
| UAT-09 | Farmer | Open settlement after generate | Net matches operator; can download settlement PDF | | |
| UAT-10 | Operator | Dispatch with variance above 5% and no reason | Save blocked until reason entered | | |
| UAT-11 | Operator | Open Daily Reconciliation report | Collected vs dispatched vs variance vs reason | | |
| UAT-12 | Admin | Society, operators, union dashboard | Society list and consolidated litres | | |
| UAT-13 | Admin | Unlock generated settlement with reason | Collections editable again; audit shows unlock | | |
| UAT-14 | Admin / Operator | Audit trail | Old/new JSON visible; operator sees own society only | | |
| UAT-15 | Farmer | First login password change | Forced change, then dashboard | | |
| UAT-16 | Farmer | Own records only | Cannot open another farmer’s receipt URL | | |
| UAT-17 | Negative | Duplicate collection same farmer/date/shift | Error: already exists | | |
| UAT-18 | Negative | Edit collection after settlement lock | Edit blocked | | |
| UAT-19 | Negative | Farmer URL `/Operator/MilkCollection` | Access denied | | |
| UAT-20 | Negative | Collection with no matching rate band (not rejected) | Error asking for chart band or rejection | | |

Automated stand-ins for calculation UAT (qty × rate, net after deductions, negative carry-forward) live in `DairyManagementSystem.Tests/UatScenarioTests.cs`.
