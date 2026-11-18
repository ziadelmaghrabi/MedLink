# MedLink Backend Progress Report

## A. Project Overview

MedLink is a university graduation project for healthcare-related operations. This report documents the backend implementation completed as of 2026-11-18.

The current backend scope is foundational:

- A .NET 10 ASP.NET Core Web API solution.
- A small domain layer containing initial Patient and Doctor entities.
- PostgreSQL persistence through Entity Framework Core.
- An initial database migration.
- A temporary database reachability endpoint.

The implemented architecture is a simplified three-layer structure:

- **API:** application entry point and HTTP exposure.
- **Core:** domain entities and enums.
- **Infrastructure:** EF Core context, entity mappings, migrations, and PostgreSQL registration.

Authentication, authorization, business workflows, clinical data, and API CRUD operations are not yet implemented.

## B. Development Environment

The following environment details were verified:

| Component | Verified value |
|---|---|
| Operating system | macOS, Darwin 27.0.0 |
| Architecture | ARM64 |
| .NET SDK | 10.0.301 |
| Target framework | `net10.0` |
| PostgreSQL client | 17.12, Homebrew build |
| PostgreSQL server | Accepting connections on `localhost:5432` |
| Git | 2.54.0 |
| Entity Framework Core | 10.0.4 |
| Npgsql provider | 10.0.3 |
| EF Core CLI | 10.0.4 |

The workspace is not currently inside a Git repository. Git status could therefore not provide source-control change information.

The EF Core CLI was not initially available and was subsequently installed as a global .NET tool. Entity Framework Core Design 10.0.4 is referenced by both MedLink.API and MedLink.Infrastructure for migration tooling.

## C. Solution Structure

The solution is at:

- `MedLink.sln`

Main source structure:

```text
backend/
├── MedLink.sln
├── MedLink.API/
│   ├── MedLink.API.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
├── MedLink.Core/
│   ├── Entities/
│   │   ├── Patient.cs
│   │   └── Doctor.cs
│   ├── Enums/
│   │   └── Gender.cs
│   └── MedLink.Core.csproj
└── MedLink.Infrastructure/
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   ├── Configurations/
    │   └── Migrations/
    ├── InfrastructureServiceCollectionExtensions.cs
    └── MedLink.Infrastructure.csproj
```

### MedLink.API

ASP.NET Core Web API entry point. It is responsible for configuration, dependency-injection composition, hosting, and HTTP endpoints.

It references:

- `MedLink.Core`
- `MedLink.Infrastructure`

### MedLink.Core

Contains domain entities and enums that do not depend on ASP.NET Core or EF Core infrastructure.

It currently has no project references.

### MedLink.Infrastructure

Contains EF Core infrastructure, including `ApplicationDbContext`, entity configurations, dependency-injection registration, and migrations.

It references:

- `MedLink.Core`

The resulting dependency direction is:

```text
MedLink.API -> MedLink.Core
MedLink.API -> MedLink.Infrastructure
MedLink.Infrastructure -> MedLink.Core
```

There is no circular project dependency.

## D. Database Setup

A PostgreSQL database named `medlink_db` was already provisioned locally and is reachable through:

- Host: `localhost`
- Port: `5432`

The database creation itself was not performed or modified by this implementation. No database, table, or migration was deleted or recreated.

The API reads the PostgreSQL connection string from ASP.NET Core configuration under:

```text
ConnectionStrings:DefaultConnection
```

For local Development use, the connection string is intended to come from ASP.NET Core User Secrets associated with `MedLink.API`. The secret identifier is stored in the API project file. The actual secret value and password are not stored in this report or in appsettings files.

The API project enables Development environment configuration by convention and supports the local secret store. `ASPNETCORE_ENVIRONMENT=Development` was used for EF migration commands.

No database credentials appear in:

- `appsettings.json`
- `appsettings.Development.json`

The exact secret setup command uses the secret key and a password supplied separately by the developer:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<developer-supplied PostgreSQL connection string>" --project MedLink.API/MedLink.API.csproj
```

Do not substitute a fake password or commit the resulting connection string.

### Infrastructure registration

`InfrastructureServiceCollectionExtensions.AddInfrastructure(IConfiguration)`:

1. Retrieves `ConnectionStrings:DefaultConnection`.
2. Throws an `InvalidOperationException` if it is missing or empty.
3. Calls `services.AddDbContext<ApplicationDbContext>()`.
4. Configures the context with `UseNpgsql(connectionString)`.

This registration is called from `Program.cs` at application startup.

## E. Entity Framework Core Setup

EF Core provides the object-relational mapping and migration support for the PostgreSQL database. Npgsql is the provider that translates EF Core operations to PostgreSQL.

### ApplicationDbContext

File:

- `MedLink.Infrastructure/Data/ApplicationDbContext.cs`

`ApplicationDbContext`:

- Inherits from `DbContext`.
- Accepts `DbContextOptions<ApplicationDbContext>`.
- Exposes `DbSet<Patient> Patients`.
- Exposes `DbSet<Doctor> Doctors`.
- Overrides `OnModelCreating`.
- Calls `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`.

The DbSets make the two mapped entities available through EF Core. `ApplyConfigurationsFromAssembly` discovers all `IEntityTypeConfiguration<T>` classes in the Infrastructure assembly, keeping mappings outside entity classes.

### EF Core packages

In `MedLink.Infrastructure/MedLink.Infrastructure.csproj`:

- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3.
- `Microsoft.EntityFrameworkCore.Design` 10.0.4.

In `MedLink.API/MedLink.API.csproj`:

- `Microsoft.EntityFrameworkCore.Design` 10.0.4, required for EF CLI tooling because MedLink.API is the startup project.

The EF Core CLI was verified as version 10.0.4.

## F. Implemented Entities and Enums

No additional entity or enum outside the following implementation was found.

### Patient

File:

- `MedLink.Core/Entities/Patient.cs`

| Property | C# type | Required or optional | Purpose |
|---|---|---|---|
| `Id` | `Guid` | Required | Primary key |
| `FirstName` | `string` | Required | Patient first name |
| `LastName` | `string` | Required | Patient last name |
| `DateOfBirth` | `DateOnly?` | Optional | Date without a time component |
| `Gender` | `Gender?` | Optional | One of the Gender enum values |
| `PhoneNumber` | `string?` | Optional | Patient phone number |
| `Address` | `string?` | Optional | Patient address |

The required string properties use empty-string initializers to avoid nulls before the entity is persisted. EF Core configuration defines which properties are database-required.

The entity contains no passwords, account relationships, medical history, diagnoses, prescriptions, or clinical notes.

### Doctor

File:

- `MedLink.Core/Entities/Doctor.cs`

| Property | C# type | Required or optional | Purpose |
|---|---|---|---|
| `Id` | `Guid` | Required | Primary key |
| `FirstName` | `string` | Required | Doctor first name |
| `LastName` | `string` | Required | Doctor last name |
| `PhoneNumber` | `string?` | Optional | Doctor phone number |
| `LicenseNumber` | `string` | Required | Professional license identifier |
| `Bio` | `string?` | Optional | Short doctor biography |
| `ConsultationFee` | `decimal` | Required | Consultation cost |
| `IsActive` | `bool` | Required | Whether the doctor profile is active |

The entity contains no authentication fields or account relationships. Identity and profile linkage are intentionally deferred.

### Gender

File:

- `MedLink.Core/Enums/Gender.cs`

The enum contains:

- `Male`
- `Female`
- `Other`
- `PreferNotToSay`

## G. Database Configurations

### PatientConfiguration

File:

- `MedLink.Infrastructure/Data/Configurations/PatientConfiguration.cs`

Configuration includes:

- `Guid Id` primary key.
- Required `FirstName` with maximum length 100.
- Required `LastName` with maximum length 100.
- Optional `DateOfBirth` mapped explicitly to PostgreSQL `date`.
- Optional `Gender` converted to a string with maximum length 20.
- Optional `PhoneNumber` with maximum length 32.
- Optional `Address` with maximum length 500.

Explicit string conversion keeps enum names stable and readable in the database rather than storing numeric enum values.

### DoctorConfiguration

File:

- `MedLink.Infrastructure/Data/Configurations/DoctorConfiguration.cs`

Configuration includes:

- `Guid Id` primary key.
- Required `FirstName` with maximum length 100.
- Required `LastName` with maximum length 100.
- Optional `PhoneNumber` with maximum length 32.
- Required `LicenseNumber` with maximum length 50.
- Unique index on `LicenseNumber`.
- Optional `Bio` with maximum length 2000.
- `ConsultationFee` precision 10 and scale 2.
- Required `IsActive` Boolean column.

The unique license index prevents duplicate license numbers. The `numeric(10,2)` mapping limits the fee to eight digits before the decimal point and two digits after it.

### Configuration discovery

Both configurations implement `IEntityTypeConfiguration<T>`. `ApplyConfigurationsFromAssembly` discovers them when the EF model is created.

## H. Initial EF Core Migration

Migration name:

- `InitialCreate`

Migration files:

- `MedLink.Infrastructure/Data/Migrations/20261118115640_InitialCreate.cs`
- `MedLink.Infrastructure/Data/Migrations/20261118115640_InitialCreate.Designer.cs`
- `MedLink.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`

The migration creates:

- `Doctors`
- `Patients`
- Primary keys on both tables.
- A unique PostgreSQL index named `IX_Doctors_LicenseNumber`.
- PostgreSQL `date` mapping for `Patients.DateOfBirth`.
- PostgreSQL `numeric(10,2)` mapping for `Doctors.ConsultationFee`.
- PostgreSQL string, Boolean, UUID, and nullable-column mappings.

The migration was generated with the Infrastructure project as the migrations project and MedLink.API as the startup project:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add InitialCreate \
  --project MedLink.Infrastructure/MedLink.Infrastructure.csproj \
  --startup-project MedLink.API/MedLink.API.csproj \
  --output-dir Data/Migrations
```

It was applied with:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project MedLink.Infrastructure/MedLink.Infrastructure.csproj \
  --startup-project MedLink.API/MedLink.API.csproj
```

EF Core created its migration-history table and applied `20261118115640_InitialCreate`.

Verified migration history:

```text
MigrationId: 20261118115640_InitialCreate
ProductVersion: 10.0.4
```

EF Core’s normal migration lock mechanism was used during application; no database objects were manually deleted or recreated.

## I. API and Database Connectivity

`Program.cs`:

1. Creates the ASP.NET Core application builder.
2. Calls `builder.Services.AddInfrastructure(builder.Configuration)`.
3. Builds the application.
4. Maps the temporary database health endpoint.
5. Runs the application.

### Database health endpoint

- Method: `GET`
- URL: `/health/database`

The endpoint resolves `ApplicationDbContext` and calls:

```csharp
await dbContext.Database.CanConnectAsync(cancellationToken)
```

It returns:

- HTTP 200 with `{"status":"healthy","database":"reachable"}` when the connection succeeds.
- HTTP 503 with `{"status":"unhealthy","database":"unreachable"}` when EF reports no successful connection.

On exceptions, the server logs an error and returns the same non-sensitive unhealthy response.

It does not expose the host, port, database name, username, password, or connection string.

Verified response:

```json
{"status":"healthy","database":"reachable"}
```

This proves that the API could load its configuration and reach PostgreSQL with the configured credentials at verification time. It does not prove that all future migrations, queries, authorization rules, or application features work.

## J. Build and Verification Results

| Check | Result |
|---|---|
| `dotnet restore MedLink.sln` | Verified successful; all projects up to date after restore |
| `dotnet build MedLink.sln` | Successful, 0 warnings, 0 errors |
| Solution project listing | MedLink.API, MedLink.Core, and MedLink.Infrastructure listed |
| Project references | Verified as specified in the architecture |
| PostgreSQL readiness | `localhost:5432` accepting connections |
| Migration listing | `20261118115640_InitialCreate` present |
| Migration applied | Verified in `__EFMigrationsHistory` |
| `Doctors` table | Verified in PostgreSQL |
| `Patients` table | Verified in PostgreSQL |
| Doctor license unique index | `IX_Doctors_LicenseNumber` verified as unique |
| Consultation fee | PostgreSQL type `numeric`, precision 10, scale 2 |
| Patient date of birth | PostgreSQL type `date`, nullable |
| Patient gender | PostgreSQL `character varying(20)`, nullable |
| Database health endpoint | Verified healthy and reachable |

No build warnings or errors are outstanding.

Authentication is not implemented, so a health endpoint may report database connectivity without validating an application account or role model.

## K. Architecture and Design Decisions

The following decisions are implemented:

1. **Simplified layered architecture:** API, Core, and Infrastructure remain separate for readability in a university project.
2. **Core owns domain types:** Patient, Doctor, and Gender live in MedLink.Core.
3. **Infrastructure owns persistence:** EF Core context, mappings, migrations, and PostgreSQL registration live in MedLink.Infrastructure.
4. **EF Core plus Npgsql:** EF Core provides standard mappings and migrations; Npgsql provides PostgreSQL support.
5. **No authentication fields in profiles:** Patient and Doctor contain only profile information.
6. **Deferred account relationships:** No navigation properties or Identity relationships were added.
7. **No unnecessary pattern catalog:** Repository Pattern, Unit of Work, CQRS, MediatR, Redis, Docker, and similar mechanisms were not introduced.
8. **No clinical implementation:** Medical history, diagnoses, prescriptions, and clinical notes remain outside the current scope.
9. **Secrets remain outside source code:** Database credentials are intended for ASP.NET Core User Secrets locally.

These decisions keep the project understandable and incremental while reserving account/authentication concerns for a separate design and implementation task.

## L. Current Project Status

| Area | Status | Notes |
|---|---|---|
| Solution scaffolding | Completed | All three projects created and targeting .NET 10 |
| PostgreSQL setup | Completed | Existing local database reachable; database name is `medlink_db` |
| EF Core integration | Completed | EF Core 10.0.4 and Npgsql 10.0.3 configured |
| Patient entity | Completed | Entity and EF configuration implemented |
| Doctor entity | Completed | Entity and EF configuration implemented |
| Gender enum | Completed | Four requested values implemented |
| Database configurations | Completed | Required fields, lengths, conversion, precision, and index configured |
| Initial migration | Completed | `InitialCreate` generated and applied |
| Database connectivity | Verified | API health endpoint returned healthy |
| Authentication | Not implemented | Identity/authentication model has not been selected or implemented |
| Authorization / roles | Not implemented | Roles and permissions are undecided |
| Patient APIs | Not implemented | No Patient controllers or endpoints added |
| Doctor APIs | Not implemented | No Doctor controllers or endpoints added |

## M. Pending Work and Next Steps

The following work is logical future scope but is not yet implemented:

1. Discuss and design the account model.
2. Decide whether to use ASP.NET Core Identity.
3. Define roles and permissions.
4. Decide how user accounts should connect to Patient and Doctor profiles.
5. Add authentication after the account model is agreed.
6. Add authorization and role-based or policy-based protections.
7. Continue adding domain entities and relationships incrementally.
8. Introduce Patient and Doctor API endpoints only after validation and authorization decisions are clear.
9. Add validation rules and automated tests.

Pending design decisions require discussion:

- Whether ASP.NET Core Identity is appropriate for this project.
- Whether the project should use custom account entities.
- Which roles exist.
- Whether patients and doctors should share a common account entity.
- Whether Doctor and Patient profile records are required before or after account creation.
- Which operations require authentication or specific roles.

These decisions should be made before editing authentication or relationship code.

## N. Important Development Commands

Run commands from the backend workspace unless stated otherwise.

### Check the .NET SDK

```bash
dotnet --version
dotnet --list-sdks
```

### Check PostgreSQL status

```bash
pg_isready -h localhost -p 5432
psql --version
```

### Build and restore

```bash
dotnet restore MedLink.sln
dotnet build MedLink.sln
```

### Run the API in Development

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project MedLink.API/MedLink.API.csproj
```

### Check database health

With the API running:

```bash
curl http://localhost:5000/health/database
```

Adjust the port if the runtime URL differs.

### EF CLI and migration commands

Set the EF tool on PATH for this shell if needed:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

List migrations:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations list \
  --project MedLink.Infrastructure/MedLink.Infrastructure.csproj \
  --startup-project MedLink.API/MedLink.API.csproj
```

Create a future migration:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add <MigrationName> \
  --project MedLink.Infrastructure/MedLink.Infrastructure.csproj \
  --startup-project MedLink.API/MedLink.API.csproj \
  --output-dir Data/Migrations
```

Apply migrations:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project MedLink.Infrastructure/MedLink.Infrastructure.csproj \
  --startup-project MedLink.API/MedLink.API.csproj
```

Do not include connection strings or passwords in shell history, source files, or commands shared with the project.

## O. Important Notes for the Next Development Session

Development stopped after the initial EF Core migration was generated and applied successfully.

The next discussion should focus on the account and authentication design before implementing Identity or profile relationships.

Before implementing any future authentication or role work, explicitly decide:

- Whether ASP.NET Core Identity is required.
- How accounts relate to Patient and Doctor records.
- Which roles and permissions are needed.
- Whether profile creation is coupled with account creation.

Do not implement authentication, authorization, Identity migrations, CRUD controllers, or clinical entities before these decisions are agreed.

The following files are important for the first follow-up review:

- `MedLink.Infrastructure/Data/ApplicationDbContext.cs`
- `MedLink.Infrastructure/Data/Configurations/PatientConfiguration.cs`
- `MedLink.Infrastructure/Data/Configurations/DoctorConfiguration.cs`
- `MedLink.Core/Entities/Patient.cs`
- `MedLink.Core/Entities/Doctor.cs`
- `MedLink.Core/Enums/Gender.cs`
- `MedLink.API/Program.cs`
- `MedLink.Infrastructure/InfrastructureServiceCollectionExtensions.cs`

Future work should remain incremental: inspect, explain, implement a small scope, build, migrate where intended, and verify before proceeding.
