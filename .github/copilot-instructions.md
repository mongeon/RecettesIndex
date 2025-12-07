# AI Agent Instructions for Mes Recettes

> **Version**: 5.1 | **Last Updated**: December 7, 2025 | **Status**: Active | **AI-Optimized**: ✅

## ⚡ Critical Directives

1.  **UI Framework**: **ALWAYS** use **MudBlazor** components. **NEVER** use raw HTML elements (div, span, input) unless absolutely necessary.
2.  **Testing**: **MANDATORY** unit tests for all business logic. No code changes without tests. Use **xUnit** + **NSubstitute**.
3.  **Async/Await**: All I/O operations must be **asynchronous** and accept a `CancellationToken`.
4.  **GitHub Operations**: Use **GitHub MCP Server** tools for all PRs, issues, and repo management.
5.  **Workflow**: **NEVER** commit directly to `main`. Always use feature branches.
6.  **Constructors**: Use **C# 12 Primary Constructors** with null validation.
7.  **Error Handling**: All service methods must return `Result<T>`.
8.  **Namespaces**: Use **file-scoped** namespace declarations.
9.  **Caching**: Use `ICacheService` for frequently accessed data (books, authors).
10. **Validation**: **ALWAYS** validate changes in local development before creating pull requests.

## 🏗️ System Context

-   **Project Type**: Blazor WebAssembly (.NET 9.0)
-   **Backend**: Supabase (PostgreSQL + Auth)
-   **Language**: C# 12
-   **Architecture**: Client-side SPA, Query/Service pattern, In-memory caching.

## 📂 Project Structure

```
/src
  /Models       # Data models (Supabase attributes)
  /Pages        # Blazor pages & dialogs (MudBlazor)
  /Services     # Business logic (Result<T> pattern)
    /Abstractions # Interfaces
  /Components   # Reusable UI components
/tests          # xUnit tests
```

## 🧩 Code Patterns

### 1. Data Models (Supabase)

```csharp
[Table("recettes")]
public class Recipe : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Column("rating")]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Reference(typeof(Book), joinType: ReferenceAttribute.JoinType.Left, true)]
    public Book? Book { get; set; }
}
```

### 2. Services (Primary Constructors + Result<T>)

```csharp
public class RecipeService(
    IRecipesQuery query,
    ICacheService cache,
    ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _query = query ?? throw new ArgumentNullException(nameof(query));

    public async Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var item = await _query.GetByIdAsync(id, ct);
            return item != null ? Result<Recipe>.Success(item) : Result<Recipe>.Failure("Not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipe {Id}", id);
            return Result<Recipe>.Failure("Error loading recipe");
        }
    }
}
```

### 3. UI Components (MudBlazor)

```razor
@page "/recipes"
@inject IRecipeService Service

<MudContainer>
    <MudText Typo="Typo.h3">Recipes</MudText>
    @if (loading) { <MudSkeleton /> }
    else if (error != null) { <MudAlert Severity="Severity.Error">@error</MudAlert> }
    else {
        <MudDataGrid Items="@items">
            <PropertyColumn Property="x => x.Name" />
            <TemplateColumn>
                <CellTemplate>
                    <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="@(() => Edit(context.Item))" />
                </CellTemplate>
            </TemplateColumn>
        </MudDataGrid>
    }
</MudContainer>

@code {
    private bool loading = true;
    private string? error;
    // ... implementation
}
```

### 4. Unit Tests (xUnit + NSubstitute)

```csharp
public class RecipeServiceTests
{
    private readonly IRecipesQuery _query = Substitute.For<IRecipesQuery>();
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _service = new RecipeService(_query, Substitute.For<ICacheService>(), Substitute.For<ILogger<RecipeService>>());
    }

    [Fact]
    public async Task GetById_ReturnsSuccess_WhenFound()
    {
        _query.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Recipe { Id = 1 });
        var result = await _service.GetByIdAsync(1);
        Assert.True(result.IsSuccess);
    }
}
```

## 🔄 Development Workflow

1.  **Plan**: Analyze requirements.
2.  **Branch**: Create a feature branch (e.g., `feature/add-search`).
3.  **Test**: Write unit tests for new logic.
4.  **Implement**: Write code using patterns above.
5.  **Verify**: Run tests (`dotnet test`).
6.  **PR**: Use GitHub MCP to create a Pull Request.

## 🧠 AI Decision Matrix

| Scenario | Action |
| :--- | :--- |
| **UI Element** | Use `Mud*` components (e.g., `MudButton`, `MudTextField`). |
| **Data Access** | Use `Supabase.Client` wrapped in Query services. |
| **State** | Use `ICacheService` for read-heavy data. |
| **Validation** | Use DataAnnotations on Models. |
| **Logging** | Log exceptions with structured logging before returning `Result.Failure`. |
