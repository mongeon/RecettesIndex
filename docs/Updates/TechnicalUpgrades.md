# Technical Recommendations — Mes Recettes (Blazor WASM + Supabase)

> These recommendations cover code quality, performance, security, and maintainability.
> Before implementing anything, read existing files to validate what is already in place.

---

## 1. 📦 Performance — Lazy Loading Assemblies (low priority)

**Problem:** Blazor WASM loads all .NET assemblies on first load, which can make startup slow (several seconds on mobile).

**Note:** Since all pages live in the same `RecettesIndex` assembly, assembly-level lazy loading requires splitting pages into separate class library projects. This is significant refactoring — only worth pursuing if startup performance is measured and confirmed as a real issue.

**If pursued — `RecettesIndex.csproj`:**
```xml
<ItemGroup>
  <!-- Lazy load less-visited pages -->
  <BlazorWebAssemblyLazyLoad Include="RecettesIndex.Pages.Stores.dll" />
  <BlazorWebAssemblyLazyLoad Include="RecettesIndex.Pages.Authors.dll" />
</ItemGroup>
```

**`App.razor`:**
```razor
@using Microsoft.AspNetCore.Components.WebAssembly.Services
@inject LazyAssemblyLoader AssemblyLoader

<Router AppAssembly="typeof(App).Assembly"
        AdditionalAssemblies="lazyLoadedAssemblies"
        OnNavigateAsync="OnNavigateAsync">
```

---

## 2. 🔄 State Management — Consider Fluxor (low priority)

**Problem:** With Scoped/Singleton services, shared state between components can become difficult to trace as the app grows (e.g., updating a recipe should refresh the dashboard, favourites, and the list simultaneously).

**Current state:** The existing service + `Action`/`EventCallback` pattern handles state well for the app's current scale. This is not urgent.

**Consider — Fluxor (Redux for Blazor):**
```xml
<PackageReference Include="Fluxor.Blazor.Web" Version="*" />
```

Relevant only if more than 3–4 components need to react to the same data changes and the current event-based approach becomes difficult to maintain.

---

## Implementation Priority

| Priority | Item                        |
|----------|-----------------------------|
| 🟢 Low   | #1 Lazy loading, #2 Fluxor |
