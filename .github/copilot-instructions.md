# AI Agent Instructions for Mes Recettes

> **Version**: 5.0 | **Last Updated**: November 26, 2025 | **Status**: Active | **AI-Optimized**: ✅

## 🤖 AI Agent Quick Reference

### Immediate Context
- **Project Type**: Blazor WebAssembly (.NET 9.0) with Supabase backend
- **Primary Language**: C# 12 with Razor components
- **UI Framework**: MudBlazor (Material Design)
- **Database**: PostgreSQL via Supabase
- **Test Framework**: xUnit with NSubstitute
- **Architecture**: Client-side SPA with Query/Service pattern + in-memory caching
- **Key Patterns**: Primary constructors, file-scoped namespaces, Result<T> pattern, CancellationToken support

### Key AI Decision Points
1. **Always use MudBlazor components** - Never suggest HTML elements
2. **Mandatory unit tests** - No code changes without comprehensive tests
3. **Async/await patterns** - All I/O operations must be asynchronous with CancellationToken support
4. **GitHub MCP server** - Use for all GitHub operations (PRs, issues, etc.)
5. **Feature branch workflow** - Never work on main branch directly
6. **Primary constructors** - Use C# 12 primary constructors with null validation
7. **Result<T> pattern** - All service methods return Result<T> for error handling
8. **File-scoped namespaces** - Always use file-scoped namespace declarations
9. **Constants organization** - Use dedicated constant classes (CacheConstants, PaginationConstants, etc.)
10. **In-memory caching** - Use ICacheService for frequently accessed data (books, authors)

## Table of Contents
- [🤖 AI Agent Quick Reference](#-ai-agent-quick-reference)
- [Project Overview](#project-overview)
- [AI-Specific Guidelines](#ai-specific-guidelines)
- [Code Patterns & Examples](#code-patterns--examples)
- [Business Domain](#business-domain)
- [Development Environment](#development-environment)
- [Architecture & Design](#architecture--design)
- [Coding Standards](#coding-standards)
- [Testing Strategy](#testing-strategy)
- [Development Workflow](#development-workflow)
- [Security Guidelines](#security-guidelines)
- [Performance Standards](#performance-standards)
- [Common Scenarios](#common-scenarios)
- [Troubleshooting Guide](#troubleshooting-guide)

## Project Overview

### Mission Statement
Mes Recettes is a personal recipe management application built with Blazor WebAssembly that empowers home cooks to organize, discover, and enjoy their culinary journey through digital recipe management.

### Core Application Features
The application provides users with:
- **Recipe Management**: Store, organize, and categorize personal recipes
- **Cookbook Integration**: Associate recipes with physical and digital cookbooks
- **Author Tracking**: Manage cookbook authors and their publications  
- **Rating System**: 1-5 star rating system for tried recipes
- **Reference Tracking**: Page numbers for physical cookbook references
- **Recipe Sources**: Support for books, stores/restaurants, and websites
- **Print Optimization**: Clean, printer-friendly recipe formatting
- **Search & Discovery**: Advanced filtering and search capabilities

- **Print Optimization**: Clean, printer-friendly recipe formatting
- **Search & Discovery**: Advanced filtering and search capabilities

### Target Audience
- **Primary**: Home cooks managing personal recipe collections
- **Secondary**: Cooking enthusiasts organizing multiple cookbooks
- **Tertiary**: Food bloggers and culinary students

## AI-Specific Guidelines

### 🎯 Primary AI Objectives
1. **Code Quality**: Generate maintainable, testable C# code following established patterns
2. **UI Consistency**: Use MudBlazor components exclusively with Material Design principles
3. **Test Coverage**: Create comprehensive unit tests for all business logic
4. **Performance**: Implement async patterns and efficient data operations
5. **User Experience**: Prioritize simplicity and intuitive workflows

### 🧠 AI Decision Matrix

| Scenario | Always Do | Never Do | Consider |
|----------|-----------|----------|-----------|
| UI Components | Use MudBlazor | Raw HTML | Material Design principles |
| Data Operations | Async/await | Blocking calls | Error handling |
| Testing | xUnit + NSubstitute | Manual testing only | Edge cases |
| Database | Supabase patterns | Direct SQL | Optimized queries |
| GitHub | MCP server tools | Git CLI for GitHub ops | PR templates |

### 🔧 Code Generation Patterns

#### Component Template
```csharp
@page "/example"
@inject ServiceType Service

<MudContainer MaxWidth="MaxWidth.Large">
    <MudText Typo="Typo.h4" Class="mb-4">Page Title</MudText>
    
    @if (loading)
    {
        <MudSkeleton />
    }
    else if (error != null)
    {
        <MudAlert Severity="Severity.Error">@error</MudAlert>
    }
    else
    {
        <!-- Content -->
    }
</MudContainer>

@code {
    private bool loading = true;
    private string? error;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Load data
        }
        catch (Exception ex)
        {
            error = "Failed to load data";
        }
        finally
        {
            loading = false;
        }
    }
}
```

#### Service Template (C# 12 Primary Constructors)
```csharp
using Microsoft.Extensions.Logging;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing example operations.
/// </summary>
public class ExampleService(
    IExampleQuery query, 
    ICacheService cache, 
    Supabase.Client supabaseClient, 
    ILogger<ExampleService> logger) : IExampleService
{
    private readonly IExampleQuery _query = query ?? throw new ArgumentNullException(nameof(query));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<ExampleService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    public async Task<Result<T>> GetAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var item = await _query.GetByIdAsync(id, ct);
            return item != null 
                ? Result<T>.Success(item) 
                : Result<T>.Failure("Item not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get {Type} with id {Id}", typeof(T).Name, id);
            return Result<T>.Failure($"An error occurred while loading {typeof(T).Name}");
        }
    }
}
```

#### Test Template (with Query Pattern)
```csharp
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecettesIndex.Services;
using RecettesIndex.Services.Abstractions;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class ExampleServiceTests
{
    private readonly IExampleQuery _query;
    private readonly ICacheService _cache;
    private readonly Supabase.Client _client;
    private readonly ILogger<ExampleService> _logger;
    private readonly ExampleService _service;
    
    public ExampleServiceTests()
    {
        _query = Substitute.For<IExampleQuery>();
        _cache = new CacheService();
        _client = new Supabase.Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<ExampleService>>();
        _service = new ExampleService(_query, _cache, _client, _logger);
    }
    
    [Fact]
    public async Task GetAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var expected = new Example { Id = 1, Name = "Test" };
        _query.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(expected);
        
        // Act
        var result = await _service.GetAsync(1);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expected.Id, result.Value.Id);
    }
    
    [Fact]
    public async Task GetAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        _query.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Example?)null);
        
        // Act
        var result = await _service.GetAsync(999);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
```

### 🚀 AI Workflow Optimization

#### When Generating Code:
1. **Start with interfaces** - Define contracts before implementation
2. **Include validation** - Add data annotations and business rule validation
3. **Handle errors gracefully** - Wrap operations in try-catch with user-friendly messages
4. **Add logging** - Include structured logging for debugging
5. **Write tests first** - Generate test cases alongside implementation

#### When Modifying Existing Code:
1. **Read existing patterns** - Match the established code style
2. **Preserve business logic** - Don't break existing functionality
3. **Update related tests** - Modify tests to match code changes
4. **Check dependencies** - Verify impact on other components
5. **Validate changes** - Run tests before suggesting commits

## Code Patterns & Examples

### 📋 File Structure Patterns

#### Models (Data Layer)
```csharp
// Location: /src/Models/Recette.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;

namespace RecettesIndex.Models
{
    /// <summary>
    /// Represents a recipe in the application.
    /// </summary>
    [Table("recettes")]
    public class Recipe : BaseModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the recipe.
        /// </summary>
        [PrimaryKey("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipe.
        /// </summary>
        [Column("name")]
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rating of the recipe (1-5 stars).
        /// </summary>
        [Column("rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the recipe.
        /// </summary>
        [Column("created_at")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets optional notes or modifications for the recipe.
        /// </summary>
        [Column("notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the ID of the book this recipe is from, if applicable.
        /// </summary>
        [Column("book_id")]
        public int? BookId { get; set; }

        /// <summary>
        /// Gets or sets the page number where the recipe can be found in the book.
        /// </summary>
        [Column("page")]
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be positive")]
        public int? BookPage { get; set; }

        /// <summary>
        /// Gets or sets the URL of the recipe source, if applicable.
        /// </summary>
        [Column("url")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Url { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date of the recipe.
        /// </summary>
        [Column("created_at")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets optional notes or modifications for the recipe.
        /// </summary>
        [Column("notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the ID of the book this recipe is from, if applicable.
        /// </summary>
        [Column("book_id")]
        public int? BookId { get; set; }

        /// <summary>
        /// Gets or sets the page number where the recipe can be found in the book.
        /// </summary>
        [Column("page")]
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be positive")]
        public int? BookPage { get; set; }

        /// <summary>
        /// Gets or sets the URL of the recipe source, if applicable.
        /// </summary>
        [Column("url")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? Url { get; set; }
    
        /// <summary>
        /// Gets or sets the book this recipe is associated with.
        /// </summary>
        [Reference(typeof(Book), joinType: ReferenceAttribute.JoinType.Left, true)]
        public Book? Book { get; set; }
    }
}
```

#### Services (Business Logic Layer with Query Pattern)
```csharp
// Location: /src/Services/Abstractions/IRecipeService.cs
using RecettesIndex.Models;

namespace RecettesIndex.Services.Abstractions;

public interface IRecipeService
{
    Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(
        string? term, 
        int? rating, 
        int? bookId, 
        int? authorId, 
        int page, 
        int pageSize, 
        string? sortLabel = null, 
        bool sortDescending = false, 
        CancellationToken ct = default);
    
    Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    
    // Helper data for filters
    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
}

// Location: /src/Services/RecipeService.cs
using Microsoft.Extensions.Logging;
using RecettesIndex.Models;
using RecettesIndex.Services.Abstractions;

namespace RecettesIndex.Services;

/// <summary>
/// Service for managing recipe operations including search, CRUD operations, and related data retrieval.
/// </summary>
public class RecipeService(
    IRecipesQuery q, 
    ICacheService cache, 
    Supabase.Client supabaseClient, 
    ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _q = q ?? throw new ArgumentNullException(nameof(q));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<RecipeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(
        string? term, 
        int? rating, 
        int? bookId, 
        int? authorId, 
        int page, 
        int pageSize, 
        string? sortLabel = null, 
        bool sortDescending = false, 
        CancellationToken ct = default)
    {
        try
        {
            // Clamp pagination parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            var ids = new HashSet<int>();

            // Build search based on term
            if (!string.IsNullOrWhiteSpace(term))
            {
                foreach (var id in await _q.GetRecipeIdsByNameAsync(term.Trim(), rating, ct)) 
                    ids.Add(id);
                
                var bookIdsByTitle = await _q.GetBookIdsByTitleAsync(term.Trim(), ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByTitle, rating, ct)) 
                    ids.Add(id);
                
                var authorIds = await _q.GetAuthorIdsByNameAsync(term.Trim(), ct);
                var bookIdsByAuthors = await _q.GetBookIdsByAuthorIdsAsync(authorIds, ct);
                foreach (var id in await _q.GetRecipeIdsByBookIdsAsync(bookIdsByAuthors, rating, ct)) 
                    ids.Add(id);
            }
            else
            {
                foreach (var id in await _q.GetAllRecipeIdsAsync(rating, ct)) 
                    ids.Add(id);
            }

            // Apply filters
            if (bookId.HasValue)
            {
                var idsByBook = await _q.GetRecipeIdsByBookIdsAsync([bookId.Value], rating, ct);
                ids.IntersectWith(idsByBook);
            }

            if (authorId.HasValue)
            {
                var bookIds = await _q.GetBookIdsByAuthorAsync(authorId.Value, ct);
                var idsByAuthor = await _q.GetRecipeIdsByBookIdsAsync(bookIds, rating, ct);
                ids.IntersectWith(idsByAuthor);
            }

            var total = ids.Count;
            var allModels = await _q.GetRecipesByIdsAsync(ids.ToList(), ct);

            // Apply sorting
            IEnumerable<Recipe> sortedModels = allModels;
            if (!string.IsNullOrWhiteSpace(sortLabel))
            {
                sortedModels = sortLabel.ToLower() switch
                {
                    RecipeSortConstants.Name => sortDescending 
                        ? allModels.OrderByDescending(r => r.Name) 
                        : allModels.OrderBy(r => r.Name),
                    RecipeSortConstants.Rating => sortDescending 
                        ? allModels.OrderByDescending(r => r.Rating) 
                        : allModels.OrderBy(r => r.Rating),
                    RecipeSortConstants.CreatedAt => sortDescending 
                        ? allModels.OrderByDescending(r => r.CreationDate) 
                        : allModels.OrderBy(r => r.CreationDate),
                    _ => allModels
                };
            }

            // Apply pagination
            var items = sortedModels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Result<(IReadOnlyList<Recipe>, int)>.Success((items, total));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching recipes");
            return Result<(IReadOnlyList<Recipe>, int)>.Failure("An error occurred while searching recipes");
        }
    }

    public async Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheConstants.BooksListKey,
            CacheConstants.DefaultTtl,
            async ct => await _q.GetBooksAsync(ct),
            ct);
    }

    public async Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheConstants.AuthorsListKey,
            CacheConstants.DefaultTtl,
            async ct => await _q.GetAuthorsAsync(ct),
            ct);
    }
}
```

#### Pages/Components (Presentation Layer)
```razor
@* Location: /src/Pages/Recipes.razor *@
@page "/recipes"
@inject IRecipeService RecipeService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<PageTitle>Recipes - Mes Recettes</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h3" Class="mb-6">My Recipes</MudText>
    
    <MudStack Row Justify="Justify.SpaceBetween" AlignItems="Center" Class="mb-4">
        <MudTextField @bind-Value="searchTerm" 
                      Placeholder="Search recipes..." 
                      Adornment="Adornment.Start" 
                      AdornmentIcon="Icons.Material.Filled.Search"
                      Immediate="true"
                      OnTextChanged="@(async () => await SearchRecipes())" />
        
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="Icons.Material.Filled.Add"
                   OnClick="OpenAddDialog">
            Add Recipe
        </MudButton>
    </MudStack>
    
    @if (loading)
    {
        <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="400px" />
    }
    else if (error != null)
    {
        <MudAlert Severity="Severity.Error" Class="my-4">
            @error
        </MudAlert>
    }
    else
    {
        <MudDataGrid Items="@filteredRecipes" 
                     Filterable="true" 
                     SortMode="SortMode.Multiple"
                     Pagination="true"
                     PageSize="10">
            <Columns>
                <PropertyColumn Property="x => x.Name" Title="Name" />
                <PropertyColumn Property="x => x.Rating" Title="Rating">
                    <CellTemplate>
                        @if (context.Item.Rating.HasValue)
                        {
                            <MudRating SelectedValue="context.Item.Rating.Value" ReadOnly="true" />
                        }
                        else
                        {
                            <MudText Typo="Typo.body2" Color="Color.Secondary">Not rated</MudText>
                        }
                    </CellTemplate>
                </PropertyColumn>
                <PropertyColumn Property="x => x.Book!.Title" Title="Book" />
                <TemplateColumn Title="Actions" Sortable="false">
                    <CellTemplate>
                        <MudButtonGroup>
                            <MudIconButton Icon="Icons.Material.Filled.Visibility" 
                                         Color="Color.Primary"
                                         OnClick="@(() => ViewRecipe(context.Item.Id))" />
                            <MudIconButton Icon="Icons.Material.Filled.Edit" 
                                         Color="Color.Secondary"
                                         OnClick="@(() => EditRecipe(context.Item))" />
                            <MudIconButton Icon="Icons.Material.Filled.Delete" 
                                         Color="Color.Error"
                                         OnClick="@(() => DeleteRecipe(context.Item))" />
                        </MudButtonGroup>
                    </CellTemplate>
                </TemplateColumn>
            </Columns>
        </MudDataGrid>
    }
</MudContainer>

@code {
    private bool loading = true;
    private string? error;
    private string searchTerm = string.Empty;
    private List<Recipe> recipes = new();
    private List<Recipe> filteredRecipes = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadRecipes();
    }
    
    private async Task LoadRecipes()
    {
        try
        {
            loading = true;
            error = null;
            recipes = await RecipeService.GetAllAsync();
            filteredRecipes = recipes;
        }
        catch (Exception ex)
        {
            error = "Failed to load recipes. Please try again.";
        }
        finally
        {
            loading = false;
        }
    }
    
    private async Task SearchRecipes()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredRecipes = recipes;
        }
        else
        {
            filteredRecipes = recipes
                .Where(r => r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        StateHasChanged();
    }
    
    private async Task OpenAddDialog()
    {
        var dialog = await DialogService.ShowAsync<AddRecipeDialog>("Add New Recipe");
        var result = await dialog.Result;
        
        if (!result.Canceled && result.Data is Recipe newRecipe)
        {
            await LoadRecipes();
            Snackbar.Add("Recipe added successfully!", Severity.Success);
        }
    }
}
```

### Recipe Source Management
Recipes can have mutually exclusive sources: books, stores/restaurants, websites, or none. When implementing recipe creation/editing:

1. **Source Selection UI**: Use radio buttons for source type selection (book/store/url/none)
2. **Conditional Fields**: Show relevant input fields based on selected source type
3. **Validation**: Ensure only one source type is set, clear other source fields when switching
4. **Display**: Show appropriate icons and links based on source type (📖 book, 🏪 store, 🌐 website, 🏠 none)
5. **URL Handling**: Validate URLs and provide clickable external links for website sources

#### Source Type Priority (for display/sorting)
1. Book sources (highest priority)
2. Store sources  
3. Website sources
4. No source (home/family recipes)

### 🧪 Testing Patterns

#### Model Tests
```csharp
// Location: /tests/Models/RecipeModelTests.cs
public class RecipeModelTests
{
    [Fact]
    public void Recipe_ShouldHaveCorrectTableAttribute()
    {
        // Arrange & Act
        var tableAttribute = typeof(Recipe).GetCustomAttribute<TableAttribute>();
        
        // Assert
        Assert.NotNull(tableAttribute);
        Assert.Equal("recettes", tableAttribute.Name);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Recipe_Rating_ValidValues_ShouldPass(int rating)
    {
        // Arrange
        var recipe = new Recipe { Rating = rating };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Recipe_Rating_InvalidValues_ShouldFail(int rating)
    {
        // Arrange
        var recipe = new Recipe { Rating = rating };
        var context = new ValidationContext(recipe);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(recipe, context, results, true);
        
        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Recipe.Rating)));
    }
}
```

#### Service Tests
```csharp
// Location: /tests/Services/RecipeServiceTests.cs
public class RecipeServiceTests
{
    private readonly IRecipeService _recipeService;
    private readonly SupabaseClient _mockSupabaseClient;
    private readonly ILogger<RecipeService> _mockLogger;
    
    public RecipeServiceTests()
    {
        _mockSupabaseClient = Substitute.For<SupabaseClient>("", "");
        _mockLogger = Substitute.For<ILogger<RecipeService>>();
        _recipeService = new RecipeService(_mockSupabaseClient, _mockLogger);
    }
    
    [Fact]
    public async Task GetAllAsync_Success_ReturnsRecipes()
    {
        // Arrange
        var expectedRecipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Test Recipe 1" },
            new() { Id = 2, Name = "Test Recipe 2" }
        };
        
        var mockResponse = new ModeledResponse<Recipe>();
        mockResponse.Models = expectedRecipes;
        
        _mockSupabaseClient.From<Recipe>().Returns(Substitute.For<ISupabaseTable<Recipe>>());
        _mockSupabaseClient.From<Recipe>().Get().Returns(mockResponse);
        
        // Act
        var result = await _recipeService.GetAllAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test Recipe 1", result[0].Name);
        Assert.Equal("Test Recipe 2", result[1].Name);
    }
    
    [Fact]
    public async Task GetAllAsync_Exception_ThrowsApplicationException()
    {
        // Arrange
        _mockSupabaseClient.From<Recipe>().Returns(Substitute.For<ISupabaseTable<Recipe>>());
        _mockSupabaseClient.From<Recipe>().Get().Throws(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(() => _recipeService.GetAllAsync());
        Assert.Equal("Failed to load recipes", exception.Message);
    }
}
```

## Business Domain

### Core Entities
```mermaid
erDiagram
    Author ||--o{ Book : "writes"
    Book ||--o{ Recipe : "contains"
    Recipe {
        int id PK
        string name
        string ingredients
        string instructions
        int rating
        string notes
        int pageNumber
        datetime createdAt
        datetime updatedAt
    }
    Book {
        int id PK
        string title
## Project Structure
```
/
├── src/
│   ├── Models/              # Data models (Recipe, Book, Author) - all in Recette.cs
│   ├── Pages/               # Blazor pages and dialogs
│   ├── Services/            # Business logic and data access
│   │   ├── Abstractions/    # Service interfaces (IRecipeService, IRecipesQuery, etc.)
│   │   ├── Exceptions/      # Custom exceptions
│   │   ├── RecipeService.cs # Main recipe business logic
│   │   ├── CacheService.cs  # In-memory caching
│   │   ├── SupabaseRecipesQuery.cs # Data access queries
│   │   ├── Result.cs        # Result<T> pattern implementation
│   │   └── ServiceConstants.cs # Constants (Cache, Pagination, Sort)
│   ├── Layout/              # App layout components
│   ├── Configuration/       # App configuration
│   ├── Shared/              # Shared components
│   └── wwwroot/             # Static files and assets
└── tests/                   # Unit and integration tests
    ├── Services/            # Service layer tests
    ├── Pages/               # Component tests
    ├── Models/              # Model validation tests
    └── Integration/         # Integration tests
```
### Business Rules
1. **Recipe Rating**: Must be between 1-5 stars (inclusive)
2. **Author Names**: First and last name are required
3. **Book-Recipe Association**: Recipes can exist without a book reference
4. **Page Numbers**: Optional, used for physical cookbook references
5. **Data Integrity**: Soft delete pattern for maintaining referential integrity

### User Journey Mapping
```
Discovery → Collection → Organization → Usage → Sharing
    ↓           ↓            ↓           ↓        ↓
  Browse    Add Recipe   Categorize   Cook    Print/Export
  Search    Rate/Review  Tag/Label    Rate    Share Link
  Filter    Take Notes   Group        Note    Archive
```

### User Workflows
1. **Recipe Discovery & Addition**
   - Browse existing recipes
   - Add new recipes with detailed information
   - Associate with books/authors when applicable
   - Upload or reference images

2. **Organization & Management**
   - Categorize recipes by cuisine, type, or custom tags
   - Rate recipes after cooking (1-5 stars)
   - Add personal notes and modifications
   - Track cooking history and favorites

3. **Search & Filtering**
   - Quick search by name, ingredients, or author
   - Advanced filtering by rating, book, cuisine
   - Sort by rating, date added, or alphabetical
   - Bookmark frequently accessed recipes

4. **Cooking & Usage**
   - Print-friendly recipe view
   - Mobile-optimized cooking mode
   - Timer integration (future feature)
   - Shopping list generation (future feature)

5. **Data Management**
   - Import/export recipes
   - Backup personal data
   - Sync across devices (if cloud storage enabled)

### Business Constraints
- **Offline Limitations**: Requires internet for Supabase operations
- **Data Ownership**: User owns all recipe data
- **Privacy**: No sharing features implemented (personal use only)
- **Scalability**: Designed for personal collections (thousands, not millions of recipes)

## Development Environment

### UI/UX Guidelines
- Use Material Design principles via MudBlazor components exclusively
- Maintain consistent spacing and typography throughout the application
- Provide clear navigation between related entities (recipes, books, authors)
- Include confirmation dialogs for destructive actions
- Show loading states for async operations with proper indicators
- Display meaningful error messages to users with user-friendly language
- Keep the interface clean and uncluttered for home cook user persona
- Prioritize simplicity and ease of use in all interactions

### Technical Constraints
- Client-side Blazor WebAssembly (no server-side rendering)
- Supabase requires internet connectivity for all data operations
- Browser storage limitations for offline scenarios
- Single-page application navigation patterns
- Consider both digital and physical cookbook workflows in design

### Common User Stories
- "As a user, I want to quickly find recipes by rating or author"
- "As a user, I want to see all recipes from a specific cookbook"
- "As a user, I want to add my own notes to existing recipes"
- "As a user, I want to print a recipe without the website formatting"
- "As a user, I want to rate recipes after I've tried them"
- "As a user, I want to save recipes from websites with clickable links"
- "As a user, I want to distinguish between book, store, and online recipes"
