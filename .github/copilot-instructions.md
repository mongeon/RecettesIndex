# AI Agent Instructions for Mes Recettes

> **Version**: 4.0 | **Last Updated**: November 16, 2025 | **Status**: Active | **AI-Optimized**: ✅

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
        public int? BookPage { get; set; }

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

## Development Environment
- **IDE**: Visual Studio Code with C# Dev Kit
- **Framework**: .NET 9.0 Blazor WebAssembly
- **Package Manager**: NuGet
- **Database**: Supabase (PostgreSQL)
- **GitHub Operations**: Use GitHub MCP server for all GitHub-related tasks (PRs, issues, etc.)

## Project Structure
```
/
├── Models/           # Data models (Recipe, Book, Author)
├── Pages/           # Blazor pages and dialogs
├── Services/        # Business logic and data access
├── Layout/          # App layout components
├── Configuration/   # App configuration
├── Shared/          # Shared components
├── wwwroot/         # Static files and assets
└── Resources/       # Localization resources
```
#### Namespace Declarations
- **Always use file-scoped namespaces** (C# 10+):
```csharp
// ✅ Correct - File-scoped namespace
namespace RecettesIndex.Services;

public class RecipeService(IRecipesQuery query, ILogger<RecipeService> logger) : IRecipeService
{
    // Implementation
}

// ❌ Incorrect - Do not use block-scoped namespaces
namespace RecettesIndex.Services
{
    public class RecipeService : IRecipeService
    {
        // Implementation
    }
}

// ❌ Also Incorrect - Models should use block-scoped for multiple types
#### Constructor Parameter Validation with Primary Constructors
- **Always validate dependencies with ArgumentNullException** (using C# 12 primary constructors):
```csharp
// ✅ Correct - Primary constructor with null validation
public class RecipeService(
    IRecipesQuery query, 
    ICacheService cache, 
    Supabase.Client supabaseClient, 
    ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _query = query ?? throw new ArgumentNullException(nameof(query));
    private readonly ICacheService _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly Supabase.Client _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
    private readonly ILogger<RecipeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}

// ❌ Incorrect - Missing null validation
public class RecipeService(IRecipesQuery query, ICacheService cache, Supabase.Client supabaseClient, ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _query = query;
    private readonly ICacheService _cache = cache;
    private readonly Supabase.Client _supabaseClient = supabaseClient;
    private readonly ILogger<RecipeService> _logger = logger;
}

// ❌ Also Incorrect - Old-style constructor (use primary constructors)
public class RecipeService : IRecipeService
{
    private readonly IRecipesQuery _query;
    
    public RecipeService(IRecipesQuery query)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
    }
}
```
#### Constructor Parameter Validation
- **Always validate dependencies with ArgumentNullException**:
```csharp
// ✅ Correct
public RecipeService(IRecipesQuery query, ICacheService cache, Client supabaseClient, ILogger<RecipeService> logger)
{
    _query = query ?? throw new ArgumentNullException(nameof(query));
    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
#### Private Field Naming
- **Use descriptive names for Supabase client and follow naming conventions**:
```csharp
// ✅ Correct - Fully qualified with descriptive name
private readonly Supabase.Client _supabaseClient;

// ✅ Also Correct - With using directive
using Supabase;
private readonly Client _supabaseClient;

// ❌ Incorrect - Too generic
private readonly Client _client;

// ❌ Incorrect - Wrong casing
private readonly Supabase.Client supabaseClient;
``` _cache = cache;
    _supabaseClient = supabaseClient;
    _logger = logger;
}
```

#### Logger Parameters
- **Loggers are always required, never optional**:
```csharp
// ✅ Correct
public RecipeService(Client supabaseClient, ILogger<RecipeService> logger)

// ❌ Incorrect - Do not make loggers optional
public RecipeService(Client supabaseClient, ILogger<RecipeService>? logger = null)
```

#### Private Field Naming
- **Use descriptive names for Supabase client**:
```csharp
// ✅ Correct
private readonly Client _supabaseClient;

// ❌ Incorrect - Too generic
private readonly Client _client;
```

#### Razor Component Directives
- **Standard ordering: @page → @using (alphabetical) → @inject (alphabetical)**:
```razor
@* ✅ Correct *@
@page "/recipes"
@using Microsoft.Extensions.Logging
@using MudBlazor
@using RecettesIndex.Models
@using RecettesIndex.Services
@using Supabase
@inject AuthService AuthService
@inject IDialogService DialogService
@inject ILogger<Recipes> Logger
@inject Client SupabaseClient

@* ❌ Incorrect - Wrong order and fully-qualified names *@
@page "/recipes"
@using MudBlazor
@inject Supabase.Client SupabaseClient
### C# Conventions
- Use nullable reference types throughout
- Enable implicit usings
- Follow PascalCase for public members, _camelCase for private fields
- Use async/await for all I/O operations with CancellationToken support
- Implement proper disposal patterns for resources
- **Use C# 12 primary constructors** for all new services
- **Use file-scoped namespaces** for all files (except Models/Recette.cs which has multiple types)
- **Use Result<T> pattern** for all service method return values
- **Add XML documentation comments** to all public members
- **Organize constants** in dedicated static classes (CacheConstants, PaginationConstants, RecipeSortConstants)not fully-qualified names**:
```razor
@* ✅ Correct *@
@using Supabase
@inject Client SupabaseClient

@* ❌ Incorrect *@
@inject Supabase.Client SupabaseClient
```

#### Coding Style Enforcement
- All C# files follow `.editorconfig` rules
- File-scoped namespaces: **required** (warning level)
- Private fields: must use `_camelCase` (warning level)
- Interfaces: must start with `I` (warning level)
- Use `dotnet format` before committing to ensure compliance

### C# Conventions
- Use nullable reference types throughout
- Enable implicit usings
- Follow PascalCase for public members, camelCase for private
- Use async/await for all I/O operations
- Implement proper disposal patterns for resources

### Blazor Patterns
```csharp
// Component parameters
[Parameter] public string Title { get; set; } = string.Empty;

// Event callbacks
[Parameter] public EventCallback<Recipe> OnRecipeSelected { get; set; }

// Dependency injection with @inject directive
@inject IRecipeService RecipeService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

// Lifecycle methods with async/await and CancellationToken
protected override async Task OnInitializedAsync()
{
    await LoadDataAsync();
}

// Server-side data loading for MudTable
private async Task<TableData<Recipe>> LoadServerData(TableState state, CancellationToken ct)
{
    var result = await RecipeService.SearchAsync(
        searchTerm, 
        ratingFilter, 
        bookFilter, 
        authorFilter, 
        state.Page + 1, 
        state.PageSize, 
        state.SortLabel, 
        state.SortDirection == SortDirection.Descending,
        ct);
    
    return result.IsSuccess 
        ? new TableData<Recipe> { Items = result.Value.Items, TotalItems = result.Value.Total }
        : new TableData<Recipe> { Items = Array.Empty<Recipe>(), TotalItems = 0 };
}
```

### Database Patterns
```csharp
// Model definition with comprehensive attributes
[Table("recettes")]
public class Recipe : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("name")]
    [Required(ErrorMessage = "The Name field is required.")]
    public string Name { get; set; } = string.Empty;
    
    [Column("rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [Column("book_id")]
    public int? BookId { get; set; }
### UI Patterns
- Use MudBlazor components exclusively for UI
- Implement consistent dialog patterns for CRUD operations
- Use MudTable with server-side data loading (ServerData parameter)
- Implement proper loading states and error handling
- Use MudSnackbar for user notifications
- Apply consistent spacing with MudStack and Class properties

### Result<T> Pattern for Error Handling
```csharp
// Service methods return Result<T>
public async Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default)
{
    try
    {
        var recipe = await _q.GetRecipeByIdAsync(id, ct);
        return recipe != null 
            ? Result<Recipe>.Success(recipe) 
            : Result<Recipe>.Failure("Recipe not found");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get recipe {Id}", id);
        return Result<Recipe>.Failure("An error occurred while loading the recipe");
    }
}

// Consumers check IsSuccess
var result = await RecipeService.GetByIdAsync(id);
### Example Component Structure (Server-Side Data Loading)
```razor
@page "/recipes"
@using MudBlazor
@using RecettesIndex.Models
@using RecettesIndex.Services
@using RecettesIndex.Services.Abstractions
@inject IRecipeService RecipeService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<PageTitle>Recettes</PageTitle>

<MudPaper Class="pa-4">
    <MudText Typo="Typo.h5">Recettes</MudText>
    
    <MudStack Row AlignItems="AlignItems.Center" Spacing="2" Class="mb-2">
        <MudTextField T="string" 
                      Value="@searchTerm" 
                      ValueChanged="OnSearchChanged" 
                      Placeholder="Rechercher des recettes..." 
                      Adornment="Adornment.Start" 
                      AdornmentIcon="@Icons.Material.Filled.Search" />
        
        <MudSelect T="string" 
                   Value="@ratingFilter" 
                   ValueChanged="OnRatingChanged" 
                   Clearable="true" 
                   Dense="true" 
                   Label="Évaluation">
            <MudSelectItem T="string" Value='@("all")'>Toutes</MudSelectItem>
            <MudSelectItem T="string" Value='@("5")'>5 étoiles</MudSelectItem>
        </MudSelect>
    </MudStack>
    
    <MudTable T="Recipe" 
              ServerData="LoadServerData" 
              Hover="true" 
              Dense="true" 
              RowsPerPage="20" 
              @ref="table">
        <HeaderContent>
            <MudTh><MudTableSortLabel T="Recipe" SortLabel="name">Nom</MudTableSortLabel></MudTh>
            <MudTh><MudTableSortLabel T="Recipe" SortLabel="rating">Évaluation</MudTableSortLabel></MudTh>
        </HeaderContent>
        <RowTemplate>
## Error Handling
- Always wrap async operations in try-catch blocks
- Use Result<T> pattern for service methods to encapsulate success/failure
- Provide user-friendly error messages via Result<T>.Failure()
- Log errors with structured logging (ILogger)
- Display errors to users via MudSnackbar with Severity.Error
- Never throw exceptions from services - return Result<T>.Failure instead
- Handle CancellationToken properly in all async methods

### Error Handling Pattern
```csharp
// Service layer
public async Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default)
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(recipe.Name))
            return Result<Recipe>.Failure("Recipe name is required");
        
        // Operation
        var updated = await _supabaseClient
            .From<Recipe>()
            .Update(recipe);
        
        return Result<Recipe>.Success(updated);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to update recipe {Id}", recipe.Id);
        return Result<Recipe>.Failure("An error occurred while updating the recipe");
    }
}

// UI layer
private async Task SaveRecipe()
{
    var result = await RecipeService.UpdateAsync(recipe);
    if (result.IsSuccess)
    {
        Snackbar.Add("Recipe updated successfully!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(result.Value));
    }
    else
    {
        Snackbar.Add(result.ErrorMessage, Severity.Error);
    }
}
```ormat="Affichage {first_item}-{last_item} sur {all_items}" />
        </PagerContent>
    </MudTable>
</MudPaper>

@code {
    private MudTable<Recipe>? table;
    private string searchTerm = string.Empty;
    private string ratingFilter = "all";
    
    private async Task<TableData<Recipe>> LoadServerData(TableState state, CancellationToken ct)
    {
        int? rating = ratingFilter == "all" ? null : int.Parse(ratingFilter);
        
        var result = await RecipeService.SearchAsync(
            searchTerm,
            rating,
            null, // bookId
            null, // authorId
            state.Page + 1,
            state.PageSize,
            state.SortLabel,
            state.SortDirection == SortDirection.Descending,
## Performance Considerations
- Use async patterns consistently with CancellationToken support
- Implement server-side pagination for MudTable (ServerData parameter)
- Use in-memory caching (ICacheService) for frequently accessed reference data
- Apply proper filtering at the query level before loading into memory
- Use IReadOnlyList<T> for collections that won't be modified
- Consider using HashSet<int> for efficient ID-based filtering
- Invalidate cache entries when underlying data changes
- Use MudTable instead of MudDataGrid for better performance with large datasets
- Clamp pagination parameters to prevent excessive data loading (PaginationConstants)
            { 
                Items = result.Value.Items, 
                TotalItems = result.Value.Total 
            };
        }
        
        Snackbar.Add(result.ErrorMessage, Severity.Error);
        return new TableData<Recipe> { Items = Array.Empty<Recipe>(), TotalItems = 0 };
    }
    
    private async Task OnSearchChanged(string value)
    {
### NSubstitute Testing Patterns (Updated)
```csharp
// Basic substitution for interfaces
var mockQuery = Substitute.For<IRecipesQuery>();

// Setup method returns with CancellationToken
mockQuery.GetRecipeByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Recipe { Id = 1 });

// Verify method calls
mockQuery.Received(1).GetRecipeByIdAsync(1, Arg.Any<CancellationToken>());

// Throw exceptions
mockQuery.GetRecipeByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
    .Throws(new Exception("Database error"));

// Use real implementations where appropriate
var cache = new CacheService(); // Simple in-memory cache, no need to mock

// For Supabase.Client (sealed class), use real instance with fake URL
var client = new Supabase.Client("http://localhost", "test-key", new SupabaseOptions());
```Query pattern for data access (separates query logic from business logic)
public interface IRecipesQuery
{
    Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct);
    Task<List<int>> GetRecipeIdsByNameAsync(string term, int? rating, CancellationToken ct);
    Task<List<Recipe>> GetRecipesByIdsAsync(List<int> ids, CancellationToken ct);
    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct);
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct);
}

// Supabase data access implementation
var response = await _supabaseClient
    .From<Recipe>()
    .Where(r => r.Id == id)
    .Single();
```

### UI Patterns
- Use MudBlazor components exclusively for UI
- Implement consistent dialog patterns for CRUD operations
- Use MudDataGrid for data display
- Implement proper loading states and error handling

### Example Component Structure
```razor
@page "/recipes"
@inject SupabaseClient SupabaseClient

<MudContainer>
    <MudText Typo="Typo.h4">Recipes</MudText>
    
    @if (loading)
    {
        <MudProgressLinear Indeterminate="true" />
    }
    else
    {
        <MudDataGrid Items="recipes" />
    }
</MudContainer>

@code {
    private bool loading = true;
    private List<Recipe> recipes = new();
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await SupabaseClient.From<Recipe>().Get();
            recipes = response.Models;
        }
        catch (Exception ex)
        {
            // Handle error
        }
### Testing Complex Dependencies
When testing services that depend on complex external libraries (like Supabase.Client), follow these patterns:
- **Query pattern**: Separate query logic into IRecipesQuery interface for easier mocking
- **Real instances**: Use real Supabase.Client instances with dummy URLs for constructor satisfaction
- **Focus on logic**: Test business logic, parameter validation, and error handling rather than Supabase internals
- **Simple implementations**: Use real CacheService (simple in-memory) instead of mocking
- **Integration tests**: Create separate integration tests for actual Supabase interactions

### Current Testing Architecture
```csharp
public class RecipeServiceTests
{
    private readonly IRecipesQuery _query;        // Mock this
    private readonly ICacheService _cache;        // Use real CacheService
    private readonly Supabase.Client _client;     // Real instance with dummy URL
    private readonly ILogger<RecipeService> _logger; // Mock this
    private readonly RecipeService _service;
    
    public RecipeServiceTests()
    {
        _query = Substitute.For<IRecipesQuery>();
        _cache = new CacheService(); // Real implementation
        _client = new Supabase.Client("http://localhost", "test-key", new SupabaseOptions());
        _logger = Substitute.For<ILogger<RecipeService>>();
        _service = new RecipeService(_query, _cache, _client, _logger);
### Service Implementation (Updated Pattern)
```csharp
// Use Query pattern to separate data access
public interface IRecipesQuery
{
    Task<List<int>> GetAllRecipeIdsAsync(int? rating, CancellationToken ct);
    Task<List<Recipe>> GetRecipesByIdsAsync(List<int> ids, CancellationToken ct);
}

// Service uses query abstraction and caching
public class RecipeService(
    IRecipesQuery query, 
    ICacheService cache,
    Supabase.Client supabaseClient, 
    ILogger<RecipeService> logger) : IRecipeService
{
    private readonly IRecipesQuery _query = query ?? throw new ArgumentNullException(nameof(query));
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
            // Clamp parameters
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);
            
            // Use query abstraction
            var ids = await _query.GetAllRecipeIdsAsync(rating, ct);
            var recipes = await _query.GetRecipesByIdsAsync(ids, ct);
            
            // Business logic for sorting, pagination
            var sorted = ApplySorting(recipes, sortLabel, sortDescending);
            var paginated = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return Result<(IReadOnlyList<Recipe>, int)>.Success((paginated, recipes.Count));
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
            async ct => await _query.GetBooksAsync(ct),
            ct);
    }
}
```
}
```

## Authentication Patterns
- Use Supabase Auth if authentication is implemented
- Protect routes appropriately
- Handle auth state changes

## Error Handling
- Always wrap async operations in try-catch
- Provide user-friendly error messages
- Log errors appropriately
- Implement graceful degradation

## Performance Considerations
- Use async patterns consistently
- Implement proper loading states
- Consider virtualization for large data sets
- Optimize Supabase queries with proper filtering

## Testing Guidelines
- **Write comprehensive unit tests for ALL business logic** - every new feature must have corresponding tests
- **Use xUnit framework** with proper test organization and naming conventions
- **Use NSubstitute for mocking** - preferred over Moq for cleaner syntax and better maintainability
- **Test file naming**: Match class names (e.g., `RecipeModelTests.cs` for `Recipe` model)
- **Test method naming**: Use descriptive names that explain what is being tested
- **Follow Arrange-Act-Assert pattern** in all test methods
- **Test both positive and negative scenarios** - valid inputs and edge cases
- **Validate business rules** - especially data validation attributes and constraints
- **Use Theory tests** with `[InlineData]` for testing multiple scenarios
- **Test Blazor components with bUnit** when component testing is needed
- **Mock dependencies carefully** - some complex types like Supabase.Client may require wrapper interfaces for effective testing
- **Test error scenarios** and exception handling
- **Achieve high test coverage** - aim for comprehensive coverage of models, services, and business logic
- **Run tests before committing** - `dotnet test` must pass before any PR creation

### NSubstitute Testing Patterns
```csharp
// Basic substitution
var mockService = Substitute.For<IService>();

// Setup method returns
mockService.Method(Arg.Any<string>()).Returns("result");

// Verify method calls
mockService.Received(1).Method("expected");

// Throw exceptions
mockService.Method().Throws(new Exception("error"));
```

### Testing Complex Dependencies
When testing services that depend on complex external libraries (like Supabase.Client), consider:
- Creating wrapper interfaces for better testability
- Testing integration points separately from unit tests
- Using partial mocks or test doubles for complex scenarios

## Common Patterns to Follow

### Dialog Implementation
```csharp
// Dialog component
public class AddRecipeDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    
    private async Task Submit()
    {
        // Validation and submission logic
        MudDialog.Close(DialogResult.Ok(recipe));
    }
    
    private void Cancel() => MudDialog.Cancel();
}
```

### Service Implementation
```csharp
public class RecipeService
{
    private readonly SupabaseClient _supabaseClient;
    
    public RecipeService(SupabaseClient supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }
    
    public async Task<List<Recipe>> GetRecipesAsync()
    {
        var response = await _supabaseClient.From<Recipe>().Get();
        return response.Models;
    }
}
```

## When Providing Code Suggestions
1. **Consistency**: Match existing code style and patterns
2. **MudBlazor First**: Use MudBlazor components over HTML
3. **Async/Await**: Use async patterns for all I/O
4. **Error Handling**: Include try-catch blocks
5. **Nullable Types**: Use nullable reference types appropriately
6. **Loading States**: Include loading indicators
7. **Supabase Patterns**: Follow established data access patterns
8. **Component Structure**: Follow the existing component architecture

## Development Workflow
- **Never work directly on main branch** - always create feature branches
- Use descriptive branch names (e.g., `feature/add-recipe-search`, `fix/rating-validation`)
- **Always validate changes before committing** - use `git diff`, `git show --stat`, or `git status` to review what was changed
- **Run the application to test changes** - use `dotnet run` or appropriate commands to validate functionality
- **Write comprehensive unit tests** - add unit tests for ALL new functionality before creating PRs
- **Achieve high test coverage** - aim for comprehensive coverage of business logic, models, and services
- **Run tests before committing** - ensure `dotnet test` passes with all tests
- **Confirm changes with user before creating commits or PRs** - show validation results and get approval
- Make atomic commits with clear messages
- Test changes before suggesting merge to main
- **Use GitHub MCP server** for all GitHub operations (creating PRs, issues, reviews, etc.)
- Follow existing pull request patterns if established

### Unit Testing Requirements
- **Mandatory for all PRs**: No pull request should be created without comprehensive unit tests
- **Test file naming**: Use descriptive names matching the classes being tested (e.g., `RecipeModelTests.cs`, `AuthorServiceTests.cs`)
- **Test organization**: Separate test files for different concerns (models, services, components)
- **Validation testing**: Test both valid and invalid scenarios, especially for business rules
- **Edge cases**: Include tests for boundary conditions and error scenarios
- **Test patterns to follow**:
  - Arrange-Act-Assert pattern
  - Descriptive test method names
  - Theory tests for multiple data scenarios
  - Proper use of xUnit attributes
- **Current test structure**: Follow the established pattern in `/tests/` directory
- **Test coverage areas**:
  - Model validation and business rules
  - Service layer functionality
  - Data access patterns
  - Component behavior
  - API endpoints (when applicable)
- **Run tests**: Always run `dotnet test` before committing and ensure all tests pass

### GitHub Operations
- **Always use GitHub MCP server tools** for:
  - Creating pull requests (`mcp_github_create_pull_request`)
  - Managing issues (`mcp_github_create_issue`, `mcp_github_update_issue`)
  - Adding comments (`mcp_github_add_issue_comment`)
  - Repository management and operations
- **Never use git CLI commands** for GitHub-specific operations when MCP server is available
- **Example**: Instead of `gh pr create`, use `mcp_github_create_pull_request`

### Documentation Requirements
- **Single Source of Truth**: This file (`.github/copilot-instructions.md`) is the ONLY copilot instruction file - do not create or maintain multiple instruction files
- **Update Project Documentation**: For every modification, ensure documentation in the `/docs` folder is updated to reflect:
  - New features and functionality
  - API changes or new endpoints
  - Configuration changes
  - Setup or deployment procedures
  - User-facing changes or new workflows
- **Update README files**: Keep both the main `README.md` and `docs/README.md` synchronized with:
  - Feature additions or removals
  - Setup instruction changes
  - New dependencies or requirements
  - Architecture or technology changes
- **Documentation Consistency**: Keep all documentation synchronized with code changes to maintain accuracy and usefulness for future development

## Common Scenarios

### 🔄 Typical AI Tasks

#### Adding a New Feature
1. **Analyze requirements** - Understand the business need and user story
2. **Check existing patterns** - Look for similar implementations in the codebase
3. **Create interface first** - Define the contract before implementation
4. **Implement service layer** - Add business logic with proper error handling
5. **Create/update UI components** - Use MudBlazor components consistently
6. **Write comprehensive tests** - Cover all scenarios including edge cases
7. **Update documentation** - Reflect changes in relevant docs
8. **Create feature branch** - Never work directly on main
9. **Test thoroughly** - Run application and all tests
10. **Create PR with tests** - Use GitHub MCP server

#### Debugging Issues
1. **Identify the problem scope** - UI, service, or data layer
2. **Check error logs** - Look for exceptions and structured logging
3. **Verify data flow** - Trace from UI to database and back
4. **Test with minimal data** - Isolate the issue
5. **Check business rules** - Ensure validation is working correctly
6. **Review recent changes** - Use git history to identify regressions

#### Refactoring Code
1. **Write tests first** - Ensure current behavior is captured
2. **Make small, atomic changes** - One refactoring concern at a time
3. **Maintain public APIs** - Don't break existing contracts
4. **Update related tests** - Keep tests in sync with changes
5. **Run full test suite** - Ensure nothing is broken
6. **Update documentation** - Reflect architectural changes

### 🎯 Quick Decision Guide

| Question | Answer | Reasoning |
|----------|--------|-----------|
| Should I use HTML or MudBlazor? | Always MudBlazor | Consistent Material Design |
| Sync or async method? | Always async for I/O | Better UX and scalability |
| How to handle errors? | Try-catch with user-friendly messages | Better user experience |
| Where to put business logic? | Service layer | Separation of concerns |
| How to test Supabase calls? | Mock with NSubstitute | Isolated unit tests |
| Create new branch? | Always for features/fixes | Safe development workflow |
| Use git CLI for GitHub? | No, use MCP server | Consistent tooling |

### 🧩 Code Snippets Library

#### Quick Error Handler
```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    _snackbar.Add("Something went wrong. Please try again.", Severity.Error);
}
```

#### Standard Service Method
```csharp
public async Task<ServiceResult<T>> GetAsync(int id)
{
    try
    {
        var item = await _supabaseClient.From<T>().Where(x => x.Id == id).Single();
        return ServiceResult<T>.Success(item);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get {Type} with id {Id}", typeof(T).Name, id);
        return ServiceResult<T>.Error("Failed to load item");
    }
}
```

#### Loading State Pattern
```csharp
private bool _loading = true;
private string? _error;

protected override async Task OnInitializedAsync()
{
    try
    {
        _loading = true;
        _error = null;
        // Load data
    }
    catch (Exception ex)
    {
        _error = "Failed to load data";
    }
    finally
    {
        _loading = false;
        StateHasChanged();
    }
}
```

## Troubleshooting Guide

### 🐛 Common Issues & Solutions

#### Compilation Errors
- **Missing using statements**: Check _Imports.razor for global usings
- **Nullable reference warnings**: Ensure proper null handling with `?` and `!`
- **Attribute errors**: Verify Supabase table/column attributes match database schema

#### Runtime Errors
- **Supabase connection**: Check configuration and internet connectivity
- **Authentication issues**: Verify Supabase credentials in appsettings.json
- **Component lifecycle**: Ensure proper disposal of resources

#### Testing Issues
- **NSubstitute setup**: Complex types may need wrapper interfaces
- **Async test failures**: Ensure proper await usage in test methods
- **Missing test coverage**: Check that all business logic has corresponding tests

#### Performance Issues
- **Slow UI updates**: Ensure proper use of StateHasChanged()
- **Large data loading**: Consider pagination and filtering
- **Memory leaks**: Verify proper disposal of services and components

### 🔍 Debugging Strategies

#### For Service Layer Issues
1. Check logs for detailed error messages
2. Verify Supabase query syntax and table names
3. Test with simple data first
4. Use browser dev tools to inspect network requests

#### For UI Issues
1. Use browser dev tools to inspect component state
2. Check for console errors and warnings
3. Verify MudBlazor component parameters
4. Test responsive behavior on different screen sizes

#### For Test Failures
1. Run tests individually to isolate failures
2. Check test data setup and mocking configuration
3. Verify async/await patterns in test methods
4. Ensure proper arrange-act-assert structure

### 📋 Pre-commit Checklist

- [ ] All code compiles without warnings
- [ ] All unit tests pass (`dotnet test`)
- [ ] Application runs successfully (`dotnet run`)
- [ ] New features have comprehensive tests
- [ ] UI follows MudBlazor patterns
- [ ] Error handling is implemented
- [ ] Logging is added where appropriate
- [ ] Documentation is updated
- [ ] Branch is up to date with main
- [ ] Changes are reviewed and validated

### 🚨 Red Flags to Avoid

- ❌ Working directly on main branch
- ❌ Using raw HTML instead of MudBlazor
- ❌ Synchronous I/O operations
- ❌ Missing error handling
- ❌ No unit tests for new code
- ❌ Hardcoded configuration values
- ❌ Breaking existing functionality
- ❌ Inconsistent naming conventions
- ❌ Missing null checks
- ❌ Inadequate logging
