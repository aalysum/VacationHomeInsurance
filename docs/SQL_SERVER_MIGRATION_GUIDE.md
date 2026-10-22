# SQL Server Migration Guide

This guide records the database configuration found in the repository and a cautious process for moving the application from local SQL Server Express to a company SQL Server. The company host, database names, authentication policy, network requirements, and target data state are unknown; obtain them from the company DBA rather than inferring them.

## Scope and verified architecture

The backend uses EF Core with the SQL Server provider. The runtime implementation has five separate databases:

| Service | Connection string key | Current database |
|---|---|---|
| Authentication | `ConnectionStrings:DefaultConnection` | `VacationHomeAuthDb` |
| Customer | `ConnectionStrings:CustomerDb` | `VacationHomeCustomerDb` |
| Claims | `ConnectionStrings:ClaimDb` | `VacationHomeClaimDb` |
| Policy | `ConnectionStrings:PolicyDb` | `VacationHomePolicyDb` |
| Product/Quote | `ConnectionStrings:ProductQuoteDb` | `VacationHomeProductQuoteDb` |

The API Gateway and Angular application do not connect directly to SQL Server. They call the backend services. The five databases are independently configured; do not point all services at one shared database unless the data ownership and schema design are intentionally changed and reviewed.

### Important project-layout discrepancy

`VacationHomeInsurance.slnx` lists the `VacationHome.ProductQuote.*` project paths. Those include a minimal `VacationHome.ProductQuote.Api` project whose `Program.cs` has no database registration or initializer. The database-backed API and implementation inspected in this workspace are under `VacationHome.Quote.Api`, `VacationHome.Quote.Application`, and `VacationHome.Quote.Infrastructure`; their project file names still use `VacationHome.ProductQuote.*`. The `VacationHome.Quote.Api` path is not listed in the solution file.

Before building or deploying the Product/Quote service on the company laptop, confirm which project and publish artifact are intended. The connection string and startup behavior described below are from the database-backed `VacationHome.Quote.Api` implementation. Do not assume that building the solution-listed scaffold uses that implementation.

## Connection strings

The five checked-in runtime connection strings all use SQL Server Express on the local machine, database-specific catalog names, Windows integrated authentication, and `TrustServerCertificate=True`. The Claims and database-backed Product/Quote strings also enable Multiple Active Result Sets. No SQL passwords are present in these connection strings.

| Service | Exact key | Checked-in current value (no credentials) |
|---|---|---|
| Auth API | `ConnectionStrings:DefaultConnection` | `Server=localhost\SQLEXPRESS;Database=VacationHomeAuthDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Customer API | `ConnectionStrings:CustomerDb` | `Server=localhost\SQLEXPRESS;Database=VacationHomeCustomerDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Claim API | `ConnectionStrings:ClaimDb` | `Server=localhost\SQLEXPRESS;Database=VacationHomeClaimDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |
| Policy API | `ConnectionStrings:PolicyDb` | `Server=localhost\SQLEXPRESS;Database=VacationHomePolicyDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Database-backed Product/Quote API | `ConnectionStrings:ProductQuoteDb` | `Server=localhost\SQLEXPRESS;Database=VacationHomeProductQuoteDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True` |

The design-time Policy DbContext factory has a separate hardcoded LocalDB connection; the database-backed Product/Quote factory reads `ProductQuoteDb` from configuration. See the file inventory below. EF tooling may therefore target a different server than the running APIs.

### Connection string examples

Use the same key names above when configuring each service. Replace only company-provided placeholders; the names shown here are examples, not confirmed company settings.

**Local SQL Server Express (current development pattern):**

```text
Server=localhost\SQLEXPRESS;Database=<service-database>;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Remove `MultipleActiveResultSets=true` for services that do not currently use it unless the application has a verified need for it. This example mirrors the current development trust setting; it is not a production security recommendation.

**Company SQL Server with Windows Integrated Authentication (only if supported and approved):**

```text
Server=tcp:<company-host>,<port>;Database=<approved-service-database>;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;Connect Timeout=<approved-seconds>
```

Grant the Windows/domain identity running each backend service the minimum required database permissions. Integrated authentication uses the service process identity; it does not automatically use the identity of the person signed into Angular. Confirm that the company network, domain, SQL Server configuration, and service hosting model support integrated authentication.

**Company SQL Server with SQL Server Authentication (only if company policy permits):**

```text
Server=tcp:<company-host>,<port>;Database=<approved-service-database>;User ID=<secret-user>;Password=<injected-secret>;Encrypt=True;TrustServerCertificate=False;Connect Timeout=<approved-seconds>
```

Supply the password through an approved secret store or protected deployment environment, not a checked-in file or command history. Do not use `TrustServerCertificate=True` to bypass a certificate problem on a company server; install/configure a trusted certificate or use the DBA-approved TLS settings.

The current strings do not specify `Encrypt` or a connection timeout. Determine the required encryption, certificate chain, port/instance, and timeout with the DBA and test them using the actual .NET SqlClient version. Do not assume company policy or driver defaults.

### Where to set values and precedence

The services use `IConfiguration.GetConnectionString(...)` and `UseSqlServer(...)`. ASP.NET Core loads the base `appsettings.json`, then `appsettings.{Environment}.json`, then environment variables and other configured providers; later providers override earlier ones. In the standard environment-variable provider, use a double underscore for each colon:

```text
ConnectionStrings__DefaultConnection
ConnectionStrings__CustomerDb
ConnectionStrings__ClaimDb
ConnectionStrings__PolicyDb
ConnectionStrings__ProductQuoteDb
```

Set only the relevant connection string in the process/deployment environment for each backend. The checked-in `appsettings.Development.json` files currently contain logging or non-database settings, not connection string overrides. The launch profiles select `Development`; they do not configure SQL access. No `UserSecretsId` or repository SQL deployment/container manifest was found in the inspected source. User-level secrets or company deployment settings outside this workspace could not be inspected.

For local development, keep the existing Express values intact. Put company-only overrides in the company laptop's user/deployment configuration, not in source-controlled local defaults. If the company deployment deliberately uses an environment-specific JSON file, keep it out of source control if it contains credentials and verify which environment name is actually set.

## File inventory: what to review

Paths in this table are relative to the solution root `VacationHomeInsurance/`. “Required” refers to a company migration; “conditional” means it depends on the company's architecture or selected deployment mode. Verification steps are read-only until an approved test environment/database is used.

| File path | Purpose | Current database-related setting or behavior | What and where to change/review | Required or conditional | Verification |
|---|---|---|---|---|---|
| `VacationHomeInsurance.slnx` | Solution project membership. | Lists `VacationHome.ProductQuote.*` paths, including the scaffold API. | Reconcile intended Product/Quote project membership with the database-backed `VacationHome.Quote.*` implementation before using solution builds or publish output. | Required to resolve before company deployment. | Open/build the intended project in a controlled environment and confirm the published API contains the Product/Quote controllers, DI registration, and selected connection-string name. |
| `VacationHome.Auth.Api/appsettings.json` | Auth API defaults. | `ConnectionStrings:DefaultConnection` targets `VacationHomeAuthDb` on `localhost\SQLEXPRESS` with Windows auth; also contains a development JWT key. | Review `ConnectionStrings:DefaultConnection`; override outside source. Replace development JWT settings through approved secrets/configuration as well. | Database override required; JWT replacement required for non-development use. | Inspect effective Auth API configuration without printing secrets; make an authenticated DB-backed Auth API request in a test environment. |
| `VacationHome.Auth.Api/appsettings.Development.json` | Auth Development overrides. | Logging only; no connection string override. | Leave local Development behavior unchanged. Add no company secret here. | Conditional; normally unchanged. | Confirm the selected environment and effective `DefaultConnection` source. |
| `VacationHome.Auth.Api/Program.cs` | Auth startup and DB initialization. | Calls `EnsureCreatedAsync()` on startup in every environment, then calls `IdentitySeeder.SeedAsync`. | Review before connecting to a company DB. `EnsureCreated` is not an incremental schema migration. Decide a safe schema provisioning path and whether startup seeding is appropriate. | Required review; code changes may be needed for safe production deployment. | Confirm startup logs and schema on an isolated, approved database; do not test on company data. |
| `VacationHome.Auth.Infrastructure/DependencyInjection.cs` | Auth EF Core registration. | Reads `DefaultConnection`, registers `AuthDbContext` with `UseSqlServer` and transient retry (3 retries). | Usually no provider change; verify the connection-string key and company SQL permissions/retry expectations. | Conditional. | Verify effective key and make a controlled DB-backed Auth API request. |
| `VacationHome.Auth.Infrastructure/Persistence/Context/AuthDbContext.cs` | Auth/Identity EF model. | Defines Identity and application configuration mappings. | Review only if schema/provider model must change; schema is created from this model. | Conditional. | Compare the generated/reviewed schema with the target DB before applying it. |
| `VacationHome.Auth.Infrastructure/Identity/IdentitySeeder.cs` | Creates roles and an initial administrator. | Runs every Auth API startup; the initial administrator credential is hardcoded in source and must be treated as a development-only risk. | Do not seed a company database with the development administrator. Replace with an approved one-time provisioning/onboarding process and rotate/remove sample credentials. | Required security review before company use. | Confirm no default/demo admin account remains and required roles/users were provisioned through the approved process. |
| `VacationHome.Auth.Infrastructure/VacationHome.Auth.Infrastructure.csproj` | Auth EF provider/tooling packages. | References SQL Server provider and EF Design at `8.0.31`. | Keep provider/tooling versions compatible with the target .NET/EF version and SQL Server; avoid changing provider merely because the server is remote. | Conditional. | Restore/build the intended project and verify compatible provider versions. |
| `VacationHome.Customer.Api/appsettings.json` | Customer API defaults. | `ConnectionStrings:CustomerDb` targets `VacationHomeCustomerDb` on local Express with Windows auth. | Review/override `ConnectionStrings:CustomerDb` outside source. | Required company override. | Verify effective configuration and a DB-backed customer/property request. |
| `VacationHome.Customer.Api/appsettings.Development.json` | Customer Development overrides. | Logging only; no connection string override. | Keep local development configuration; do not put company secrets here. | Conditional; normally unchanged. | Confirm environment and effective connection-string source. |
| `VacationHome.Customer.Api/Program.cs` | Customer API startup. | Calls `EnsureCreatedAsync()` in every environment. | Review because it may create a new database but does not reconcile an existing database to a changed model. | Required review. | Verify target is an approved empty/provisioned DB before any startup. |
| `VacationHome.Customer.Infrastructure/DependencyInjection.cs` | Customer EF Core registration. | Reads `CustomerDb`, uses SQL Server and retry-on-failure. | Check the key, account grants, TLS, and retry behavior. | Conditional. | Verify an approved DB-backed customer/property operation. |
| `VacationHome.Customer.Infrastructure/Persistence/Context/CustomerDbContext.cs` | Customer EF model. | Defines entities and applies configurations from its assembly. | Use as schema source when preparing the Customer database. | Required for schema preparation. | Compare reviewed schema to model and check constraints/indexes in staging. |
| `VacationHome.Customer.Infrastructure/VacationHome.Customer.Infrastructure.csproj` | Customer EF provider/tooling packages. | SQL Server and EF Design packages are `8.0.31`. | Keep EF/provider versions compatible. | Conditional. | Restore/build; ensure the intended API and infrastructure projects resolve these packages. |
| `VacationHome.ClaimService.API/appsettings.json` | Claim API defaults and external-service URLs. | `ConnectionStrings:ClaimDb` targets `VacationHomeClaimDb` on local Express with Windows auth and MARS. | Review/override `ClaimDb`; separately review `ExternalServices` URLs for company service addresses. | Required DB override; service URLs conditional. | Verify the DB request and all configured service base URLs from the company laptop. |
| `VacationHome.ClaimService.API/appsettings.Development.json` | Claim Development overrides. | Logging only; no DB override. | Keep company secrets out; use local override only for local testing. | Conditional; normally unchanged. | Confirm environment and effective ClaimDb configuration. |
| `VacationHome.ClaimService.API/Program.cs` | Claim startup. | On every startup calls `EnsureCreatedAsync()` and raw SQL conditionally adds `dbo.Claims.PolicyNumber` with a default for existing rows. | Treat the SQL as a schema/data change. Review against the target schema and historical rows before starting the service. | Required review. | In staging, verify column type/nullability/default and existing row values after DBA-approved deployment. |
| `VacationHome.ClaimService.Infrastructure/DependencyInjection.cs` | Claim EF Core registration. | Reads `ClaimDb`, uses SQL Server and retry-on-failure. | Verify the key, login grants, TLS, MARS need, and retry behavior. | Conditional. | Make an approved claim DB-backed request in staging. |
| `VacationHome.ClaimService.Infrastructure/Persistence/ClaimDbContext.cs` | Claim EF model. | Defines claim persistence and applies assembly configurations. | Use to review claim tables, foreign keys, and indexes for schema preparation. | Required for schema preparation. | Compare reviewed schema with target metadata in staging. |
| `VacationHome.ClaimService.Infrastructure/VacationHome.ClaimService.Infrastructure.csproj` | Claim EF provider/tooling packages. | SQL Server and EF Design packages are `8.0.31`. | Keep provider/tooling versions compatible. | Conditional. | Restore/build and run appropriate tests against test resources only. |
| `VacationHome.PolicyService.API/appsettings.json` | Policy API defaults and service/document settings. | `ConnectionStrings:PolicyDb` targets `VacationHomePolicyDb` on local Express with Windows auth. | Review/override `PolicyDb`; separately set company service URLs and policy document configuration as appropriate. These are not SQL settings but affect issuance. | Required DB override; other settings conditional. | Test policy and payment flow in staging without using real production-like issuance until approved. |
| `VacationHome.PolicyService.API/appsettings.Development.json` | Policy Development overrides. | Logging and illustrative sample policy-document wording; no DB override. | Do not treat the sample wording as approved company policy wording. Keep local Development settings separate from company deployment. | Required policy-content review; DB override is separate. | Confirm company-approved wording/provider configuration and DB environment independently. |
| `VacationHome.PolicyService.API/Program.cs` | Policy startup and schema preparation. | In every environment calls `EnsureCreatedAsync()` and then `PolicyDocumentSchemaMigration.ApplyAsync(...)`. | Review both operations before company startup; custom SQL performs schema/data changes on each startup when missing objects are detected. | Required review. | Verify reviewed migration effects on a restored staging copy; inspect schema and affected rows afterward. |
| `VacationHome.PolicyService.Infrastructure/AddInfrastructure.cs` | Policy EF Core registration and service URLs. | Reads `PolicyDb`, uses SQL Server with retries; also reads Quote/Customer service URLs. | Verify DB key, company SQL permissions, encryption, and service endpoints. | Required DB override; endpoints conditional. | Test DB-backed policy read/write and service dependencies in staging. |
| `VacationHome.PolicyService.Infrastructure/Persistence/PolicyDbContext.cs` | Policy/payment/document EF model. | Defines policy model and applies assembly configurations. | Use as the schema reference for Policy and Payment tables. | Required for schema preparation. | Compare reviewed schema and constraints with staging metadata. |
| `VacationHome.PolicyService.Infrastructure/Persistence/PolicyDocumentSchemaMigration.cs` | Hand-written idempotent SQL schema update. | Adds many policy snapshot columns and `PolicyCoverages.CoverageDescription` with defaults and `WITH VALUES`; creates `PolicyDocuments` and indexes, including unique indexes on policy quote/payment/document keys. | Have the DBA review every statement and effects on legacy rows before applying. Unique-index creation can fail if existing data violates uniqueness. It is not an EF migration history. | Required review before any existing DB startup. | Test on a restored copy; validate row counts/values and duplicate checks before/after index creation. |
| `VacationHome.PolicyService.Infrastructure/Persistence/PolicyDbContextFactory.cs` | EF design-time Policy DbContext factory. | Hardcodes `(localdb)\MSSQLLocalDB` and `VacationHomePolicyDb`, unlike runtime `PolicyDb` (SQL Express). | If EF design-time tooling is introduced/used, change the factory to the intended environment-safe configuration and verify the actual target before running tools. | Required if using EF tooling; otherwise do not use it as-is. | Log/inspect resolved server and database in a safe design-time test; never infer target from runtime appsettings. |
| `VacationHome.PolicyService.Infrastructure/VacationHome.PolicyService.Infrastructure.csproj` | Policy EF provider/tooling packages. | SQL Server and EF Design packages are `8.0.31`. | Keep versions compatible; review whether tooling targets the same runtime model. | Conditional. | Restore/build and check generated SQL against the intended provider. |
| `VacationHome.Quote.Api/appsettings.json` | Defaults for the database-backed Product/Quote API. | `ConnectionStrings:ProductQuoteDb` targets `VacationHomeProductQuoteDb` on local Express with Windows auth and MARS. | Review/override `ProductQuoteDb`; separately set company Customer/Policy internal service URLs if they differ. | Required if this is the deployed API. | Verify API process loads this file/project and make an approved products/quotes DB-backed request in staging. |
| `VacationHome.Quote.Api/appsettings.Development.json` | Product/Quote Development overrides. | Logging only; no connection string override. | Do not put company secrets here. Note that the startup initializer runs only when environment is Development. | Required environment check; normally no file change. | Confirm the runtime environment name and effective ProductQuoteDb. |
| `VacationHome.Quote.Api/Program.cs` | Database-backed Product/Quote API startup. | Registers infrastructure, and only in Development runs `InitializeProductQuoteDatabaseAsync()`. | Do not start in Development against an existing company database. Determine how schema and reference data will be deployed when not using the Development initializer. | Required before deployment. | Inspect actual process environment and confirm the initializer is not inadvertently run against company data. |
| `VacationHome.Quote.Infrastructure/DependencyInjection.cs` | Product/Quote EF Core registration. | Reads `ProductQuoteDb`, uses SQL Server, configures a migrations assembly, and enables retries. | Verify key, assembly, login grants, MARS need, and target provider. | Required if this implementation is deployed. | Confirm resolved connection and execute a read-only ProductQuote operation in staging. |
| `VacationHome.Quote.Infrastructure/Persistence/Context/ProductQuoteDbContext.cs` | Product, quote, pricing, coverage, audit, and history EF model. | Defines tables and applies configuration classes from its assembly. | Use as the Product/Quote schema reference; preserve stored quote/policy snapshots and approval/audit data. | Required for schema preparation. | Compare approved schema with staging metadata and verify existing records are unchanged by deployment. |
| `VacationHome.Quote.Infrastructure/Persistence/ProductQuoteDatabaseInitializer.cs` | Development database creation, destructive compatibility SQL, and seed invocation. | Calls `EnsureCreatedAsync`; updates every `Quotes.Status = 1` to `2`; drops alternative-quote indexes/constraints and columns (`QuoteGroupId`, `OptionNumber`, `AlternativeRequestId`, `SelectedQuoteId`); then seeds reference data. | Do not run against a company database or historical quotes without explicit approval, backup, and a separately reviewed migration plan. Avoid Development environment on a company target. | Required safety review; do not run as a routine migration. | On a disposable database only, review SQL and inspect affected rows/objects; compare quote status counts and legacy schema before/after. |
| `VacationHome.Quote.Infrastructure/Seed/ProductQuoteSeeder.cs` | Product/Quote catalog, coverage, pricing, and risk reference data. | Called by the Development initializer. Each seed stage generally skips its whole table if any row exists; it is not a reconciliation/upsert migration. | Decide how reference data is provisioned and versioned. Review seed identifiers and ensure partial tables are handled deliberately. | Required if a fresh Product/Quote DB needs catalog data. | Verify expected products, coverages, product mappings, pricing and risk configuration in an isolated test DB. |
| `VacationHome.Quote.Infrastructure/Persistence/ProductQuoteDbContextFactory.cs` | Product/Quote design-time EF factory. | Reads `ProductQuoteDb` and environment variables; searches for a path named `VacationHome.ProductQuote.API` then falls back to its current directory. | Confirm its base path resolves to the database-backed API settings. Reconcile the project-folder/name mismatch before using EF commands. | Required if EF tooling is used. | Run only a non-mutating design-time context inspection and confirm resolved server/catalog before any script generation. |
| `VacationHome.Quote.Infrastructure/VacationHome.ProductQuote.Infrastructure.csproj` | Product/Quote SQL Server and EF Design dependencies. | SQL Server and EF Design packages are `8.0.31`. | Keep compatible with the selected .NET/EF tool versions; ensure this is the infrastructure project used by the deployed API. | Required project reconciliation; package change conditional. | Build the intended project and inspect its dependency graph. |
| `VacationHome.ApiGateway/appsettings.json` | Gateway security, JWT, CORS and logging settings. | No database connection. Contains local frontend CORS origins and development JWT configuration. | Update company origins/secrets as appropriate; do not place DB credentials here. | Conditional for company-hosted UI/API. | Verify CORS and JWT issuer/audience/key consistency without exposing secrets. |
| `VacationHome.ApiGateway/ocelot.json` | Routes gateway endpoints to backend services. | Downstream hosts are `localhost` with local HTTPS ports; routes include database-backed APIs but the Gateway does not access SQL. | Update service host/port/scheme only if company topology differs. | Conditional. | Call through the gateway in staging and confirm each route reaches the intended service. |
| `VacationHome.ApiGateway/ocelot.Development.json` | Development gateway base URL. | Sets `GlobalConfiguration:BaseUrl` to `http://localhost:7000`. | Keep local configuration local; configure company URL in the proper deployment environment if needed. | Conditional. | Verify gateway public URL and route health. |
| `../VacationHomeInsuranceUI/src/environments/environment.ts` | Angular API Gateway URL. | `apiGatewayBaseUrl` is `http://localhost:7000`. | Change/build-time configure only if the company laptop uses a different Gateway address; no SQL settings belong here. | Conditional for company UI use. | Load the deployed UI and verify API requests target the company Gateway, not localhost. |
| `VacationHome.PolicyService.Tests/PolicyDocumentTests.cs` | Policy document/model tests. | Contains a `UseSqlServer` test context configured with `Server=localhost;Database=PolicyModelTest`. | Check whether a test actually opens a connection before changing it. Keep tests isolated from company data; use a test-only database/container if integration access is required. | Conditional for tests on the company laptop. | Run only focused tests using an isolated test resource and verify no company database is addressed. |

The API Gateway and Angular entries above are included because their host/route settings affect end-to-end verification, not because they contain SQL settings. There is no gateway-to-database connection.

## Database creation, schema, and seed behavior

### What the repository currently uses

- The five service databases use EF Core `UseSqlServer`.
- No EF Core `Migrations` directories or checked-in `.sql` deployment scripts were found in the inspected source.
- No `Database.Migrate()` call was found.
- Auth, Customer, Claim, and Policy API startup paths call `EnsureCreatedAsync()` in all environments.
- Claim startup additionally adds `dbo.Claims.PolicyNumber` if it is missing.
- Policy startup additionally runs `PolicyDocumentSchemaMigration.ApplyAsync()` on every startup.
- The database-backed Product/Quote API calls `EnsureCreatedAsync()`, the quote status/schema cleanup SQL, and its seeder only when the app environment is `Development`.
- Design-time tooling does not consistently resolve the same server as runtime: Policy has a hardcoded LocalDB factory, and Product/Quote has a folder-name/fallback path that needs validation.

`EnsureCreated` is intended for creating a schema in a new database; it does not compare an existing database with later model changes or apply incremental schema changes. It must not be used as a replacement for EF migrations against an existing company database. The codebase has no checked-in migration history to apply.

### Startup side effects and historical-data risks

1. **Product/Quote initializer:** Development startup changes all `Quotes.Status` values of `1` to `2`, removes two alternative-quote indexes and associated constraints, and drops four alternative-quote columns when present. This can alter historical quote statuses and permanently discard legacy alternative-quote data. It is not safe to run merely to create a company database.
2. **Policy schema updater:** Adds missing snapshot columns using `NOT NULL` defaults with `WITH VALUES`, which fills existing rows with empty/zero/default values. It creates `PolicyDocuments` and unique indexes. Existing duplicate data could make a unique-index creation fail. This changes schema and existing row values.
3. **Claims startup SQL:** Adds `PolicyNumber` with a default to existing claims if the column is missing.
4. **Auth seeding:** Startup ensures role records and creates a hardcoded sample administrator if absent. Do not expose this account or sample credentials on the company environment.
5. **Product/Quote reference seeding:** Runs in Development only. It skips a seed category if its table has any rows, so partial reference data is not automatically repaired.
6. **Auth, Customer, Claim, and Policy `EnsureCreated`:** May create a new database schema but does not upgrade an existing schema to match current entity models.

Do not execute these initializers or migration-like commands against a company database during discovery. Before any company change:

1. Have the DBA identify the source and target database state and approve whether the work is a fresh installation, a restore/copy, or an in-place migration.
2. Back up the source and target; test restore/recovery. Use a separate staging database and least-privilege test account.
3. Reconcile the Product/Quote solution/project discrepancy and identify the exact source revision and published service binaries.
4. Have the engineering owner and DBA derive a reviewed schema deployment plan from the actual DbContext models and raw SQL. The repository currently has no standard EF migrations or ready-to-apply SQL scripts; do not improvise by starting services against the target.
5. Treat schema deployment, historical-data transfer, reference-data loading, and application startup as separate approved steps. Preserve quote/policy snapshots, payment history, audit records, and identities. Review the Product/Quote status update and dropped columns separately; do not apply them unless their business/data impact is explicitly approved.
6. Seed reference data through an explicit, reviewed, repeatable procedure. Do not rely on partial-table-skipping behavior as a production data migration.
7. Validate on the restored staging copy, take a fresh backup, and obtain DBA approval before applying to any production-like database.

If the company target is a brand-new disposable development database, use a separately approved provisioning process and verify exactly which service startup code will execute. That does not make `EnsureCreated` suitable for an existing database.

## Company laptop checklist

### 1. Software and access

- Install the supported .NET SDK/runtime for the intended .NET 8 services, the required SQL Server client/provider dependencies restored from the project, and the frontend runtime only if running Angular locally.
- Obtain the company VPN/network route, server/instance and port, database names, approved authentication method, TLS certificate chain, and service identity from the DBA.
- Confirm whether SQL Server remote TCP connections are enabled and whether firewall rules permit the laptop/service host to reach the SQL port.
- Confirm a least-privilege login/role per service database. Application runtime permissions should not include database/schema administration unless specifically required and approved.

### 2. Identify the SQL instance and authentication

- Ask the DBA for the canonical DNS name, instance/port, database/catalog name per service, authentication mode, encryption requirements, certificate authority, and connection timeout policy.
- Verify the service process identity for integrated authentication; a successful desktop SSMS login under a different user does not prove the backend service can connect.
- Do not guess server, database, domain principal, or whether SQL Authentication is allowed.

### 3. Configure each backend separately

- Supply the appropriate company value for `DefaultConnection`, `CustomerDb`, `ClaimDb`, `PolicyDb`, or `ProductQuoteDb` to the matching API process using protected environment/deployment configuration.
- Preserve the local Express values for local developers; avoid editing a shared checked-in file just to configure one company laptop.
- Review the two current MARS settings and use them only where needed.
- Configure non-database secrets (JWT signing keys and any internal-service credentials) through the approved secret mechanism. The checked-in JWT keys are development examples.
- Confirm the API's `ASPNETCORE_ENVIRONMENT`; the Product/Quote initializer is Development-only, while other startup DB operations are not environment-gated.

### 4. Prepare schema and reference data safely

- Decide with the DBA whether to provision five separate service databases as currently configured or intentionally redesign database ownership.
- For an existing database, back up and restore to staging, inspect schema/data, and use an approved incremental migration plan; do not use `EnsureCreated` to patch it.
- For a new database, reconcile the actual Product/Quote project first, then produce and review a schema deployment from the selected DbContext models. The repository has no checked-in migrations or SQL scripts, so a DBA-approved schema artifact/process must be established before company deployment.
- Apply schema changes through the approved deployment process, not by accidentally starting APIs with broad database permissions.
- Seed only the reference and identity data approved for the target. Review default-admin creation, existing data, seed-table completeness, and uniqueness constraints first.
- Keep schema deployment, data migration, seeding, and application startup as separate steps with recorded approvals and backups.

### 5. Verify each backend database connection

- Start/test each service only in the approved staging environment with its own connection string and least-privilege identity.
- Check the service logs for the selected database, authentication, TLS, and SQL errors; never log full credentials.
- `/health` is registered as an application health endpoint, but the code does not register a database health check. A successful health response alone does not prove SQL connectivity.
- Make a safe read-only request that exercises each service's EF query after connectivity is established. Use a controlled write only when needed and only in staging, with a cleanup plan.
- Verify Auth, Customer, Claim, Policy, and Product/Quote independently; success of one does not validate the others.

### 6. Verify API Gateway and Angular

- Confirm Ocelot downstream host, port, scheme, certificates, service URLs, Gateway CORS origins, JWT issuer/audience, and Gateway public URL for the company topology.
- Update the Angular API Gateway base URL through the intended build/deployment configuration if it is not `http://localhost:7000`. Do not place database credentials in Angular.
- Test safe product/catalog reads, customer/authentication flows, quote reads/generation, claim and policy read paths through the Gateway using test accounts/data.
- Confirm quote acceptance/payment/policy flow only in a non-production test environment; verify approved prices and historical snapshots are not being recalculated.

### 7. Troubleshoot common failures

| Symptom | Checks |
|---|---|
| Cannot reach server / timeout | VPN/DNS, instance name and TCP port, SQL Server TCP/IP and remote access, firewall, routing, and configured timeout. |
| Login failed / cannot open database | Verify exact database name, that it exists and is online, the login's database mapping, service process identity, and database permissions. A running SQL Server service alone does not prove the database or login is usable. |
| Invalid object/table or missing column | Compare schema to the intended DbContext and approved schema version. Do not rerun `EnsureCreated` as a repair or run the Development quote initializer against company data. |
| Permission denied | Identify the failing statement and grant only the minimum required role/permission; startup initializers may require DDL and should not run under the permanent application runtime identity. |
| TLS/certificate or pre-login handshake error | Confirm server certificate name, trusted issuing CA, encryption requirements, client/provider version, and DBA-approved trust settings. Do not turn off certificate validation as a workaround. |
| Provider/retry-limit error | Read the innermost SQL exception. Retries do not fix wrong catalog, missing permissions, schema mismatch, or certificate trust. |
| Service starts but DB-backed route fails | Check the service's effective connection string source, the actual process identity, startup logs, and whether the request reaches the expected API project. Gateway or `/health` success does not prove database access. |

### 8. Keep secrets out of source control

- Do not commit SQL passwords, production JWT keys, private certificates, or company connection details.
- Use an approved company secret manager or protected deployment environment variables. Restrict access and rotate credentials under company policy.
- Use least-privilege, service-specific database credentials where possible; do not reuse a human DBA account for a running application.
- Avoid putting credentials in command-line arguments, terminal history, screenshots, logs, Angular configuration, or unprotected `appsettings.*.json`.

## Confirmed findings and unknowns

**Confirmed:** The checked-in runtime service configurations use SQL Server Express/Windows authentication and five distinct database names. All five infrastructure projects use EF Core SQL Server. The database-backed Product/Quote service has the destructive Development initializer; Claim and Policy have startup raw SQL; Auth has startup role/admin seeding. No EF migration folders, `Database.Migrate()` calls, or checked-in `.sql` files were found. Policy design-time tooling hardcodes LocalDB. Gateway/frontend configuration is separate from SQL.

**Environment-dependent/unverified:** Company server/instance/port, database catalogs, Windows versus SQL login support, TLS certificate and encryption policy, firewall/VPN, service identity, target database/schema/data state, required grants, intended Product/Quote solution/publish project, and external service URLs. User-level secrets and company deployment configuration outside the repository were not inspected. The live company database state has not been queried.

No database commands, startup initializers, migrations, or data changes were run while preparing this guide.
