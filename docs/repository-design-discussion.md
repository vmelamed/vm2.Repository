# Repository Pattern, Aggregate Boundaries, and the Commit Seam — Design Discussion

> **Document type:** design discussion / decision record (free-form). This is not a normative specification, so it
> deliberately avoids RFC 2119 keywords. It captures the reasoning, trade-offs, and leanings behind `vm2.Repository`
> so they survive between working sessions.
>
> **Date:** 2026-05-23 · **Participants:** Val Melamed, Claude (Opus 4.7)

## TL;DR

- **Position (settled):** one repository per *bounded context*, not one `IRepository<TRoot>` per aggregate.
- **Aggregate-as-consistency-boundary:** kept. **One-repository-class-per-aggregate:** rejected. The latter conflates an
  *invariant* (one aggregate per transaction, with justified exceptions) with a *code structure* (N repository classes).
- **Enforcement:** a commit-time policy pipeline (already prototyped) defaults to a single aggregate root per unit of
  work and permits a *declared* multi-root set as an explicit, auditable exception.
- **Keystone decision (leaning):** own the commit seam by overriding the virtual `SaveChangesAsync()` rather than
  registering an EF `SaveChangesInterceptor`. Whoever owns the commit seam owns the rule contract — and that is what
  lets the policy rules become provider-neutral and leave the EF folder.
- **Persistence ignorance:** a primary goal, with real-world proof (a past NHibernate → EF migration behind this
  abstraction). `IQueryable` is treated as a standardized seam, not a leak.

## 1. The crux: where the generic type parameter lives

Two interfaces look almost identical and differ only in the *position* of one type parameter:

```csharp
// Repo per bounded context — type parameter on the METHOD
interface IRepository : IDisposable
{
    TEntity Find<TEntity>(params object[] keys) where TEntity : class;
    IQueryable<TEntity> Set<TEntity>() where TEntity : class;
    void Add<TEntity>(TEntity entity) where TEntity : class;
    TEntity Remove<TEntity>(TEntity entity) where TEntity : class;
    Task CommitAsync(CancellationToken ct = default);
}

// Repo per aggregate — type parameter on the INTERFACE
interface IRepository<TEntity> where TEntity : class { /* same members, one type */ }
```

The interface-level `<T>` limits a repository to a single aggregate root, which forces a service that spans roots to
inject several repositories. The method-level `<T>` gives one repository per context that speaks the whole ubiquitous
language. That single positional difference is the entire argument.

`Set<TEntity>()` deliberately exposes `IQueryable` so that LINQ is the domain query language — the service layer
composes declarative queries that read like the business requirement. The accepted cost: a service can write a query
the provider cannot translate, which surfaces at runtime rather than compile time.

## 2. The seven corners (both sides)

The position was stress-tested by arguing the strongest case *for* per-aggregate repositories on each corner, then
answering it. A steelman you can knock over is not a steelman.

| Corner                     | Strongest case for `IRepository<TRoot>`                          | Verdict                          |
|----------------------------|-----------------------------------------------------------------|----------------------------------|
| 1. Consistency boundary    | The aggregate *is* the transaction boundary; per-aggregate      | Repo-per-context (uses Vernon)   |
|                            | repos make that the structural default.                         |                                  |
| 2. Read / query side       | Aggregate roots are the only legitimate entry points.           | Repo-per-context                 |
| 3. Testability             | A small, focused interface is trivial to mock.                  | Narrow win for per-aggregate     |
| 4. Cognitive load          | Juniors get rails; the shape is hard to misuse.                 | Per-aggregate *if* tooling weak  |
| 5. DDD orthodoxy           | "Evans says a repository per aggregate root."                   | Draw on letter, win on spirit    |
| 6. EF mechanics            | Per-aggregate maps onto command handlers.                       | Repo-per-context (decisive)      |
| 7. Persistence ignorance   | (Not a per-aggregate argument — a separate axis.)               | Repo-per-context                 |

### Corner 1 — Consistency boundary

Vernon's first rule of thumb is "modify one aggregate per transaction," but he states it as a *guideline with named
exceptions*, not a law. A structural cage cannot express the exception: crossing the boundary means injecting a second
repository, which then silently permits *any* multi-aggregate write with no record of intent. The policy pipeline
instead defaults to one root and makes the exception explicit and auditable (see §4). That is more faithful to Vernon
than the cage, and it wins the corner *using Vernon as the authority*.

### Corner 2 — Read / query side

Aggregates are a command-side concept. Queries legitimately span aggregates, and CQRS read models routinely do.
Per-aggregate repositories push reads into over-fetching whole roots or breeding `FindByX` methods. Boundaries are
enforced at *commit*, not at *query* — a one-line thesis worth emphasizing: the write side is governed, the read side
is free.

### Corner 3 — Testability

Mostly illusory: mocking a hand-written `FindActiveByUserAsync` tests the mock, not the query, and the query is the
thing most likely to be wrong. The honest concession: for a pure command-handler unit test, the small mock is genuinely
lighter. A real but narrow win.

### Corner 4 — Cognitive load

The strongest honest argument *against* the position. The answer is a bet, stated as one: the guardrail tooling gives
juniors the same safety net without amputating the expressive repository — "sharp tools with guardrails over blunt
tools with training wheels." The bet pays only if the guardrails are genuinely good.

### Corner 5 — DDD orthodoxy

Evans constrains *access semantics* (a repository gives the illusion of an in-memory collection of a root's objects),
not *class count*, and never forbids a context-scoped facade. Fowler's Repository is collection-like over the mapping
layer, not per-type. So "the book says per aggregate" reduces to "aggregates are consistency boundaries" — which the
position enforces — not "one repository class per root is the only faithful encoding."

### Corner 6 — EF mechanics

`DbContext` is the unit of work; `DbSet<T>` is the per-type repository. A per-aggregate repository over a shared
`DbContext` is redundant *and* cannot obtain a per-root transaction without a per-root `DbContext` (a connection per
root — absurd). The only place to enforce the boundary without that absurdity is in software at commit time. EF's own
reality forces the design.

## 3. When `IRepository<TRoot>` is the right call

The position is *not* anti-per-aggregate. `IRepository<TRoot>` is the better choice for a trivial domain model, a team
without a strong architect, inexperienced developers without mentorship, or a very large team. In those contexts the
simpler, harder-to-misuse shape earns its keep, and the per-aggregate repository often *equals* the bounded-context
repository because the context holds a single aggregate.

## 4. Enforcement: the commit-time policy pipeline

A prototype already exists (roughly one year old; treated as a thinking artifact to rebuild from, not a baseline to
preserve). Its shape:

- An EF `SaveChangesInterceptor` runs `DetectChanges()` and applies an ordered list of `IPolicyRule` to every
  Added/Modified/Deleted entry.
- `IPolicyRule` is a per-entity action; `ICommitPolicy` is an ordered bundle; a DI-aware fluent builder composes them.
- Aggregate membership is declared by a **marker interface** `IAggregate<TRoot>` (interfaces chosen over attributes).
  An entity that has no `IAggregate<TRoot>` maps to `NoRoot`. An entity may implement only one `IAggregate<TRoot>`.
- `EnforceAggregateBoundary` adopts the first seen root and requires all entries in the unit of work to share it —
  unless the `DbContext` implements `IHasAllowedAggregateRoots`, which declares a permitted *set* of roots.
- Companion rules: `EnforceTenantBoundary`, `AuditEntities` (audit fields, soft delete), `CompleteEntities`,
  `EnforceInvariants`. `FullDddTenantedAuditedPolicy` bundles all five in order.

`IHasAllowedAggregateRoots` is the important piece: it is the concrete encoding of Vernon's "the boundary is not
absolute." The single-root rule is the default; a multi-root transaction is a declared, visible, auditable opt-in
rather than something forbidden or something that happens by accident.

### Open issue 4a — set-level invariants through a per-entity API

The boundary and tenant rules are *set*-level invariants ("how many distinct roots are in this unit of work?") but the
current `IPolicyRule` API is *per-entity*, so those rules smuggle cross-entity state through `AsyncLocal` and a
"first one wins" adoption. A change-set-level rule shape — `ApplyAsync(IReadOnlyCollection<change entries>, ...)` over
a *provider-neutral* change-set abstraction (not EF's `EntityEntry`) — would remove the `AsyncLocal` usage and the
order-dependence, make "count distinct roots" trivial and deterministic, and make the pipeline ORM-portable. This is
the single most load-bearing refactor on the table: it serves three goals at once (set semantics, determinism,
portability). Per-entity rules (audit, complete, invariants) loop internally with no real loss.

### Open issue 4b — `NoRoot` in auto-adopt mode

A trace suggests that, with no `IHasAllowedAggregateRoots` context, any `NoRoot` entity in the change set throws,
because the adoption branch is skipped for `NoRoot` and the subsequent containment check runs against a null allowed
set. Outbox rows, audit records, and join entities are exactly the cross-cutting `NoRoot` rows one expects in a
transaction. Resolution candidate: treat `NoRoot` as implicitly always allowed, and pin the intended behavior with a
test. (Trace to be confirmed against the rebuilt code.)

### Open issue 4c — reflection and reset

Cache the `GetInterfaces()` lookup per CLR type (it runs on every entity on every commit; performance is a stated NFR).
The `AsyncLocal` reset across sequential commits currently relies on `ExecutionContext` restoration, which is subtle
and fragile to refactors; the change-set API in 4a dissolves the problem entirely.

## 5. The commit seam: interceptor vs. overridden `SaveChangesAsync()`

This is the keystone, because the seam choice determines the *input type* of the policy rules.

- **`SaveChangesInterceptor` (prototype).** Works on any `DbContext`, composes via DI, and catches a direct
  `SaveChanges()` that bypasses the repository. But EF hands the interceptor `EntityEntry`, so the interceptor is
  precisely what forces `EntityEntry` into the rule signatures and traps the rules in the EF folder.
- **Override the virtual `SaveChangesAsync()` (leaning toward this).** The method is owned, so the EF adapter maps
  `ChangeTracker.Entries()` onto a provider-neutral change-set inside the adapter and passes *that* to the rules. EF is
  confined to the adapter; the rules live in the core package. The override *is* the method, so it cannot be bypassed
  on the `SaveChanges` path.

The two doubts — "is the interceptor the right hook?" and "how do I get `IPolicyRule` out of the EF folder?" — are one
decision. Owning the commit seam is what makes the rule contract provider-neutral.

### Async-only repository and the sync guard

`IRepository` exposes only async methods. The sync `SaveChanges()` is overridden to throw, which both enforces the
async-only contract and closes the synchronous bypass. For the public surface this is a clear, helpful exception
(for example, `RepositoryUsageException("This repository is async-only. Call CommitAsync().")`) rather than a bare
framework exception — a sharp edge with a sign on it.

### Honest residual hole — bulk operations

EF Core's `ExecuteUpdate` / `ExecuteDelete` and raw SQL go straight to the database, skipping the change tracker, so
they skip the policy pipeline entirely; neither the interceptor nor the override can see them. The lean is *practical,
not dogmatic*: do not ban them (they are real set-based performance wins, and banning would be the blunt tool). The key
fact for any future handling — they materialize no entity instances, so audit, tenant, invariant, and soft-delete logic
*cannot* run; an `ExecuteDelete` on a soft-deletable type silently hard-deletes. The danger is concentrated on governed
aggregates and harmless on `NoRoot` plumbing. The repository does not expose these operations, so reaching for them is
the deliberate "full-on EF, on your own responsibility" boundary. Likely future resolution: allow, but make it a
conscious, visible choice — the same pattern as the aggregate boundary, transposed.

## 6. Persistence ignorance / ORM portability (Corner 7)

Hiding ORM mechanics so the persistence technology is swappable is a primary goal, backed by real evidence: roughly
fifteen years ago, an NHibernate → EF migration behind this abstraction paid off with one DI change once EF matured —
a rare, concrete proof, since most teams never swap.

- **The query seam.** `IQueryable` is a *standardized* BCL interface, not a proprietary leak: a provider either
  implements it (and becomes a valid swap target) or declines and fades (NHibernate shipped a LINQ provider only late
  and incomplete relative to EF). The practical portability target is therefore "other competent `IQueryable`
  providers," which bounds residual differences to translation quirks one regression-tests anyway — not a
  "queries don't port" problem. The real, lived caveat: the same expression can translate, throw, or silently
  client-evaluate per provider (a Postgres → Cosmos DB migration is the sharpest example — Cosmos' LINQ has no real
  joins and a crippled `GroupBy`). "Barring provider query *languages* like HQL," `IQueryable`-based LINQ largely ports.
- **The implementation seam.** The commit-time policy pipeline is the package's actual differentiator, and today its
  rules depend on EF's `EntityEntry`. The fix is the change-set abstraction from §4a/§5: confine EF to the adapter so
  the rules — the investment worth keeping — are portable.

## 7. Static analysis (Roslyn): why the runtime is the guarantee

A compile-time analyzer cannot reliably decide "does this transaction touch more than one aggregate?" — not because of
a Roslyn limitation but because of undecidability (Rice's theorem). The analyzer sees the branches but cannot decide
which execute or what types and values flow through them at runtime (interface dispatch, configuration, reflection,
dynamic query predicates). A guardrail analyzer is therefore tuned for *low false positives* (flag only when certain),
accepts false negatives, and lets the runtime be authoritative.

Division of labor: the **runtime check is the guarantee** (the suspenders, already built); the **analyzer is early,
advisory feedback** (the belt, incomplete, lowest priority). Build the runtime first — which is, in fact, how it
happened.

## 8. Testing note — `FakeDbSet` retires

`FakeDbSet` served its purpose and is being retired. Test fakes move to **SQLite in-memory**, or, less preferred, EF
Core's `InMemory` provider — Microsoft itself steers toward SQLite because `InMemory` does not enforce relational
semantics (no constraints, no real SQL translation) and so green-lights queries that fail against a real database.

## 9. Honest costs of the position

- The compiler enforces an `IRepository<TRoot>` boundary for free and bug-free; this position moves that enforcement
  into tooling that must be built, maintained, and trusted. The failure mode shifts from "a developer injects two
  repositories" to "a bug in the policy pipeline."
- Lighter command-handler test mocks and the junior-safety argument are genuine ground the per-aggregate side holds;
  the guardrail tooling is the answer to both, and only as good as that tooling.

## 10. Decision log

| Topic                          | State            | Note                                                             |
|--------------------------------|------------------|-----------------------------------------------------------------|
| Repo-per-bounded-context       | Settled          | The core position.                                              |
| Aggregate marker mechanism     | Settled in code  | `IAggregate<TRoot>` marker interface over attributes.          |
| Multi-root exception mechanism | Prototype        | `IHasAllowedAggregateRoots`; Vernon's "boundary not absolute."  |
| Commit seam                    | Leaning          | Override `SaveChangesAsync()`, retire the interceptor hook.     |
| Async-only + sync guard        | Leaning          | Sync `SaveChanges()` throws a clear usage exception.            |
| Rule API shape                 | Open (issue 4a)  | Per-entity vs. provider-neutral change-set; the load-bearing one.|
| `NoRoot` in auto-adopt mode    | Open (issue 4b)  | Likely "always allowed"; confirm trace, add test.              |
| Bulk operations (`ExecuteX`)   | Parked           | Practical, not dogmatic; make the bypass a conscious choice.    |
| `FakeDbSet`                    | Settled          | Retire; use SQLite in-memory.                                  |
| Roslyn analyzer                | Deferred         | Advisory belt; runtime is the guarantee.                       |
| Blog post                      | On hold          | Finish after the package is built, tested, and tooled.         |

## References

- [Repository (Patterns of Enterprise Application Architecture)](https://martinfowler.com/eaaCatalog/repository.html) — Martin Fowler
- [Patterns of Enterprise Application Architecture](https://martinfowler.com/books/eaa.html) — Martin Fowler
- [Domain-Driven Design: Tackling Complexity in the Heart of Software](https://www.domainlanguage.com/ddd/) — Eric Evans
- [Domain-Driven Design Distilled](https://www.informit.com/store/domain-driven-design-distilled-9780134434421) — Vaughn Vernon
- [Implementing Domain-Driven Design](https://www.informit.com/store/implementing-domain-driven-design-9780321834577) — Vaughn Vernon
- [Microservices](https://martinfowler.com/articles/microservices.html) — Martin Fowler & James Lewis
- [The Law of Leaky Abstractions](https://www.joelonsoftware.com/2002/11/11/the-law-of-leaky-abstractions/) — Joel Spolsky
- [EF Core: Testing with SQLite vs. the In-Memory provider](https://learn.microsoft.com/ef-core/testing/) — Microsoft
- [EF Core: ExecuteUpdate and ExecuteDelete](https://learn.microsoft.com/ef-core/saving/execute-insert-update-delete) — Microsoft
