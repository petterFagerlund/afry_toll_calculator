# Toll Calculator

The toll fee assignment rewritten as a .NET 10 REST API: SQL Server with EF Core, DDD-structured projects,
documented with Scalar, protected by a bearer token.

The original Java and C# are left untouched in `Java/` and `C#/` as the reference this was rewritten from.
`C#/TollCalculator.cs` is where the six bugs listed at the end live.

## Toll rules

| Time | Fee | | Time | Fee |
|---|---|---|---|---|
| 06:00–06:29 | 8 SEK | | 15:00–15:29 | 13 SEK |
| 06:30–06:59 | 13 SEK | | 15:30–16:59 | 18 SEK |
| 07:00–07:59 | 18 SEK | | 17:00–17:59 | 13 SEK |
| 08:00–08:29 | 13 SEK | | 18:00–18:29 | 8 SEK |
| 08:30–14:59 | 8 SEK | | 18:30–05:59 | 0 SEK |

Each band runs from its start up to, but **not including**, the next one, so a passage at 06:29:30 costs
8 SEK rather than nothing. A vehicle is charged at most once per rolling 60-minute window and the highest
fee in that window applies; each day is capped at 60 SEK. Weekends and Swedish public holidays are free, as
are Motorbike, Tractor, Emergency, Diplomat, Foreign and Military.

Every timestamp is converted to Europe/Stockholm before the schedule and the holiday calendar are consulted,
so the same instant costs the same whether it arrives as `07:15+01:00` or as `06:15Z`.

## Running it

Needs the .NET 10 SDK and SQL Server at `localhost` with Windows authentication — otherwise change
`ConnectionStrings:TollDatabase` in `src/TollCalculator.WebApi.Host/appsettings.Development.json`.

```bash
dotnet dev-certs https --trust
dotnet run --project src/TollCalculator.WebApi.Host
```

In Development the database is migrated and seeded on startup, and every endpoint is open. Scalar is at
**https://localhost:7077/scalar**.

## Calling it

| | |
|---|---|
| `POST /api/v1/toll-passages` | Record that one or more vehicles passed a toll station |
| `POST /api/v1/toll-calculations` | A vehicle's toll for one day |

```bash
curl -k -X POST https://localhost:7077/api/v1/toll-passages \
  -H "Content-Type: application/json" \
  -d '{"passages":[
        {"registrationNumber":"ABC123","vehicleType":"Car","passedAt":"2026-03-16T07:15:00+01:00"},
        {"registrationNumber":"MC0001","vehicleType":"Motorbike","passedAt":"2026-03-16T07:15:00+01:00"}
      ]}'

curl -k -X POST https://localhost:7077/api/v1/toll-calculations \
  -H "Content-Type: application/json" \
  -d '{"registrationNumber":"ABC123","date":"2026-03-16"}'   # 18 SEK
```

Swap `ABC123` for `MC0001` to see 0 SEK. No SQL and no setup — clone, run, POST, POST.

**A passage is an observation**, not a billing decision: a camera read a plate, and that happened whether
or not the authority knows the vehicle. So any registration number is accepted, the batch may span several
vehicles, and each passage carries the vehicle type the roadside equipment reported — which is what the day
is priced from. A plate reported with no usable type is still charged, because `VehicleType.Unknown` is
not toll-free:
the failure mode is billing someone who should have been exempt, which is visible and appealable, rather
than silent free-riding. The trade-off is that a caller reporting `Diplomat` rides free, which is why the
TODO below wants plates resolved against Trafikverket.

Every body field is `required`, so a missing one is rejected rather than defaulting to `0001-01-01` or an
empty plate. The batch is written in one transaction: a repeated `(RegistrationNumber, PassedAt)` answers
409 and records **nothing**, making the whole request safe to retry. A batch holds at most 1000 passages,
so one request cannot load an unbounded number of entities into a single transaction. A day with no passages is 200 with a
zero fee, not 404. Only *charged* passages are listed — one sharing its window with a dearer one, or
arriving after the cap, costs nothing and is left out.

`WebApi.Models` declares **no enums** (enforced by a test); `vehicleType` is a string validated against the
domain, and `VehicleTypeSchemaTransformer` publishes the accepted values onto the OpenAPI schema so Scalar
still renders a dropdown. One source of truth, no contract drift.

Outside Development a bearer token is mandatory on everything except `/health`, enforced by an authorization
fallback policy. The API validates tokens but does not issue them. The Development signing key is committed
on purpose and never read elsewhere; startup fails if a real environment has none.

## Architecture

```
src/
  TollCalculator.Domain/          Models, enums, constants — references nothing, holds no behaviour
  TollCalculator.Application/     Services, repository and provider interfaces, ApplicationResult
  TollCalculator.Infrastructure/  DbContext, configurations, seed, repository and provider implementations
  TollCalculator.WebApi.Models/   V1 request/response contracts — references nothing
  TollCalculator.WebApi.Host/     Controllers, validators, mappers, auth, OpenAPI
```

Dependencies point inward, and neither Application nor Domain knows EF Core or Nager.Date exists — those
interfaces live in `Application/` and only `Infrastructure` implements them.

The interesting part of this problem is a handful of rules that interact: the hourly window, the daily cap,
toll-free dates, toll-free vehicle types. In the original they were tangled into one method together with
the data they operate on, which is why four of the six bugs are arithmetic or precedence mistakes hiding
inside an if-chain. They now live in `Application/Services/`, separated from that data and testable without
a database. Every service returns `ApplicationResult<T>`, so a failure travels back to the controller as
data and `ApiControllerBase.MapErrorResponse` turns it into a status code.

Rates are rows in the database because a rate table is reference data. The daily cap is a single number, so
it lives in configuration (`Toll:DailyCap`, validated on startup — the minimum is 1, because a missing
section would otherwise bind to 0 and silently make every passage free). Holidays are not stored at all —
they are
resolved per year through `IHolidayProvider`, backed by Nager.Date.

## Tests

```bash
dotnet test
```

290 tests. The container-backed ones in `TollCalculator.WebApi.Host.IntegrationTests` **require Docker** —
Testcontainers starts SQL Server, migrates it and runs the real API against it. They fail rather than skip
when Docker is missing. The Domain, Application and Infrastructure projects need nothing but the SDK.

## What was wrong with the original

All six are in `C#/TollCalculator.cs`.

**1. The hourly window was measured in milliseconds-of-second (line 25).**

```csharp
long diffInMillies = date.Millisecond - intervalStart.Millisecond;
long minutes = diffInMillies/1000/60;
```

`DateTime.Millisecond` is the sub-second component, 0–999, not a timestamp, so `minutes` is always 0 and
every passage counted as inside the same hour however far apart.
*Fixed:* `DailyTollService` compares the passages against a real 60-minute window opened by the first
charged passage.

**2. The midday rate never applied (line 66).**

```csharp
else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
```

`minute >= 30` was meant to exclude 08:00–08:29. Applied to the whole 08–14 range it also excludes 09:05,
10:12 and every other first-half-hour, which fall through to free.
*Fixed:* the schedule is data, not control flow — `TollRateInterval` rows matched with `Covers(TimeOnly)`.

**3. Mixed `&&` and `||` without parentheses (line 68).**

```csharp
else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
```

`&&` binds tighter, so this reads "15:00–15:59 **or** 16:00–16:59" and returns 18 for 15:00, which should be
13. It only worked because an earlier branch caught 15:00–15:29 first; reordering would have changed the fee.
*Fixed:* intervals cannot overlap and `TollSchedule` validates that on construction, so no ordering of the
rules can change the answer.

**4. Holidays were hardcoded to 2013 (line 82).**

```csharp
if (year == 2013)
```

From 2014 onward no holiday was toll free. The same block made all of July free via a bare `month == 7`.
*Fixed:* `WeekendAndHolidayService` resolves the Swedish public holidays for whatever year the date falls in.

**5. `GetTollFee` trusted its input (line 18).**

```csharp
DateTime intervalStart = dates[0];
```

An empty array throws `IndexOutOfRangeException`, and out-of-order passages produced a wrong total rather
than an error.
*Fixed:* `Calculate` sorts defensively and returns `DailyToll.None()` for an empty day; the API validates at
the edge and answers 400 with the offending field.

**6. Toll-free vehicles were matched by string (lines 43–53).**

```csharp
return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) || ...
```

Six `ToString()` comparisons against a private enum nothing else could reuse. Any typo or casing difference
silently meant "not toll free" — the vehicle got charged and nothing reported why.
*Fixed:* a `VehicleType` enum in the domain and `TollFreeVehicleTypes` naming the free ones in one place,
matched by value.

## Deliberately out of scope

**Resolve plates against Trafikverket.** `RegistrationNumberRule` only checks non-empty and ≤ 20 characters.
A format regex would be actively wrong here — `Foreign` and `Diplomat` are first-class toll-free types whose
plates do not follow the Swedish format. A real lookup would confirm the plate *and* its type from the
authoritative source rather than trusting the caller-reported vehicle type. That brings
a network call, an API key, caching and a failure policy, which is why it is a TODO and not a regex.

**Split the auth policies per audience.** One bearer scheme and one fallback policy is right for a single
internal consumer and wrong the moment the API is exposed more than one way.

**Nager.Date is pinned to 1.33.0** (Nov 2022), the last release that runs without a licence key, so the
holiday data is a 2022 snapshot. The free alternative is its public HTTP API, which trades a network
dependency for freshness.

**The daily cap in `appsettings.json`** should move to a secret store once it varies per environment.

**July and the day before a holiday** are free under the real Swedish congestion tax. Neither is in the
assignment's rules, so neither is implemented.
