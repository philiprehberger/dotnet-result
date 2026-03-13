# Changelog

## [0.2.0] - 2026-03-12

### Added
- `Combine` method for merging multiple results (fail-fast)
- `CombineAll` method for collecting all errors from multiple results
- `Tap` and `TapErr` methods for side-effects without breaking chains
- LINQ query syntax support via `Select` and `SelectMany` extensions

## 0.1.1 (2026-03-10)

- Add README to NuGet package so it displays on nuget.org

## 0.1.0 (2026-03-09)

- Initial release
- `Result<T, E>` struct with Ok/Err factory methods
- Unwrap, UnwrapOr, UnwrapOrElse, UnwrapErr
- Map, MapErr, FlatMap, Match
- Static helpers: Result.Ok, Result.Err, Result.Try, Result.TryAsync, Result.All
