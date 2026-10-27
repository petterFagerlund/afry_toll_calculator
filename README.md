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
The day is capped at 60 SEK. Weekends and holidays are free, as are Motorbike, Tractor, Emergency,
Diplomat, Foreign and Military vehicles. The day boundary is Europe/Stockholm, not UTC.

None of this is configuration. The rates, the cap, the toll-free vehicle types and the holidays are rows
in the database, seeded by the initial migration.

## Architecture

```
src/
  TollCalculator.Domain/           Models, Enums, Constants, Services   — references nothing
  TollCalculator.Infrastructure/   DbContext, configurations, seed, repositories
  TollCalculator.Application/      Services over the repositories, ApplicationResult
  TollCalculator.WebApi.Models/    V1 request/response contracts        — references nothing
  TollCalculator.WebApi.Host/      Controllers, validators, mappers, auth, OpenAPI
test/
  TollCalculator.Domain.UnitTests/
  TollCalculator.Application.UnitTests/
  TollCalculator.Infrastructure.UnitTests/
  TollCalculator.WebApi.Host.IntegrationTests/
```

Dependencies point inward: Infrastructure → Domain, Application → Infrastructure + Domain,
WebApi.Host → WebApi.Models + Application + Domain.

**Why DDD here.** The interesting part of this problem is a handful of rules that interact — the hourly
window, the daily cap, toll-free dates, toll-free vehicle types. In the original they were tangled into
one method together with the data they operate on, which is why four of the six bugs are arithmetic or
operator-precedence mistakes hiding inside an if-chain. Putting the rules in `Domain` with no dependencies
means they can be tested directly, exhaustively and without a database: `DailyTollService` takes a vehicle,
its passages, a schedule and a cap, and returns the day's total plus which passages were charged. Everything
else — SQL, HTTP, JSON, auth — is arranged around that and can be changed without touching it.

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
In Development the database is created and migrated automatically on startup, including the seeded rates,
holidays and vehicle types — there is no separate migration step to run.

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
| GET | `/api/v1/holidays?year=` | The seeded holidays, optionally for one year |
| GET | `/api/v1/vehicle-types` | Every vehicle type and whether it is toll free |
| POST | `/api/v1/auth/token` | Exchange client credentials for a bearer token |

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

### Getting a token

Development allows anonymous calls so Scalar works without one. Outside Development every endpoint except
`/health` and the token endpoint itself requires `Authorization: Bearer <token>`.

```bash
curl -k -X POST https://localhost:7077/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId":"toll-client","clientSecret":"development-only-client-secret"}'
```

```json
{"accessToken":"eyJhbGciOiJIUzI1NiIs...","tokenType":"Bearer","expiresAt":"2026-10-27T15:24:42+00:00"}
```

The credentials and signing key live in the `Jwt` section of `appsettings.Development.json`. The endpoint
is rate limited to 10 requests per minute. In Scalar, paste the `accessToken` into the Authorize dialog.

## Tests

```bash
dotnet test
```

360 tests. The Domain, Application and Infrastructure projects, and most of the host test project, are
in-process and need nothing beyond the SDK — 336 of the 360.

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
*Fixed:* a `Holidays` table seeded with Swedish public holidays for 2026 and 2027, matched by
`WeekendAndHolidayService` on the date itself. Adding a year is a migration, not a code change.

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
`VehicleTypeRules` rows carrying the flag. `VehicleTypes.TryParse` converts the API's string once, and an
unrecognised value is a 400 with a message, not a silent miss.
