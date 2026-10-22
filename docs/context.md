# Project Context — Copilot Handoff

> **Latest handoff (2026-10-22):** Read this section and `NEXT_STEPS.md` first. Earlier Policy PDF and Claims Manager history below remains useful, but earlier quote-flow descriptions are superseded by the simplified requirements here.

## Current Project Snapshot

- **Purpose:** SecureSphere vacation-home insurance platform covering customer/property management, insurance quotes, payments, policies/documents, and claims.
- **Architecture:** Angular 21 standalone frontend (`../VacationHomeInsuranceUI/`) calls .NET 8 REST services through Ocelot. Backend solution is `../VacationHomeInsurance/VacationHomeInsurance.slnx`; service areas include Auth, Customer, ProductQuote/Quote, Policy, and Claim. Backend persistence uses EF Core/SQL Server; quote request validation uses FluentValidation.
- **Frontend tooling:** npm scripts are defined in `../VacationHomeInsuranceUI/package.json` (`start`, `build`, `test`).
- **Existing features:** Role-based Customer/Admin/Claims Manager interfaces; quote, payment, and policy workflows; claims workflows; synchronous private policy PDF generation with metadata, authorized downloads, and retry/recovery (historical context below). These broad features were not all revalidated in the latest quote task.

## Current Quote Requirements and Implementation

Required flow: Customer generates a quote using the original form/pricing logic → Admin reviews and approves current price, optionally edits price and approves, or rejects → Customer views the decision and can accept only an Admin-approved quote → payment after acceptance → policy issuance only after successful payment and all existing validation. The Customer cannot change price or bypass Admin approval. **Remove alternative quotes, comparison, quote groups/three-option cap, and Request New Quote; preserve normal quote generation.**

Current code work reported in the preceding session:

- Removed alternative-generation service/repository paths and quote-group fields/index configuration.
- New generated quotes enter `PendingAdminApproval`; Admin's existing approval endpoint accepts optional `revisedAnnualPremium` and `priceAdjustmentReason`. Backend validates revised price (positive, two decimal places, supported SQL decimal range), requires a reason when price changes, updates `FinalAnnualPremium` and monthly equivalent, and writes existing approval history/audit data. Row-version checks and Admin authorization remain in the existing path.
- Customer acceptance now requires `AdminApproved`, does not change status back to review, and records acceptance time after the latest Admin decision. Payment eligibility checks approval, ownership/acceptance, expiry/deadline, and final premium; existing successful-payment/policy workflow was preserved.
- Admin UI supports optional revised premium/reason; Customer UI offers acceptance only for approved, unaccepted, unexpired quotes.
- Illustrative quote pricing uses `property value × configured base-rate fraction × risk-category multiplier`, plus each selected coverage's configured flat annual INR charge. Coverage limits are `property value × configured limit percentage`; the seeded comprehensive defaults are 100% dwelling, 20% other structures, and 50% personal property. Seeded pricing values (0.5% base rate, multipliers 1.0/1.25/1.5 and fixed coverage charges), risk-score weights, and score bands are academic examples, not insurer market rates or actuarially validated rates. Monthly equivalent is annual premium divided by 12; it is not an installment plan. Newly generated quote snapshots retain their calculated values; Admin price changes update final annual premium and monthly equivalent, while old quotes and issued policy snapshots are not recalculated.

Relevant files touched in that work (the list is from conversation/applied changes, **not** a Git diff):

- Backend: `VacationHome.Quote.Domain/Entities/Quote.cs`; `VacationHome.Quote.Infrastructure/Persistence/Configurations/QuoteConfiguration.cs`; `VacationHome.Quote.Infrastructure/Persistence/ProductQuoteDatabaseInitializer.cs`; `VacationHome.Quote.Infrastructure/Persistence/Repositories/QuoteRepository.cs`; `VacationHome.Quote.Application/Interfaces/Services/IQuoteGenerationService.cs`; `VacationHome.Quote.Application/Interfaces/Repositories/IQuoteRepository.cs`; `VacationHome.Quote.Application/DTOs/Admin/AdminApprovalRequest.cs`; `VacationHome.Quote.Application/Validators/ApproveQuoteRequestValidator.cs`; `VacationHome.Quote.Application/Services/QuoteGenerationService.cs`; `VacationHome.Quote.Application/Services/QuoteAdminService.cs`; `VacationHome.Quote.Application/Services/QuoteLifecycleService.cs`; `VacationHome.Quote.Application/Services/QuoteIntegrationService.cs`; `VacationHome.Quote.Application/Mappers/QuoteMapper.cs`; `VacationHome.Quote.Application/Mappers/QuoteIntegrationMapper.cs`.
- Frontend: `src/app/core/models/admin/quotes/approve-quote-request.model.ts`; `src/app/core/services/admin.service.ts`; `src/app/core/services/customer-quote.service.ts`; `src/app/features/admin/quotes/admin-quotes.component.ts`; `src/app/features/admin/quotes/admin-quotes.component.html`; `src/app/features/customer/quotes/customer-quotes.component.ts`; `src/app/features/customer/quotes/customer-quotes.component.html`.
- Tests/solution: `VacationHome.Quote.Tests/VacationHome.ProductQuote.Tests.csproj`; `VacationHome.Quote.Tests/QuoteWorkflowRulesTests.cs`; `VacationHomeInsurance.slnx`.

### Verification status (latest quote work)

- **Verified:** Quote API build passed with 0 errors using temporary output (default output was locked by the running Quote API); Angular production build passed and prerendered 30 routes; focused quote unit tests passed, 6/6.
- **Not verified:** Full integration/E2E workflow, Admin audit/history persistence, database startup SQL against the actual SQL Server, UI behavior in a browser, full solution build/tests, and Angular tests.
- **Runtime:** Existing Quote API was not restarted. Its loaded binaries do not include these source changes until an intentional restart.
- **Git:** `git status` failed because workspace and project directories have no `.git` metadata. No Git diff was available; inspect the correct repository/worktree first in a new session.
- **Schema caveat:** `ProductQuoteDatabaseInitializer.cs` currently has startup SQL that updates all `Active` (`1`) quotes to `PendingAdminApproval` (`2`) and drops alternative-only columns/indexes/constraints if present. This SQL has not run against the live database. Inspect carefully before restarting; confirm desired migration of all Active rows and target DB before applying. Do not assume cleanup has occurred.
- **Potential edge:** Some existing lifecycle/UI logic treats `Active` as the only withdrawable state. Verify withdrawal behavior for newly `PendingAdminApproval` quotes against the requested workflow before deciding whether changes are needed.

### Useful quote endpoints and rules

- `POST /quotes`, `GET /quotes`, `GET /quotes/{quoteId}`, `POST /quotes/{quoteId}/accept`, `POST /quotes/{quoteId}/withdraw`.
- `GET /admin/quotes`, `GET /admin/quotes/{quoteId}`, `POST /admin/quotes/{quoteId}/approve`, `/reject`, `/invalidate`.
- Internal payment validation: `POST /internal/quotes/validate-for-payment`; existing payment submission is Policy Service `POST /payments`.
- Quote statuses are in `VacationHome.Quote.Domain/Enums/QuoteStatus.cs`: Active, PendingAdminApproval, AdminApproved, AdminRejected, Expired, Invalidated, Withdrawn, ConvertedToPolicy. Expected new flow: generated→PendingAdminApproval→AdminApproved/AdminRejected; acceptance occurs after approval without changing the approved status; payment/conversion follows only after successful validation/payment.
- Existing `Quote`, `QuoteCoverage`, and `QuoteApprovalHistory` snapshots/history and audit mechanisms should be reused. Policy Service already compares payment to the quote's final annual premium and creates policy only after successful payment.

## Continue Safely

First inspect the correct Git diff and reconcile any interrupted changes; metadata was unavailable previously. Then inspect initializer SQL and migration/data state before any restart. Add service/integration tests for approval at current/revised price, audit/history, reject, concurrency/row-version, post-approval acceptance and payment; verify legacy/Active withdrawal behavior. Run targeted Quote API build/tests, Angular build/tests, and only then wider checks. Keep running services intact unless a coordinated restart is necessary.

### Commands

```powershell
Set-Location 'D:\CapstoneProject\VacationHomeInsuranceUI\VacationHomeInsurance'
dotnet build .\VacationHome.Quote.Api\VacationHome.ProductQuote.Api.csproj
dotnet test .\VacationHome.Quote.Tests\VacationHome.ProductQuote.Tests.csproj

Set-Location 'D:\CapstoneProject\VacationHomeInsuranceUI\VacationHomeInsuranceUI'
npm run build
npm test
```

## 1. Project Overview
- Project type: Existing .NET application.
- Current status: Partially completed by a previous GitHub Copilot session.
- Objective: Complete the remaining business rules, refactor the UI, and improve code quality.
- Important: Continue from the existing implementation. Do not rebuild the project from scratch.

## 2. Primary Objectives
1. Implement all remaining business rules according to the original requirements.
2. Refactor the UI and application logic to follow SOLID principles.
3. Improve maintainability, readability, performance, and reliability.
4. Follow established .NET and C# coding conventions and best practices.
5. Minimize Copilot token consumption without compromising correctness or code quality.
6. Preserve existing functionality and avoid unnecessary changes.

## 3. Token Optimization Guidelines
- Inspect only the files relevant to the current task instead of repeatedly reading the entire codebase.
- Read existing documentation, this context file, and relevant project instructions before starting.
- Reuse information already gathered during the current session.
- Avoid repeatedly explaining the project or regenerating existing code.
- Prefer concise, targeted prompts and focused changes.
- Make small, logically grouped edits rather than rewriting entire files.
- Avoid unnecessary abstractions, boilerplate, duplicate code, and verbose explanations.
- Do not generate lengthy plans or summaries for simple tasks.
- Do not repeatedly run the same commands or tests unless the code changes or a failure requires it.
- Use existing libraries, components, services, and utilities whenever appropriate.
- Summarize findings and progress briefly, highlighting only decisions, changes, blockers, and next steps.
- Ask questions only when missing information could materially affect correctness.
- Never sacrifice security, correctness, maintainability, or testing just to reduce token usage.

## 4. Coding Standards and Best Practices
- Follow SOLID principles where appropriate, especially when changing existing architecture.
- Follow DRY (Don't Repeat Yourself), KISS (Keep It Simple), and separation of concerns.
- Prefer clear, descriptive names for classes, methods, variables, and interfaces.
- Keep methods focused on a single responsibility and reasonably small.
- Avoid deeply nested conditionals, duplicated business rules, magic numbers, and unnecessary complexity.
- Use dependency injection and appropriate abstractions where they provide genuine value.
- Follow established C# naming conventions, nullable reference type settings, and asynchronous programming practices.
- Use async/await appropriately for I/O-bound operations; avoid blocking asynchronous calls.
- Handle exceptions meaningfully. Do not silently swallow exceptions or use broad catch blocks without justification.
- Validate inputs and enforce business rules at the appropriate application boundaries.
- Avoid unnecessary allocations, repeated database calls, inefficient queries, and other measurable performance bottlenecks.
- Follow secure coding practices. Never hardcode credentials, secrets, or sensitive configuration.
- Reuse existing architecture and coding conventions unless there is a clear reason to improve them.
- Avoid overengineering, premature optimization, unnecessary design patterns, and abstractions that add complexity without benefit.
- Add comments only when they clarify non-obvious decisions or business logic.
- Keep changes consistent with the project's existing framework and dependency versions.

## 5. UI and Architecture Refactoring
- Separate presentation, business logic, and data access responsibilities.
- Keep UI components focused on rendering and user interaction.
- Move business logic into appropriate services or application-layer components.
- Avoid duplicating validation or business rules across UI components.
- Use reusable components when multiple parts of the application share the same behavior.
- Follow accessibility, consistency, responsiveness, and error-feedback best practices.
- Avoid unnecessary UI redesigns or changes to existing user workflows.
- Refactor incrementally and preserve existing behavior unless the requirements explicitly demand a change.

## 6. Existing Implementation and Progress
The previous Copilot session completed part of the task.

Before editing:
1. Inspect the project structure and relevant source files.
2. Review Git status and diffs to identify existing modifications.
3. Compare the implementation with the original requirements.
4. Identify completed, incomplete, and potentially incorrect functionality.
5. Check for existing tests and known build errors.

Do not assume that functionality is complete or incomplete without evidence.

## 7. Implementation Workflow
For each focused task:
1. Identify the exact requirement and affected files.
2. Inspect the minimum necessary code and dependencies.
3. Explain the intended change briefly.
4. Implement the smallest maintainable solution.
5. Check for regressions and preserve existing behavior.
6. Build and run relevant tests when possible.
7. Review the diff for unnecessary changes, duplication, security issues, and coding-standard violations.
8. Report the result concisely.

Do not refactor unrelated files simply because they could be improved. If a broader architectural change is necessary, explain why before proceeding.

## 8. Testing and Verification
- Prefer existing tests and test frameworks.
- Add or update tests for important business rules and changed behavior.
- Test edge cases, invalid inputs, and failure paths where relevant.
- Run targeted tests first, followed by broader validation when appropriate.
- Do not claim that a build or test passed unless it was actually executed successfully.
- Distinguish pre-existing failures from failures introduced by new changes.
- If verification cannot be completed, state the limitation clearly.

## 9. Copilot Session Handoff
Maintain this section as work progresses.

### Completed
- Policy issuance generates PDF documents synchronously after payment and policy persistence through `IPolicyDocumentService`.
- Private local file storage, document versions/checksums/status metadata, recovery handling, and a policy-level filesystem lock are implemented in Policy Service.
- Added authenticated Customer document listing/download APIs and Administrator listing/download/retry APIs; configured matching Ocelot routes.
- Customer policy details and Administrator policy management now show document status and provide download/retry controls.
- Added unique quote indexes for policy issuance and successful payments, and tests for policy PDF generation, owner/role authorization, missing-file handling, retry/idempotency, and index configuration.
- MigraDocCore.Rendering 1.3.67 is already used and its installed package license is MIT; no PDF library replacement was needed.
- Development PDF configuration uses the SecureSphere demo identity, explicitly sample (not approved) wording, configured supplemental terms, and the required academic disclaimer. PDFs use a branded header, numbered sections, styled coverage table, and page numbering.
- Quotes may be generated and submitted for any protection type even when an existing policy would prevent issuance. Admin approval checks active policy overlap, and payment revalidates overlap immediately before policy creation. Property and Natural Disaster protection can coexist; Comprehensive protection conflicts with either.
- Customer quote details show an issuance warning when an active policy conflicts, while keeping risk-factor explanations administrator-only. Downloaded policy PDFs use an explicit policy-number/version filename.

### In Progress
- Customer risk-detail visibility, policy conversion warnings, and policy PDF filenames are updated and validated; no code changes remain in progress.

### Pending
- Production insurer identity and legally approved wording must be supplied through deployment configuration before production policy issuance. Development sample wording is not legally approved and the PDF clearly states it is not valid insurance coverage.
- Apply/verify the idempotent Policy Service schema update against the target SQL Server database. It is invoked at API startup; no live database migration was run during this session.

### Verification
- Build status: Policy Service tests passed; ProductQuote API and Policy API builds passed; Angular production build passed. Updated Quote and Policy APIs were restarted after building.
- Tests executed: Policy Service: 12 passed, including four policy-overlap cases. Angular: 23 passed.
- Configuration validation: base and Development `appsettings.json` files parse successfully; the obsolete wording configuration key has been removed.
- Known issues: Existing `CustomerServiceClient.GetCustomerStatusAsync` nullable-return warning remains. Git status/diff could not be inspected because the provided workspace directories contain no `.git` metadata.

### Important Decisions
- Do not emit a misleading policy contract when insurer identity or approved policy wording is missing; generation fails safely after successful issuance and can be retried once configuration is supplied.
- Store files under the configured `PolicyDocuments:StoragePath` outside `wwwroot`; API responses contain metadata and bytes, never storage keys or physical paths.
- Failed generation and missing files preserve payment/policy rows and historical document metadata. Retry reuses an available latest file and only creates a new version when recovery is required.

Update this section after meaningful milestones so another Copilot session can continue without repeating investigation or implementation.

## 10. Original Requirements
Implement synchronous policy PDF generation after successful issuance in the existing backend; use persisted policy, customer, property, premium, coverage, date, and configured-term data; keep files private and metadata in the policy database; expose authenticated owner/Admin downloads; wire existing Angular Customer/Admin policy pages and Ocelot; preserve payment/policy records on document failure; support safe retry, integrity checks, concurrency, and historical versions; add focused tests and validate .NET/Angular builds.

## 11. Claim Incident Coverage Eligibility
- Claim creation keeps one incident per claim and supports multiple selected coverage items with separate requested amounts.
- Customer incident choices are Fire, Flood, Theft, Earthquake, Wildfire, Hurricane, and Other. Historical claim enum values remain in the database but are not selectable for new claims.
- Eligibility is configured in `../VacationHomeInsurance/VacationHome.ClaimService.API/appsettings.json` under `ClaimCoverageEligibility:Rules`, scoped by ProductType and IncidentType. Static rules use shared Coverage definition IDs, not ProductCoverage mapping IDs. Only coverages attached to the selected active Policy are candidates; optional coverages not attached to that Policy cannot be claimed.
- An explicitly configured rule with an empty `CoverageIds` list means no coverage is eligible. A missing plan/incident rule is reported as missing configuration. `UsePolicyCoverages: true` is reserved for `Other`, which dynamically considers all coverage records on the selected Policy rather than the product catalog.
- The current rules configure Fire and Theft for Property Protection and Comprehensive Protection; Flood, Earthquake, Wildfire, and Hurricane for Natural Disaster Protection and Comprehensive Protection; and no coverage by default for the specified unsupported plan/incident pairs. For Fire, the configured coverages are Dwelling, Other Structures, and Personal Property; for Theft they are Theft Protection and Personal Property.
- `GET /claims/eligible-coverages` returns eligible policy coverage snapshots and remaining limits. Create requests and updates for selectable incident types revalidate policy ownership/status/dates, selected-policy coverage membership, eligibility, and limits. The PolicyCoverage data model has no independent active/deactivated status; the current code can check policy status and whether a coverage exists in that policy snapshot, but cannot separately validate per-coverage activation.
- Historical legacy claim types remain readable and can be retained when updating an existing claim, but cannot be selected for a new incident.
- No database migration is needed for the new Fire enum value; ClaimType is stored as an integer and the value is additive.

## 12. Instructions for the New Copilot Session
Read this file and any applicable repository instructions before starting.

First, assess the current implementation and provide a concise progress report. Then continue from the verified state, prioritizing unfinished requirements.

Optimize for correctness, maintainability, and token efficiency together. Do not restart the project, duplicate completed work, make speculative changes, or perform broad refactoring without justification.

When requirements are ambiguous, ask for clarification instead of inventing business rules.