# Heroes Showcase Validation Issues

## Context

Validation is being done against `C:\elysium\template-demos\HeroesShowcase` to make sure the generated template/demo works end to end and to separate demo mistakes from root TurtlePath/template/library bugs.

## Confirmed Root Issues

### TP-EF-001 - DataScorpio can break EF async materialization

- **Area:** `TurtlePath.EntityFrameworkCore` + `TurtlePath.DataScorpio`
- **Symptom:** `GET /heroes` failed with `The source 'IQueryable' doesn't implement 'IAsyncEnumerable<T>'`.
- **Root cause:** `DataScorpioStorageCriteriaApplier` materializes results and returns `Items.AsQueryable()`. `StorageReaderAdapter` then calls EF `ToListAsync` on a non-EF queryable.
- **Root fix applied locally:** `StorageReaderAdapter` now falls back to synchronous `ToList()` when the final query no longer implements `IAsyncEnumerable<TEntity>`.
- **Follow-up:** Publish a TurtlePath patch version so template projects do not need local project references.

### TP-DESIGN-001 - Base query handlers cannot request navigation includes

- **Area:** TurtlePath query handler/storage abstractions
- **Symptom:** `GET /heroes` and `GET /villains` fail when mapping `Team.Name` because `Team` is not loaded.
- **Root cause:** `GetPagedInfoQueryHandler`, `GetByIdQueryHandler`, and `IStorageReaderAdapter` support filters/sorts/paging, but not include expressions or projection hooks before mapping.
- **Why workaround is not enough:** Null-checking `TeamName` hides a broken relationship example. The demo should show a clean way to read related data.
- **Root fix applied locally:** Added include support to storage criteria/fluent read set and virtual include methods on GetOne/GetMany/GetPaged handlers.
- **Follow-up:** Publish TurtlePath patch version and update the template to the published package.

### TP-BUILD-001 - Some repo test/benchmark fixtures were not aligned with public API changes

- **Area:** TurtlePath Studio tests and benchmark fake storage read set.
- **Symptom:** Full solution build failed because `DotNetTemplatePackageManagerTests` used the old constructor and the benchmark fake did not implement the new `Include` method.
- **Fix applied locally:** Updated tests to pass `HttpClient`; updated benchmark fake to implement `Include`.

### TP-CID-001 - Create happy path did not assign configured client-generated CId values

- **Area:** `TurtlePath` create command steps + `TurtlePath.Domain.Identifier`
- **Symptom:** `POST /teams` originally failed with `invalid base32 length, length:36` when the API used `Ulid` CIds stored as strings.
- **Root cause:** The default create step mapped the request to an entity but never assigned an `Id` through the configured `ICIdFactory`. EF then tried to generate a store value that did not match the configured `Ulid` parser.
- **Root fix applied locally:** `DefaultEntityCreationStep` now assigns an entity-specific or default `CId` factory value when the entity implements `IEntity<CId>`, the current Id is empty, and the CId definition is `ClientGenerated`.
- **Follow-up:** Publish a TurtlePath patch and remove temporary local project references from the showcase.

### TP-UPD-001 - Update happy path allowed mappers to touch entity primary keys

- **Area:** `TurtlePath` update command handlers
- **Symptom:** `PUT /teams/{id}` failed because EF detected `Team.Id` as modified.
- **Root cause:** The default update handler mapped the full request onto the tracked entity. Requests inherit `BaseRequest.Id`, so mapper adapters can assign the key property while the entity is tracked.
- **Root fix applied locally:** `GenericUpdateCommandHandler` now preserves the entity Id around request-to-entity mapping for both response and no-response variants.
- **Follow-up:** Add/keep regression coverage for update handlers with tracked EF entities.

## Confirmed Library Issues

### OCTOMAP-001 - OctoMap cannot emit enum constants in MapFrom

- **Area:** OctoMap adapter/library behavior
- **Symptom:** `POST /skills/hero` failed with `The constant expression type 'Heroes.Service.Domain.Enums.Alignment' is not supported by the current OctoMap value emitter`.
- **Root cause:** The skill mapping attempted to map a constant enum value with `MapFrom(_ => Alignment.Hero)`.
- **Demo adjustment applied locally:** Removed the enum constant mapping and moved that business default to a TurtlePath `IAfterMapHook`.
- **Follow-up:** Decide whether OctoMap should support enum constants in emitted mappings.

## Confirmed Demo/Template Issues

### DEMO-001 - Automation registration was missing

- **Area:** Heroes showcase DI
- **Symptom:** Generated handlers for automation queries/commands were not resolved reliably.
- **Fix applied locally:** Added `.UseAutomations(typeof(Constants).Assembly)` to TurtlePath registration.
- **Template check:** Base template already needs this validated in generated projects.

### TEMPLATE-001 - Spider to Pelican bridge lost concrete request type for response requests

- **Area:** Template `SpiderPelicanExtensions.DefaultSend`.
- **Symptom:** HTTP endpoints using response requests failed through Spider while the same handlers passed when invoked directly through Pelican.
- **Root cause:** The bridge accepted `IRequest<TResponse>` and attached `IRequest<TResponse>` to Spider, so the pipeline did not preserve the concrete request type for generated/manual handlers and boundaries.
- **Fix applied locally:** The bridge now dispatches through a generic private method using the runtime concrete request type and calls `Attach<TRequest, TResponse>`.

### DEMO-002 - DataScorpio default sorts used entity property names instead of aliases

- **Area:** Heroes showcase automation profiles
- **Symptom:** `GET /villains` failed with `The field 'PowerLevel' is not queryable`.
- **Root cause:** Default sort was `-PowerLevel`, but the DataScorpio profile exposes `power`.
- **Fix applied locally:** Use `-power` and `alias` aliases.

### DEMO-003 - SQLite does not support `DateTimeOffset` ordering

- **Area:** Heroes showcase incident DataScorpio profile
- **Symptom:** `GET /incidents` failed when sorting by `ReportedAt`.
- **Root cause:** SQLite provider cannot translate `DateTimeOffset` in `ORDER BY`.
- **Fix applied locally:** Incident default sort now uses `ThreatLevel` via the `threat` alias.
- **Follow-up:** If the demo must sort by reported time, store a SQLite-friendly sortable value such as `ReportedAtUtcTicks`.

### DEMO-004 - Paged query constructors should not accept null settings

- **Area:** Heroes showcase paged query messages
- **Symptom:** Paged endpoints are fragile if model binding or manual callers pass null `PagedSettings`.
- **Fix applied locally:** Constructors now use `pagedSettings ?? new PagedSettings()`.

### DEMO-005 - Local SQLite startup needs schema creation and seed flow

- **Area:** Heroes showcase API startup
- **Symptom:** Endpoints failed when SQLite tables did not exist.
- **Fix applied locally:** `UseDatabaseDefaultsAsync` runs `EnsureCreatedAsync()` and optional seed job in Development.

### DEMO-006 - Automation responses that expose navigation-derived fields need null-safe mappings or projection

- **Area:** Heroes showcase OctoMap profiles
- **Symptom:** `POST /heroes` failed when mapping `Hero.Team.Name` after creating a hero with only `TeamId`.
- **Root cause:** The automation create path maps the just-saved entity to the response; the `Team` navigation is not loaded in that object.
- **Fix applied locally:** Hero and villain response mappings are null-safe for `TeamName`. Read query handlers still include `Team` and validate relationship loading.
- **Follow-up:** Consider richer automation configuration for response projection/includes when mutation responses intentionally expose navigation-derived fields.

### DEMO-007 - Some automation requests were missing validators

- **Area:** Heroes showcase validators
- **Symptom:** `PUT /teams/{id}` first failed because `UpdateTeamRequest` had no registered validator.
- **Root cause:** The automation profile declared update/patch handlers, but validators were only created for some requests.
- **Fix applied locally:** Added validators for `UpdateTeamRequest`, `UpdateVillainRequest`, `DeactivateHeroRequest`, and `ResolveIncidentRequest`.

## Current Validation State

- `GET /teams`: passing.
- `GET /heroes`: passing.
- `GET /villains`: passing.
- `GET /incidents`: passing.
- DataScorpio filtered/sorted reads: passing for heroes, villains, and incidents.
- `GET by id`: passing for teams, heroes, villains, and incidents.
- Mutations/actions: passing for team create/update, hero create/update/deactivate, villain create/update/capture, hero/villain skills, incident report/assign/resolve.
- Showcase build: passing.
- Showcase tests: passing, 10/10.
- TurtlePath full solution build: passing with 0 warnings and 0 errors.
- TurtlePath targeted tests: passing for `TurtlePath.Tests` net10, 65/65.
