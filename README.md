# Toll Calculator

A rewrite of the toll fee assignment as a .NET 10 REST API: SQL Server with EF Core, DDD-structured
projects, documented with Scalar, protected by a bearer token.

The original Java and C# implementations are left untouched in `Java/` and `C#/` as the reference this
was rewritten from. `C#/TollCalculator.cs` is also where the six bugs listed at the end of this file live.

## Toll rules

| Time        | Fee    |
|-------------|--------|
| 06:00–06:29 | 8 SEK  |
| 06:30–06:59 | 13 SEK |
| 07:00–07:59 | 18 SEK |
| 08:00–08:29 | 13 SEK |
| 08:30–14:59 | 8 SEK  |
| 15:00–15:29 | 13 SEK |
| 15:30–16:59 | 18 SEK |
| 17:00–17:59 | 13 SEK |
| 18:00–18:29 | 8 SEK  |
| 18:30–05:59 | 0 SEK  |

Each band runs from its start up to, **but not including**, the next one: a passage at 06:29:30 costs 8 SEK,
not nothing. The rows store the exclusive end (`06:00`–`06:30`), so there is no gap between the bands for a
timestamp with seconds to fall into.

A vehicle is charged at most once per 60-minute window, and the highest fee in that window applies.
Each day is capped at 60 SEK. Weekends and holidays are free, as are Motorbike, Tractor, Emergency,
Diplomat, Foreign and Military vehicles. Every timestamp is converted to Europe/Stockholm before the
schedule and the holiday calendar are consulted, so the same instant costs the same whether it arrives
as `07:15+01:00` or as `06:15Z`.

The rates are rows in the database, seeded by the initial migration, because a rate table is reference data
with a start and an end per row. The **daily cap is a single number**, so it lives in configuration —
`Toll:DailyCap` in `appsettings.json`, bound to `TollOptions` and validated on start-up — rather than in a
one-row table. The toll-free vehicle types are a domain constant, `TollFreeVehicleTypes`, so there is
exactly one source of truth for them. Holidays are not stored at all — they are resolved for any year
through `IHolidayProvider`, backed by the Nager.Date library.

## Architecture

```
src/
  TollCalculator.Domain/           Models, Enums, Constants — references nothing, holds no behaviour
  TollCalculator.Application/      Services, repository and provider interfaces, ApplicationResult
  TollCalculator.Infrastructure/   DbContext, configurations, seed, repository and provider implementations
  TollCalculator.WebApi.Models/    V1 request/response contracts        — references nothing
  TollCalculator.WebApi.Host/      Controllers, validators, mappers, auth, OpenAPI
test/
  TollCalculator.Domain.UnitTests/
  TollCalculator.Application.UnitTests/
  TollCalculator.Infrastructure.UnitTests/
  TollCalculator.WebApi.Host.IntegrationTests/
```

Dependencies point inward: Application → Domain, Infrastructure → Application → Domain. Neither
Application nor Domain knows that EF Core or Nager.Date exists — the repository and provider *interfaces*
live in `Application/Repositories/` and `Application/Services/`, and only `Infrastructure` implements them.
`WebApi.Host` is the composition root and references all of them.

**Where the rules live.** The interesting part of this problem is a handful of rules that interact — the
hourly window, the daily cap, toll-free dates, toll-free vehicle types. In the original they were tangled
into one method together with the data they operate on, which is why four of the six bugs are arithmetic
or operator-precedence mistakes hiding inside an if-chain.

They now live in `Application/Services/`, separated from the data they act on. `Domain` is deliberately
behaviour-free: models, enums and constants only, no interfaces and no services. That keeps every
abstraction in one layer — a reader looking for what the system *does* opens `Application` and finds all
of it, implementations included, rather than following an interface across a project boundary.

The rules are still testable without a database, because they depend on interfaces rather than on EF Core:
`DailyTollService` takes a vehicle type, the instants it passed and a tariff (the schedule plus the daily
cap), and returns the total plus which passages were charged — it asks for nothing it does not read, so no
entity and no registration number. It holds no state, so it is a singleton.

Every service returns `ApplicationResult<T>` rather than a bare value, so a failure anywhere in the chain
travels back to the controller as data instead of an exception, and `ApiControllerBase.MapErrorResponse`
turns it into the right status code.

Passages are grouped by Stockholm day before the hourly window and the daily cap are applied, so a set of
timestamps spanning several days is charged correctly rather than stopping at the first day's cap.

`TollCalculator.WebApi.Models` deliberately contains no enums. Vehicle type and every other categorical
field is a `string` on the wire, so adding a domain enum value cannot silently change the contract.

## Running it

You need the .NET 10 SDK and a SQL Server reachable at `localhost` with Windows authentication. If yours
differs, change `ConnectionStrings:TollDatabase` in `src/TollCalculator.WebApi.Host/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "TollDatabase": "Server=localhost;Database=TollCalculator;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Trust the local HTTPS certificate once, then run:

```bash
dotnet dev-certs https --trust
cd src/TollCalculator.WebApi.Host
dotnet run
```

The host listens on **https://localhost:7077** and http://localhost:5075; plain HTTP redirects to HTTPS.
In Development the database is created and migrated automatically on startup, including the seeded rates
— there is no separate migration step to run.

Check it is up:

```bash
curl -k https://localhost:7077/health          # Healthy
```

### API reference

Scalar is at **https://localhost:7077/scalar**, with the OpenAPI document at
`https://localhost:7077/openapi/v1.json`. Both are mapped in Development only.

## Calling the API

In Development every endpoint is open, so the examples below need no token.

| Method | Route | |
|---|---|---|
| POST | `/api/v1/toll-passages` | Record that one or more vehicles passed a toll station |
| POST | `/api/v1/toll-calculations` | A vehicle's toll for one day |

Both take a JSON body and **every field is `required`**, so a missing one is rejected rather than quietly
defaulting — no `0001-01-01` dates and no blank registration numbers reaching the calculation. The toll
calculation is a POST rather than a GET because it takes a body, and a body on a GET is outside what the
HTTP spec guarantees: `HttpClient.GetAsync` cannot send one and proxies may strip it.

A passage is an **observation**, not a billing decision: a camera read a plate at a gantry, and that
happened whether or not the authority knows anything about the vehicle. So any registration number is
accepted, and **one POST can carry passages for several vehicles**. Each passage reports the vehicle type
the roadside equipment saw, and that reported type is what the day is priced from.

A passage is rejected with 400 when any passage in the batch is missing its registration number, its
vehicle type or a usable timestamp (in the future, or before the year 2000), and with 409 when any passage
repeats one that is already recorded. The batch is written in one transaction, so a 409 means **nothing**
was recorded — the `(RegistrationNumber, PassedAt)` unique index makes the whole request safe to retry.

The daily toll lists only the passages that were *charged*. A passage sharing its 60-minute window with a
dearer one, and every passage after the 60 SEK cap is reached, costs nothing and is left out of the list.
A registration number with no passages that day owes nothing, so the answer is 200 with a zero fee rather
than 404.

Two endpoints, because a toll system has two consumers: the roadside equipment that reports passages, and
billing that asks what a vehicle owes. Anything else — a stateless fee calculator, a published rate list,
the holiday calendar, the vehicle-type table — had no caller in production and was removed rather than
maintained.

### Vehicle types come from the caller, and the API does not define them

`vehicleType` is **required** on every passage and must be one of `Car`, `Motorbike`, `Tractor`,
`Emergency`, `Diplomat`, `Foreign` or `Military` — matched case-insensitively against the domain's
`VehicleType`, which is also exactly what the database stores as a string. The contract assembly
deliberately declares **no enum of its own** (`WebApiModelsContractTests` enforces this), so the list of
types has one source and a client cannot drift from it. `VehicleTypeSchemaTransformer` publishes that same
list onto the OpenAPI schema at start-up, so **Scalar renders a dropdown** of the accepted values without
the contract declaring an enum of its own. An unrecognised value is a 400 naming the offending passage by
index, e.g. `passages[1].VehicleType`.

A plate that is not in `Vehicles` is still recorded and still charged: `VehicleType.Unknown` is not in
`TollFreeVehicleTypes`, so **unknown pays**. That is the safe direction — the failure mode is billing
someone who should have been exempt, which is visible and appealable, rather than silent free-riding.

**The trade-off this accepts:** the caller classifies its own vehicle, so a caller that reports
`Diplomat` rides free. That is tolerable when the only caller is trusted roadside equipment, and it is the
reason the TODO below wants the plate *and* its type resolved against Trafikverket instead.

```bash
curl -k -X POST https://localhost:7077/api/v1/toll-passages \
  -H "Content-Type: application/json" \
  -d '{"passages":[
        {"registrationNumber":"ABC123","vehicleType":"Car","passedAt":"2026-03-16T07:15:00+01:00"},
        {"registrationNumber":"MC0001","vehicleType":"Motorbike","passedAt":"2026-03-16T07:15:00+01:00"}
      ]}'

# 18 SEK
curl -k -X POST https://localhost:7077/api/v1/toll-calculations \
  -H "Content-Type: application/json" \
  -d '{"registrationNumber":"ABC123","date":"2026-03-16"}'

# 0 SEK, toll free
curl -k -X POST https://localhost:7077/api/v1/toll-calculations \
  -H "Content-Type: application/json" \
  -d '{"registrationNumber":"MC0001","date":"2026-03-16"}'
```

No SQL and no setup: clone, run, POST, POST.

### Authentication

Development allows anonymous calls, so Scalar and the examples above work without a token. The signing key
in `appsettings.Development.json` is a throwaway for exactly that reason — it is committed on purpose, is
never read outside Development, and startup fails if a real environment is left without one of its own. **In every other
environment a bearer token is mandatory** on every endpoint except `/health` — enforced by an authorization
fallback policy, so it covers any endpoint that does not opt out explicitly.

This API **validates** tokens but does not **issue** them; it is a resource server, not an identity
provider. Tokens come from whatever issuer owns that job. One is accepted when it is signed with the
configured key, names the configured issuer and audience, and has not expired — see `JwtBearerOptionsSetup`.

The signing key, issuer and audience live in the `Jwt` section of `appsettings.json`. In Scalar, paste a
token into the Authorize dialog.

## Tests

```bash
dotnet test
```

355 tests. The Domain, Application and Infrastructure projects, and most of the host test project, are
in-process and need nothing beyond the SDK — 331 of the 355.

The integration tests under `test/TollCalculator.WebApi.Host.IntegrationTests/{Infrastructure,Tolls,Auth}`
**require Docker** — Testcontainers starts `mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04`, applies
the migrations to it and runs the real API against it. They do not skip when Docker is missing; they fail.
To run only the tests that need no container:

```bash
dotnet test --filter "FullyQualifiedName!~IntegrationTests.Infrastructure&FullyQualifiedName!~IntegrationTests.Tolls&FullyQualifiedName!~IntegrationTests.Auth.AuthTests"
```

## What was wrong with the original

All six are in `C#/TollCalculator.cs`.

**1. The hourly window was measured in milliseconds-of-second (line 25).**

```csharp
long diffInMillies = date.Millisecond - intervalStart.Millisecond;
long minutes = diffInMillies/1000/60;
```

`DateTime.Millisecond` is the sub-second component, 0–999, not a timestamp. The difference is always
between -999 and 999, so `minutes` is always 0 and the `minutes <= 60` branch always wins — every passage,
however far apart, was treated as inside the same hour.
*Fixed:* `DailyTollService` compares the passages themselves against a real `ChargeWindow` of 60 minutes
opened by the first charged passage, and charges the highest fee in that window.

**2. The midday rate never applied (line 66).**

```csharp
else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
```

The `minute >= 30` was meant to exclude 08:00–08:29, already handled on the line above. Applied to the whole
08–14 range it also excludes 09:05, 10:12 and every other time in the first half of an hour, which fall
through to `return 0` — free.
*Fixed:* the schedule is data, not control flow. `TollRateInterval` rows (08:30–14:59 → 8 SEK) are seeded in
the database and matched with `Covers(TimeOnly)`.

**3. Mixed `&&` and `||` without parentheses (line 68).**

```csharp
else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
```

`&&` binds tighter than `||`, so this reads "15:00–15:59 **or** 16:00–16:59" and would return 18 for 15:00,
which should be 13. It only produced the right answer because the preceding branch caught 15:00–15:29 first.
Reordering the branches would have silently changed the fee.
*Fixed:* same as 2 — intervals cannot overlap, and `TollSchedule` validates that on construction, so no
ordering of the rules can change the answer.

**4. Holidays were hardcoded to 2013 (line 82).**

```csharp
if (year == 2013)
```

From 2014 onward no holiday was toll free. The same block also made the whole of July free via a bare
`month == 7`.
*Fixed:* `WeekendAndHolidayService` matches the date against the Swedish public holidays for that year,
resolved on demand through `IHolidayProvider` / Nager.Date. No year ever needs adding.

**5. `GetTollFee` trusted its input (line 18).**

```csharp
DateTime intervalStart = dates[0];
```

An empty array throws `IndexOutOfRangeException`, and the whole method assumes the passages arrive already
sorted — out-of-order input produced a wrong total rather than an error.
*Fixed:* `DailyTollService.Calculate` sorts defensively and returns `DailyToll.None()` for an empty day;
the API validates requests at the edge in `WebApi.Host/Validators/` and returns 400 with the offending
fields rather than throwing.

**6. Toll-free vehicles were matched by string (lines 43–53).**

```csharp
return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) || ...
```

Six `ToString()` comparisons against a private enum that nothing else could see or reuse. Any typo or
casing difference in the vehicle type silently meant "not toll free" — the vehicle got charged and nothing
reported why.
*Fixed:* a `VehicleType` enum in the domain and `TollFreeVehicleTypes` naming the free ones in one place —
one source of truth, matched by value rather than by string. The API contract declares no enum of its own;
it takes the type as a string and validates it against the domain, so an unrecognised value is a 400 that
names the offending passage and never reaches the fee calculation.

## TODO

Things deliberately left out of scope, with the reasoning, so the next person does not have to rediscover it.

**Separate the auth services depending on how the API is exposed.**
Right now there is one bearer scheme and one fallback policy for everything: anonymous in Development,
authenticated everywhere else. That is the right shape for a single internal consumer and the wrong shape
the moment the API is exposed more than one way. A machine-to-machine caller, a user-facing client and an
internal service each want different issuers, lifetimes and scopes, and `/health` already has to be carved
out by hand. Split the policies per audience — named policies with scope or role requirements per endpoint
group — rather than letting one fallback policy carry every case.

**Validate registration numbers against Trafikverket or another external source.**
`RegistrationNumberRule` only checks non-empty and at most 20 characters, so any string is accepted as a
vehicle key. No NuGet package validates Swedish plates — the Swedish ecosystem covers identity numbers
(personnummer, organisationsnummer, bankgiro), not vehicles — so the options are a format regex or a real
lookup. A format regex is the weaker of the two and actively harmful here: `VehicleType.Foreign` and
`VehicleType.Diplomat` are first-class toll-free types whose plates do not follow the Swedish format, and
the validator runs before the vehicle lookup, so it cannot know which it is dealing with. An external
lookup against Trafikverket's vehicle register would confirm the plate *and* its vehicle type from the
authoritative source, replacing the locally seeded `Vehicles` table. That brings a network call, an API
key, caching and a failure policy for when the register is unreachable — which is exactly why it is a TODO
and not a regex.

**Revisit the holiday source — the current data is only reachable over HTTP.**
Holidays come from `Nager.Date` pinned at **1.33.0** (Nov 2022), the last release that runs without a
licence key: 1.34.0 and everything after, including 2.x, throw `NoLicenseKeyException` unless you sponsor
the author on GitHub. So the in-process package is frozen at a 2022 snapshot, and the only free route to
current Nager data is the public HTTP API at `https://date.nager.at/api/v3/PublicHolidays/{year}/SE`.
Putting that on the toll-calculation path means a network dependency with its own outage, latency and
rate-limit modes, plus a cache and a stub for tests. Low risk in practice — the Swedish holiday rules have
not changed since 2005 — but it is a pinned, unmaintainable dependency. Three ways out: pay for the licence
and track the current package, call the HTTP API behind a cache, or drop the dependency and compute the
dates directly (six fixed dates, five Easter offsets via the Anonymous Gregorian algorithm, and two
"Saturday within a given week" rules). `IHolidayProvider` exists precisely so this swap touches one class.

**Deployment files — there are none yet, and their shape depends on the supplier.**
The repository has no `Dockerfile`, no `.dockerignore`, no compose file, no CI/CD pipeline and no
infrastructure definitions. Today the API runs from `dotnet run` against a SQL Server on `localhost`, and
that is the only way it runs. Three things need deciding before any of that can be written, and all three
depend on who hosts it and how:

- **Connection string and secrets.** `appsettings.Development.json` uses `Trusted_Connection=True`, which
  is Windows integrated auth and does not work from a Linux container. A deployed instance needs a
  credentialed connection string, and `Jwt:SigningKey` — intentionally empty in `appsettings.json` — has to
  come from the platform's secret store rather than a config file.
- **Migrations.** `Program.cs` only calls `MigrateDatabaseAsync` in Development, so outside Development
  nothing applies migrations at all. That is deliberate — auto-migrating on startup is a bad idea when
  several instances start at once — but it means a deployment needs its own step: an init container, a
  pipeline stage, or a one-shot job that runs before the new version takes traffic.
- **Image and probes.** Base image, registry, tagging and whether the API runs behind a reverse proxy are
  all supplier decisions. `/health` is already mapped and anonymous, so it is ready to wire to a liveness
  or readiness probe once there is something to wire it to.

Note that Docker *is* already required to run the tests — the integration suite uses Testcontainers to
start SQL Server — but that is a development dependency and says nothing about how the service itself
should be packaged.

**Choose a logging provider and ship the logs somewhere.**
The code now logs at the seams — unknown registration numbers, recorded passages, holiday cache misses,
migrations applied — through `ILogger<T>` with structured message templates, and `appsettings.json` sets
levels per namespace. But the only provider configured is the default console writer, so in a deployed
environment those logs go to stdout and nowhere else. `ILogger<T>` is the abstraction precisely so the sink
is a composition-root decision; nothing in `Application`, `Domain` or `Infrastructure` has to change.

What still needs deciding, and it travels with the hosting decision in the TODO above:

- **Sink.** Elastic (via Serilog's `Elasticsearch` sink or the OpenTelemetry exporter) and Datadog (via
  `Serilog.Sinks.Datadog.Logs`, the agent, or OTLP) are the obvious candidates; the honest default is
  OpenTelemetry's OTLP exporter, which keeps the vendor swappable. Picking a provider before knowing who
  operates the service is how you end up locked to the wrong one.
- **Structured, not formatted.** The templates already use named placeholders (`{RegistrationNumber}`,
  `{TollPassageId}`), so they arrive as queryable fields rather than interpolated prose — but only if the
  sink preserves structure. A plain-text file sink throws that away.
- **Correlation.** Unhandled errors carry a `traceId` in their ProblemDetails response and nothing else
  does. Trace context across the request, the database call and the response is what makes the logs usable
  during an incident, which argues for wiring OpenTelemetry tracing at the same time rather than later.
- **What must never be logged.** The JWT signing key, bearer tokens and connection strings. Registration
  numbers are personal data under GDPR when they identify a vehicle owner, so retention and whether they
  should be masked in logs is a question for whoever owns the deployment, not a coding decision.
