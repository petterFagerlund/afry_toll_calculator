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

A vehicle is charged at most once per 60-minute window, and the highest fee in that window applies.
Each day is capped at 60 SEK. Weekends and holidays are free, as are Motorbike, Tractor, Emergency,
Diplomat, Foreign and Military vehicles. Every timestamp is converted to Europe/Stockholm before the
schedule and the holiday calendar are consulted, so the same instant costs the same whether it arrives
as `07:15+01:00` or as `06:15Z`.

None of this is configuration. The rates, the cap and the toll-free vehicle types are rows in the
database, seeded by the initial migration. Holidays are not stored at all — they are resolved for any
year through `IHolidayProvider`, backed by the Nager.Date library.

## Architecture

```
src/
  TollCalculator.Domain/           Models, Enums, Constants, Services, repository interfaces — references nothing
  TollCalculator.Infrastructure/   DbContext, configurations, seed, repository implementations
  TollCalculator.Application/      Services over the repository interfaces, ApplicationResult
  TollCalculator.WebApi.Models/    V1 request/response contracts        — references nothing
  TollCalculator.WebApi.Host/      Controllers, validators, mappers, auth, OpenAPI
test/
  TollCalculator.Domain.UnitTests/
  TollCalculator.Application.UnitTests/
  TollCalculator.Infrastructure.UnitTests/
  TollCalculator.WebApi.Host.IntegrationTests/
```

Dependencies point inward: Infrastructure → Domain, Application → Domain. Neither Application nor
Domain knows that EF Core exists — the repository *interfaces* live in `Domain/Repositories/` and only
`Infrastructure` implements them. `WebApi.Host` is the composition root and references all of them.

**Why DDD here.** The interesting part of this problem is a handful of rules that interact — the hourly
window, the daily cap, toll-free dates, toll-free vehicle types. In the original they were tangled into
one method together with the data they operate on, which is why four of the six bugs are arithmetic or
operator-precedence mistakes hiding inside an if-chain. Putting the rules in `Domain` with no dependencies
means they can be tested directly, exhaustively and without a database: `DailyTollService` takes a vehicle
type, the instants it passed and a tariff (the schedule plus the daily cap), and returns the total plus
which passages were charged — it asks for nothing it does not read, so no entity and no registration
number. It holds no state, so it is a singleton. Everything else — SQL, HTTP, JSON, auth — is arranged
around that and can be changed without touching it.

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
and vehicle types — there is no separate migration step to run.

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
| POST | `/api/v1/toll/calculate` | Fee for a set of timestamps; stateless, stores nothing |
| POST | `/api/v1/toll-passages` | Record that a vehicle passed a toll station |
| GET | `/api/v1/vehicles/{registrationNumber}/toll?date=yyyy-MM-dd` | A vehicle's toll for one day |
| GET | `/api/v1/toll-rates` | The seeded rate intervals |
| GET | `/api/v1/holidays?year=` | Public holidays for a year, defaulting to the current one |
| GET | `/api/v1/vehicle-types` | Every vehicle type and whether it is toll free |

Start with `calculate`, which needs nothing in the database:

```bash
curl -k -X POST https://localhost:7077/api/v1/toll/calculate \
  -H "Content-Type: application/json" \
  -d '{"vehicleType":"Car","timestamps":["2026-03-16T07:15:00+01:00"]}'
```

```json
{"vehicleType":"Car","totalFee":18,"chargedPassages":[{"passedAt":"2026-03-16T07:15:00+01:00","amount":18}]}
```

A busy day shows the cap trimming the last charge so the total lands exactly on 60:

```bash
curl -k -X POST https://localhost:7077/api/v1/toll/calculate \
  -H "Content-Type: application/json" \
  -d '{"vehicleType":"Car","timestamps":[
        "2026-03-16T06:45:00+01:00","2026-03-16T07:50:00+01:00","2026-03-16T09:00:00+01:00",
        "2026-03-16T15:40:00+01:00","2026-03-16T17:10:00+01:00"]}'
```

returns `totalFee: 60` with the breakdown `13, 18, 8, 18, 3`.

### Recording passages needs a vehicle first

`POST /api/v1/toll-passages` only accepts a passage for a vehicle that already exists, and **there is no
endpoint that creates vehicles** — registering a vehicle is outside the scope of the assignment. Without a
row you get a 404. Insert one directly:

```sql
INSERT INTO [Vehicles] ([RegistrationNumber], [VehicleType]) VALUES ('ABC123', 'Car');
```

`RegistrationNumber` is the primary key, `nvarchar(20)`, stored trimmed and uppercased. `VehicleType` is
stored as the enum *name*, so `'Car'`, `'Motorbike'`, `'Tractor'`, `'Emergency'`, `'Diplomat'`, `'Foreign'`
or `'Military'`. Then:

```bash
curl -k -X POST https://localhost:7077/api/v1/toll-passages \
  -H "Content-Type: application/json" \
  -d '{"registrationNumber":"ABC123","passedAt":"2026-03-16T07:15:00+01:00"}'

curl -k "https://localhost:7077/api/v1/vehicles/ABC123/toll?date=2026-03-16"
```

### Authentication

Development allows anonymous calls, so Scalar and the examples above work without a token. **In every other
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
*Fixed:* a `VehicleType` enum in the domain, `TollFreeVehicleTypes` naming the free ones in one place, and
`VehicleTypeRules` rows carrying the flag. The API contract has its own `VehicleType` enum, so an
unrecognised value is a 400 from the deserializer and never reaches the domain.

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
