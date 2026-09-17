# C# Service Standards

A working .NET 10 service that demonstrates how I expect a C# backend to be
built. Every standard below is enforced by the build, a test, or an analyzer.
Nothing here is advice; a violation fails.

The sibling repository `go-standards` implements the same reference API in Go.
The two agree on behavior deliberately, so the standards can be compared without
the differences between the languages getting in the way. Where this repository
diverges, it says so and gives the reason.

## Running it

Windows, PowerShell:

```powershell
.\scripts\verify.ps1              # everything CI runs
.\scripts\verify.ps1 -SkipMutation  # faster: no mutation run
```

The whole stack, including PostgreSQL and the migrator:

```powershell
docker compose up --build
```

The API then answers on `http://localhost:8080`. Health is open; every `/api`
route needs a bearer token.

Tests run through `dotnet run`, not `dotnet test`:

```powershell
dotnet run --project tests\Domain.Tests\Domain.Tests.csproj
```

xUnit v3 brings Microsoft.Testing.Platform with it, and the .NET 10 SDK refuses
to drive a platform project through the VSTest target that `dotnet test` uses.
The command fails loudly rather than reporting a false pass, but it cannot run
these suites. Launching each one directly is xUnit v3's own documented path, and
it is what `verify.ps1` and CI do.

The VSTest adapter stays referenced even so, because Stryker drives tests
through VSTest and cannot see a platform application without it. Mutation
testing depends on that reference.

## The standards

**1. Dependencies point inward.** Domain knows nothing of Application,
Infrastructure, EF Core, ASP.NET, or Npgsql. Application knows only Domain.
*Enforced by* `tests/Architecture.Tests`, which asserts every arrow with
NetArchTest rather than describing it in a document nobody rereads.

**2. Identifiers carry their type.** A `TaskId` is not a `long`, so an inventory
identifier cannot be passed where a task identifier belongs. The wrapper is a
readonly record struct, so it costs no allocation, and converters keep it a
plain number in JSON and a bigint in the database.

A value converter must be *total* over the values its column can hold. EF runs
the provider's default through a key's converter while it works out whether a
key has been set, so a validating factory in the read direction rejects zero and
throws before an insert ever reaches the database. `From` guards values arriving
from outside; an internal `FromStored` maps values the database already
constrains. A converter is a mapping, not a guard.

**3. Values validate at construction.** A `TaskTitle` cannot exist untrimmed,
empty, or over the limit. Types that cannot hold an invalid value remove a whole
class of defensive checks from everything downstream.

**4. No literal escapes a constant.** Every string and number carrying meaning
lives in a named constant. *Enforced by* a Roslyn analyzer in `analyzers/`, so a
stray literal fails the build rather than a review. It caught two bare numbers in
this repository's own tests while they were being written.

**5. Warnings are errors.** `TreatWarningsAsErrors`, `AnalysisMode=All`, and
`EnforceCodeStyleInBuild` mean formatting, unused code, and analyzer findings all
stop the build. Every rule this repository turns off is listed in `.editorconfig`
with the reason it was turned off.

**6. One error shape.** Failures answer RFC 9457 problem details on
`application/problem+json`, carrying `code` and `fields` as extension members.
*Divergence:* the Go sibling answers with a bare `{code, message, fields}`. The
machine-readable parts survive so a client matches on the same stable codes.

**7. One place maps failures to responses.** No endpoint has a try/catch.
Middleware turns a domain failure into its status: not-found to 404, a validation
failure carrying field problems to 422, one carrying none to 400, anything
unrecognized to a generic 500 that logs the cause and reveals nothing.

**8. Validation reports every problem at once.** A form marks all its bad fields
in one round trip rather than one per attempt. The test that guards this asserts
over a wholly empty request, because a validator that returns on its first
problem passes every single-field test.

**9. Ports belong to the caller.** `ITaskStore` lives in Application beside the
service that calls it, not beside the EF implementation. A store that grows a
method no feature calls has grown it for the wrong reason.

**10. Migrations are an admin process.** A separate executable applies them and
exits with a status. The API never migrates at startup, so replicas never race
for the same lock and a failed migration is distinguishable from a failed
service. Compose runs it to completion before the API starts.

**11. The database enforces its own invariants.** Check constraints on title
length, plan membership, seat counts, quantities, and statuses mean an
application defect cannot write a row the domain would reject.
*Enforced by* `tests/Integration.Tests`, which asserts the constraints reject
what they should against a real PostgreSQL.

**12. Tests run against the real engine.** Testcontainers starts and removes its
own PostgreSQL, so the EF mapping, the migrations, and the constraints are
exercised as the database enforces them. An in-memory provider accepts writes a
real one rejects.

**13. Every request carries an identifier.** `X-Request-ID` is accepted when it
fits, generated when it does not, echoed on the response, and attached to every
log line the request produces. Logs are JSON on stdout.

**14. Probes are open; everything else is not.** Liveness and readiness answer
without a credential, because a platform probes before it holds one and must
tell a stopped instance from an unauthorized one. Readiness runs a statement
under a deadline rather than merely opening a connection. Every `/api` route
sits behind a bearer token compared in constant time.

**15. Coverage is not the bar; mutation is.** Stryker mutates Domain and
Application and the build fails below 70%. Coverage says a line ran. Mutation
says a test would have noticed if that line were wrong.

**16. The image is small and unprivileged.** Chiseled runtime bases carry no
shell and no package manager, and the app runs as a non-root user. Tool versions
are pinned in `.config/dotnet-tools.json` and package versions centrally in
`Directory.Packages.props`, so CI and a developer's machine build the same thing.

**17. State and the event announcing it commit together.** Recording a signup
writes the signup row and an outbox row in one transaction. A service that
writes to its database and to a broker separately performs a dual write, and any
failure between the two leaves one of them wrong: a consumer told about a signup
that rolled back, or a stored signup nobody hears about. A relay publishes from
the outbox afterward and records that it did.
*Enforced by* `tests/Integration.Tests/OutboxTests.cs`, against a real database.

**18. Consumers absorb repeats.** Publishing after the commit makes delivery
at-least-once: a relay can deliver and fail before marking the row, and the next
pass delivers again. The alternative, marking first, loses messages instead, and
a lost message is worse than a repeated one for any handler written to expect
repeats. `IEventConsumer` says so in its contract, and the outbox keys on the
event identifier so a producer cannot write the same message twice.

Dispatch itself needs no mediator library. `EventDispatcher` takes the
`IEnumerable<IEventConsumer>` the container already resolves and calls the ones
that accept the event. Swapping the in-process handlers for a broker means adding one
adapter behind the publisher; the domain never learns which broker you picked.

## Layout

```
analyzers/LiteralAnalyzer   the literal rule, applied to every project
src/Domain                  rules, typed values, no dependencies
src/Application             services and the ports they call
src/Infrastructure          EF Core, stores, the schema
src/Web                     endpoints, auth, problem details, logging
src/Migrator                the admin process that migrates and seeds
tests/                      domain, application, web, integration, architecture, analyzer
```

## Divergences from the Go sibling

Two, both deliberate:

- **Problem details.** RFC 9457 rather than a bare error object, as standard 6
  explains.
- **Query parsing.** The Go version parses the stock query from `url.Values` in
  its domain package. Here the domain takes three plain values and the query
  string keys stay in the web layer, so the domain does not know that HTTP
  exists.

Configuration also follows .NET convention: `ConnectionStrings__Default` rather
than `DATABASE_URL`.
