# Toll Calculator — Implementation Tasks

Rewrite of the forked assignment as a DDD-structured .NET solution: SQL Server + EF Core,
REST API over HTTPS, documented with Scalar, protected by a bearer token (anonymous in Development).

The project structure follows `Fg.Integration`: `src/` and `test/` folders, `net10.0` set per project,
`Registrations/ServiceCollectionExtensions.cs` per layer, interfaces beside an `Implementations/`
subfolder, versioned controllers, and `OpenApi/Transformers` feeding Scalar.

Each task below is sized to **one git commit**. Work them in order.

---

## Ground rules (apply to every task)

- **No enums in `TollCalculator.WebApi.Models`** — categorical fields are `string` in request/response models.
  Enums and value objects are fine inside Domain.
- **Minimal comments.** Name things well instead.
- **Tests ship with the code they cover.** A task that adds a service adds its unit tests in the same commit.
- **`appsettings` holds infrastructure and secrets only** — connection string, JWT settings, Kestrel/HTTPS.
  Toll rates, the daily cap, toll-free vehicle types and holidays are EF-seeded data, not configuration.
- **HTTPS always** — `UseHttpsRedirection` in every environment, HSTS outside Development.
- Leave the existing `C#/` and `Java/` folders untouched; they are the legacy reference.

## Target layout

```
toll-calculator/
  TollCalculator.slnx
  src/
    TollCalculator.Domain/           Models, Enums, Constants, Results, Services/Implementations
    TollCalculator.Application/      Services/Implementations, Mappers, Registrations
    TollCalculator.Infrastructure/   Persistence, Repositories/Implementations, Registrations
    TollCalculator.WebApi.Models/    V1/Requests, V1/Responses, Constants
    TollCalculator.WebApi.Host/      Controllers/V1, Auth, Configuration, OpenApi, Mappers, Validators
  test/
    TollCalculator.Domain.UnitTests/
    TollCalculator.Application.UnitTests/
    TollCalculator.Infrastructure.UnitTests/
    TollCalculator.WebApi.Host.IntegrationTests/
  C#/  Java/                         legacy reference, untouched
```

Dependency direction, as in `Fg.Integration`: Infrastructure → Domain; Application → Infrastructure +
Domain; WebApi.Host → WebApi.Models + Application + Domain. Domain references nothing.

## Toll schedule (from the assignment README)

| Time          | Fee    |
|---------------|--------|
| 06:00–06:29   | 8 SEK  |
| 06:30–06:59   | 13 SEK |
| 07:00–07:59   | 18 SEK |
| 08:00–08:29   | 13 SEK |
| 08:30–14:59   | 8 SEK  |
| 15:00–15:29   | 13 SEK |
| 15:30–16:59   | 18 SEK |
| 17:00–17:59   | 13 SEK |
| 18:00–18:29   | 8 SEK  |
| 18:30–05:59   | 0 SEK  |

Daily cap: **60 SEK**. A vehicle is charged at most once per 60-minute window; the highest fee in that
window applies. Toll-free vehicle types: Motorbike, Tractor, Emergency, Diplomat, Foreign, Military.
Weekends and holidays are free.

## Bugs in the original `C#/TollCalculator.cs` that the rewrite must fix

1. `:25` — the hour window is computed from `DateTime.Millisecond`, not a real time difference.
2. `:66` — `hour >= 8 && hour <= 14 && minute >= 30` never charges 8 SEK for e.g. 09:05.
3. `:68` — `hour == 15 && minute >= 0 || hour == 16 && minute <= 59` mixes `&&` and `||` without
   parentheses. It happens to return the right amount only because the preceding branch catches
   15:00–15:29 first; reorder the branches and 15:00 silently becomes 18 SEK.
4. `:82` — holidays are hardcoded to the year 2013.
5. `GetTollFee` assumes `dates` is non-empty and already sorted; no input validation.
6. Toll-free vehicles are matched by string comparison against a private enum.

---

# Phase 0 — Project setup

## Task 1 — Create the solution and projects

Project skeleton only. No application code, no packages beyond the test SDK — later tasks add those.

- `dotnet new sln -n TollCalculator` (SDK 10 emits `.slnx`) at the repo root.
- Class libraries under `src/`: `TollCalculator.Domain`, `TollCalculator.Application`,
  `TollCalculator.Infrastructure`, `TollCalculator.WebApi.Models`.
- Web API project under `src/`: `TollCalculator.WebApi.Host` (`Microsoft.NET.Sdk.Web`).
- xUnit projects under `test/`: `TollCalculator.Domain.UnitTests`, `TollCalculator.Application.UnitTests`,
  `TollCalculator.Infrastructure.UnitTests`, `TollCalculator.WebApi.Host.IntegrationTests`.
- Every project: `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`,
  `<ImplicitUsings>enable</ImplicitUsings>`. The host also gets `<InvariantGlobalization>true</InvariantGlobalization>`.
- Project references per the dependency direction above; each unit-test project references its own source
  project, and the source project declares `<InternalsVisibleTo Include="...UnitTests" />`.
- Delete the generated `Class1.cs` / `WeatherForecast` sample files.
- Add a `.gitignore` from the standard Visual Studio template.
- Add every project to the solution.

Files: `TollCalculator.slnx`, `src/*/*.csproj`, `test/*/*.csproj`, `.gitignore`

**Done when:** `dotnet build` and `dotnet test` both succeed (0 tests), and
`TollCalculator.Domain.csproj` contains no `ProjectReference`.

---

# Phase 1 — Domain

## Task 2 — Vehicle model

- `Domain/Enums/VehicleType.cs`: `Car`, `Motorbike`, `Tractor`, `Emergency`, `Diplomat`, `Foreign`, `Military`.
- `Domain/Models/Vehicle.cs`: `RegistrationNumber` (normalised — uppercase, trimmed) plus `VehicleType`,
  and `IsTollFree`.
- `Domain/Constants/TollFreeVehicleTypes.cs`: the six free types in one place.
- Guard clause: an empty registration number throws.

Files: `src/TollCalculator.Domain/{Enums,Models,Constants}/*.cs`,
`test/TollCalculator.Domain.UnitTests/Models/VehicleTests.cs`

**Done when:** unit tests cover normalisation, equality, and `IsTollFree` for all seven types.

## Task 3 — TollPassage model

- `Domain/Models/TollPassage.cs`: `Id`, `RegistrationNumber`, `PassedAt` (`DateTimeOffset`).
- Factory method validating a non-empty registration number.

Files: `src/TollCalculator.Domain/Models/TollPassage.cs`,
`test/TollCalculator.Domain.UnitTests/Models/TollPassageTests.cs`

**Done when:** tests cover a valid passage and the invalid-input throw.

## Task 4 — Toll schedule

- `Domain/Models/TollRateInterval.cs`: `Start` (`TimeOnly`), `End` (inclusive), `Amount`, `Covers(TimeOnly)`.
- `Domain/Models/TollSchedule.cs`: the ordered intervals plus `FeeFor(TimeOnly)`, returning 0 when nothing
  matches. The constructor validates that intervals do not overlap.

Files: `src/TollCalculator.Domain/Models/{TollRateInterval,TollSchedule}.cs`,
`test/TollCalculator.Domain.UnitTests/Models/TollScheduleTests.cs`

**Done when:** tests assert every boundary minute in the schedule table above (05:59, 06:00, 06:29, 06:30,
08:29, 08:30, 09:05, 14:59, 15:00, 15:29, 15:30, 16:59, 18:29, 18:30) — this is where original bugs 2 and 3 die.

## Task 5 — Toll-free dates

- `Domain/Models/Holiday.cs`: `Date` (`DateOnly`), `Name`.
- `Domain/Services/ITollFreeDateService.cs` + `Services/Implementations/WeekendAndHolidayService.cs`:
  `IsTollFree(DateOnly)` over the holiday set, year-agnostic (fixes original bug 4).

Files: `src/TollCalculator.Domain/Models/Holiday.cs`, `src/TollCalculator.Domain/Services/**`,
`test/TollCalculator.Domain.UnitTests/Services/WeekendAndHolidayServiceTests.cs`

**Done when:** tests cover Saturday, Sunday, a seeded holiday, and an ordinary weekday — in two different years.

## Task 6 — Single-passage fee

- `Domain/Services/ITollFeeService.cs` + `Implementations/TollFeeService.cs`: given a `Vehicle`, a
  `DateTimeOffset`, the `TollSchedule` and the toll-free date service, return the fee for that one passage.
- Returns 0 for toll-free vehicles and toll-free dates before consulting the schedule.

Files: `src/TollCalculator.Domain/Services/**`,
`test/TollCalculator.Domain.UnitTests/Services/TollFeeServiceTests.cs`

**Done when:** tests cover a charged weekday passage, a toll-free vehicle, a weekend, and a holiday.

## Task 7 — Daily toll calculation

- `Domain/Services/IDailyTollService.cs` + `Implementations/DailyTollService.cs`: given a vehicle and all
  passages for one day, return the day's total plus a breakdown of which passages were charged.
- `Domain/Models/DailyToll.cs` for that result.
- Rules: sort the input defensively; empty input returns 0 (fixes bug 5); a 60-minute window opened by the
  first charged passage, with the highest fee in that window applying (fixes bug 1); cap the day at 60 SEK.

Files: `src/TollCalculator.Domain/Services/**`, `src/TollCalculator.Domain/Models/DailyToll.cs`,
`test/TollCalculator.Domain.UnitTests/Services/DailyTollServiceTests.cs`

**Done when:** tests cover — empty list; unsorted input; two passages 20 minutes apart charging only the
higher; two passages 70 minutes apart charging both; a day exceeding 60 SEK capped at 60; a toll-free
vehicle; a holiday.

---

# Phase 2 — Infrastructure

## Task 8 — DbContext and entity configurations

- `Infrastructure/Persistence/TollDbContext.cs` with `DbSet`s for `Vehicles`, `TollPassages`,
  `TollRateIntervals`, `Holidays`, `TollSettings`.
- One `IEntityTypeConfiguration<T>` per entity under `Persistence/Configurations`: keys, max lengths, a
  unique index on `TollPassages(RegistrationNumber, PassedAt)`, `TimeOnly`/`DateOnly` conversions.
- SQL Server provider; connection string read from `ConnectionStrings:TollDb`.

Files: `src/TollCalculator.Infrastructure/Persistence/**`

**Done when:** `dotnet ef dbcontext info` resolves the context.

## Task 9 — Seed data

- `HasData` for: the nine rate intervals from the schedule table; the 60 SEK daily cap row in `TollSettings`;
  the seven vehicle types with their toll-free flag; Swedish public holidays for the current and next year.
- Seed values live as constants in one file so the table above and the code cannot drift apart.

Files: `src/TollCalculator.Infrastructure/Persistence/Seed/*.cs`

**Done when:** the seed data appears in the migration from Task 11 and the row counts match the table above.

## Task 10 — Repositories

- Interfaces in `Infrastructure/Repositories`, EF implementations in `Repositories/Implementations`:
  `IVehicleRepository`, `ITollPassageRepository`, `ITollScheduleRepository`, `IHolidayRepository`.
- Async throughout with a `CancellationToken` on every method; `AsNoTracking` on reads.
- The daily-passage query filters on the date in the service's time zone, not UTC-naively.

Files: `src/TollCalculator.Infrastructure/Repositories/**`,
`test/TollCalculator.Infrastructure.UnitTests/Repositories/*.cs`

**Done when:** every interface has exactly one implementation, with tests over an in-memory/SQLite context.

## Task 11 — Migration and infrastructure registration

- `Infrastructure/Registrations/ServiceCollectionExtensions.cs` → `AddInfrastructure(IConfiguration)`,
  registering the DbContext and repositories.
- `dotnet ef migrations add InitialCreate -p src/TollCalculator.Infrastructure -s src/TollCalculator.WebApi.Host`.
- Apply migrations on startup **in Development only**.

Files: `src/TollCalculator.Infrastructure/Registrations/ServiceCollectionExtensions.cs`,
`src/TollCalculator.Infrastructure/Migrations/*`

**Done when:** running the host against a local SQL Server creates the schema and the seeded rows.

---

# Phase 3 — Application

## Task 12 — Toll passage service

- `Application/Services/ITollPassageService.cs` + `Implementations/TollPassageService.cs`: `RecordTollPassageAsync`
  looks up (or rejects) the vehicle, creates a `TollPassage`, persists it.
- Returns an application result carrying the new passage id or a not-found error.

Files: `src/TollCalculator.Application/Services/**`,
`test/TollCalculator.Application.UnitTests/Services/TollPassageServiceTests.cs`

**Done when:** tests (NSubstitute fakes) cover the happy path and the unknown-vehicle failure.

## Task 13 — Toll service

- `Application/Services/ITollService.cs` + `Implementations/TollService.cs`:
  - `GetDailyTollAsync` — load the vehicle, that day's passages, the schedule and the holidays; run the
    domain `IDailyTollService`; return the total plus the breakdown.
  - `CalculateAsync` — stateless: vehicle type and timestamps come from the caller, nothing is persisted.

Files: `src/TollCalculator.Application/Services/**`,
`test/TollCalculator.Application.UnitTests/Services/TollServiceTests.cs`

**Done when:** tests cover a day with passages, a day with none, an unknown vehicle, an unknown vehicle-type
string, and an empty timestamp list.

## Task 14 — Reference-data service

- `Application/Services/IReferenceDataService.cs` + implementation: toll rates, holidays filtered by year,
  toll-free vehicle types.

Files: `src/TollCalculator.Application/Services/**`,
`test/TollCalculator.Application.UnitTests/Services/ReferenceDataServiceTests.cs`

**Done when:** each read has a unit test asserting it returns what the repository gave it.

## Task 15 — Application registration

- `Application/Registrations/ServiceCollectionExtensions.cs` → `AddApplication()`, registering the
  application services and the domain services from Phase 1.

Files: `src/TollCalculator.Application/Registrations/ServiceCollectionExtensions.cs`

**Done when:** a test resolves every service from a `ServiceCollection` with fake repositories registered.

---

# Phase 4 — API

## Task 16 — Request/response contracts

- `WebApi.Models/V1/Requests`: `RecordTollPassageRequest`, `CalculateTollRequest`, `TokenRequest`.
- `WebApi.Models/V1/Responses`: `RecordTollPassageResponse`, `DailyTollResponse` (plus `ChargedTollPassage`),
  `CalculateTollResponse`, `TokenResponse`, `TollRateResponse`, `HolidayResponse`, `VehicleTypeResponse`.
- **Vehicle type and every other categorical field is a `string`.** No enums in this project.
- Mappers between contracts and domain models in `WebApi.Host/Mappers/Implementations`.

Files: `src/TollCalculator.WebApi.Models/V1/**`, `src/TollCalculator.WebApi.Host/Mappers/**`

**Done when:** `grep -r "enum" src/TollCalculator.WebApi.Models` returns nothing.

## Task 17 — Host, HTTPS and error handling

- `Program.cs`: `AddApplication()`, `AddInfrastructure()`, controllers, API versioning
  (`Asp.Versioning.Mvc.ApiExplorer`), a problem-details exception handler, a health endpoint.
- HTTPS: Kestrel HTTPS endpoint, `UseHttpsRedirection()` unconditionally, `UseHsts()` outside Development.
- `appsettings.json` and `appsettings.Development.json` with `ConnectionStrings` and a `Jwt` section **only**.
- `Configuration/JwtOptions.cs` bound from configuration and validated on start.

Files: `src/TollCalculator.WebApi.Host/Program.cs`, `src/TollCalculator.WebApi.Host/Configuration/*.cs`,
`src/TollCalculator.WebApi.Host/appsettings*.json`, `src/TollCalculator.WebApi.Host/Properties/launchSettings.json`

**Done when:** `dotnet run` starts on `https://localhost:7xxx`, the health endpoint returns 200, and plain
HTTP redirects.

## Task 18 — Toll passage controller

- `Controllers/V1/TollPassagesController.cs`:
  - `POST /api/v1/toll-passages` → 201 with the passage id; 400 on bad input; 404 on an unknown vehicle.
  - `GET /api/v1/vehicles/{registrationNumber}/toll?date=yyyy-MM-dd` → `DailyTollResponse`.
- Request validation in `Validators/Implementation`.

Files: `src/TollCalculator.WebApi.Host/Controllers/V1/TollPassagesController.cs`,
`src/TollCalculator.WebApi.Host/Validators/**`

**Done when:** both routes appear in the OpenAPI document with their response types.

## Task 19 — Toll and reference controllers

- `Controllers/V1/TollController.cs`: `POST /api/v1/toll/calculate`.
- `Controllers/V1/ReferenceController.cs`: `GET /api/v1/toll-rates`, `GET /api/v1/holidays?year=`,
  `GET /api/v1/vehicle-types`.

Files: `src/TollCalculator.WebApi.Host/Controllers/V1/*.cs`

**Done when:** all four routes return seeded data from a running instance.

## Task 20 — Bearer authentication

- `Auth/` — `AddAuthentication().AddJwtBearer()` with the symmetric key, issuer and audience from `JwtOptions`.
- A fallback authorization policy requiring an authenticated user for every endpoint.
- **In Development only**, swap the fallback policy for one that allows anonymous, so Scalar works without a
  token locally. The health and token endpoints stay anonymous everywhere.

Files: `src/TollCalculator.WebApi.Host/Auth/**`, `src/TollCalculator.WebApi.Host/Program.cs`

**Done when:** `ASPNETCORE_ENVIRONMENT=Production` returns 401 without a token; Development returns 200.

## Task 21 — Token endpoint

- `Controllers/V1/AuthController.cs`: `POST /api/v1/auth/token` validates `TokenRequest` against credentials
  in the `Jwt` section and returns a signed JWT with an expiry.
- `Auth/JwtTokenIssuer.cs`. Anonymous, rate-limited.

Files: `src/TollCalculator.WebApi.Host/Controllers/V1/AuthController.cs`,
`src/TollCalculator.WebApi.Host/Auth/JwtTokenIssuer.cs`

**Done when:** the issued token is accepted by the protected endpoints in a non-Development run.

## Task 22 — OpenAPI and Scalar

- `OpenApi/Extensions/` — `AddOpenApi()` per API version plus `MapScalarApiReference()` at `/scalar`.
- `OpenApi/Transformers/BearerSecuritySchemeDocumentTransformer.cs` so Scalar's "Authorize" sends
  `Authorization: Bearer <token>`; `ApiVersionInfoDocumentTransformer.cs` for the title/version.
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` on the host so XML doc comments reach the
  document; controllers annotated with `ProducesResponseType`.

Files: `src/TollCalculator.WebApi.Host/OpenApi/**`, `src/TollCalculator.WebApi.Host/TollCalculator.WebApi.Host.csproj`

**Done when:** `https://localhost:7xxx/scalar` lists every endpoint and authorizing there reaches a
protected route.

---

# Phase 5 — Integration tests and documentation

## Task 23 — Integration test harness

- `IntegrationTestFactory : WebApplicationFactory<Program>` plus a Testcontainers `MsSqlContainer`.
- Migrations applied once per collection; a shared xUnit collection fixture; a helper to reset data between tests.
- `<InternalsVisibleTo Include="TollCalculator.WebApi.Host.IntegrationTests" />` on the host.

Files: `test/TollCalculator.WebApi.Host.IntegrationTests/Infrastructure/*.cs`

**Done when:** one smoke test hitting the health endpoint passes against the container.

## Task 24 — Toll flow integration tests

- Record a passage, then query the daily toll and assert the amount.
- Daily cap reached; toll-free vehicle returns 0; a passage on a seeded holiday returns 0.

Files: `test/TollCalculator.WebApi.Host.IntegrationTests/Tolls/*.cs`

**Done when:** all four scenarios pass end to end against real SQL Server.

## Task 25 — Auth integration tests

- A protected route without a token in a non-Development environment → 401.
- A token from `/api/v1/auth/token` → 200.
- Development environment → an anonymous call succeeds.

Files: `test/TollCalculator.WebApi.Host.IntegrationTests/Auth/AuthTests.cs`

**Done when:** all three pass.

## Task 26 — README rewrite

- Replace the assignment README with: what this is, an architecture overview and why DDD, how to run
  (SQL Server connection string, `dotnet dev-certs https --trust`, `dotnet run`), the HTTPS URLs,
  how to get a token, the Scalar URL, and how to run the tests.
- A "What was wrong with the original" section listing the six bugs above with the fix for each.

Files: `README.md`

**Done when:** a reader can clone, run and call the API using only the README.
