# Copilot Instructions — RecettesIndex

## Project Snapshot
- Blazor WebAssembly app on .NET 9 (`src/RecettesIndex.csproj`) with MudBlazor + Supabase.
- Main architecture: page/components -> service abstractions -> Supabase query/data layer.
- Entry point and DI wiring are in `src/Program.cs`.

## Architecture You Should Follow
- Prefer service interfaces from `src/Services/Abstractions/*` in pages (`IRecipeService`, `IBookService`, `IAuthorService`, `IStoreService`).
- Avoid direct `Supabase.Client` usage in pages; keep Supabase access inside services/query classes (`RecipeService`, `BookService`, `SupabaseRecipesQuery`).
- Services should return `Result<T>` for failure handling (see `src/Services/Result.cs`, `src/Services/RecipeService.cs`).
- Reuse `CrudServiceBase<TModel,TService>` for standard CRUD behaviors (logging, network error messages, cache hooks).
- Cache read-heavy lists through `ICacheService` (books/authors/stores patterns in `BookService`, `AuthorService`, `StoreService`).

## UI and Component Patterns
- Use MudBlazor components first; follow existing page composition patterns.
- `Recipes.razor` is the reference for modular page composition:
  - `RecipeQuickFilters`, `RecipeAdvancedFilters`, `RecipeActiveFilters`
  - `RecipeLoadingState`, `RecipeGridView`, `RecipeTableView`, `EmptyState`
- State UX convention on pages: `loading -> error alert -> empty state -> content`.
- Keep user-facing text in French unless surrounding file clearly uses English.

## Data and Validation Conventions
- Models are in `src/Models/Recette.cs` (Supabase attributes + DataAnnotations).
- Preserve model relationships (`Book.Authors`, `Recipe.Book`, `Recipe.Store`) and avoid duplicating relationship fetch logic in pages.
- Prefer constants classes (`CacheConstants`, `PaginationConstants`, `RecipeSortConstants`, `UIConstants`) over magic numbers.

## Async, Cancellation, and Errors
- Use async all the way; pass `CancellationToken` through service calls when available.
- In pages with route changes or reloads, use cancellation-safe loading patterns (see `RecipeDetails.razor`).
- Log with structured templates (`Logger.LogError(ex, "... {Id}", id)`) and return friendly messages through `Result<T>`.

## Testing in This Repo
- Test stack: xUnit + NSubstitute + bUnit (`tests/RecettesIndex.Tests.csproj`).
- Use `BunitContext` (new tests) and `JSInterop.Mode = JSRuntimeMode.Loose` for MudBlazor-heavy component tests.
- Prefer behavior/state tests for pages (loading/error/empty/success) over duplicating implementation logic.

## Dev Workflow and Commands
- Build/test locally before finishing:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet test`
- Run app locally from repo root: `dotnet run --project src/RecettesIndex.csproj`.
- CI (`.github/workflows/azure-static-web-apps-green-pond-067ed2010.yml`) runs restore/build/test on PRs to `main` before deploy.

## Integration Points
- Supabase config is read from configuration keys in `Program.cs` (`supabase:Url`, `supabase:Key`).
- Browser persistence is via `LocalStorageService` + extensions (`FavoritesExtensions`, `RecentRecipesExtensions`).
- Auth state is broadcast via `AuthService.AuthStateChanged`; pages subscribe/unsubscribe on init/dispose.
