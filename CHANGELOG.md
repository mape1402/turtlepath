# Changelog

All notable changes to TurtlePath will be documented in this file.

## [v1.0.0] - Unreleased

### Added

- Created the TurtlePath library using the OctoMap repository structure.
- Extracted the reusable Pelican handler foundation from the Elysium template.
- Added base tests, CI, release workflow, README, changelog, samples, and benchmark project placeholders.
- Documented compatibility surface before package splitting.
- Split the extracted surface into focused packages for identifier, domain, application, persistence, EF Core, mapping, validation, and filtering concerns.
- Moved mapping and validation contracts into dedicated abstraction packages, removed generic bucket folders from source projects, and separated application errors from ASP.NET Core HTTP exceptions.
- Consolidated mapping, validation, and persistence contracts into `TurtlePath.Abstractions`.
- Removed the root composition package, standalone identifier package, CId EF value generator package, serialization package, and Swagger package; identifiers now live under `TurtlePath.Domain.Identifier`.
- Renamed the main handler package from `TurtlePath.Application` to `TurtlePath`.
- Added the EF Core base DbContext with configuration discovery and `CId` value conversion.

