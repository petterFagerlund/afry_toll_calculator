# Toll Calculator

The toll fee assignment rewritten as a .NET 10 REST API: SQL Server with EF Core, DDD-structured projects,
documented with Scalar, protected by a bearer token.

The original Java is left untouched in `Java/` as the reference this was rewritten from.

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
**https://localhost:7077/scalar**. Migration on startup is deliberately Development-only — elsewhere
schema changes belong to the deploy, not to whichever instance happens to boot first.

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
the data they operate on, which is where most of its bugs hid — arithmetic and operator-precedence mistakes
buried in an if-chain. They now live in `Application/Services/`, separated from that data and testable
without a database. Every service returns `ApplicationResult<T>`, so a failure travels back to the controller as
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
