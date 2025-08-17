# AI Agent Instructions for Mes Recettes

> **Version**: 3.0 | **Last Updated**: August 2025 | **Status**: Active | **AI-Optimized**: ✅

## 🤖 AI Agent Quick Reference

### Immediate Context
- **Project Type**: Blazor WebAssembly (.NET 9.0) with Supabase backend
- **Primary Language**: C# with Razor components
- **UI Framework**: MudBlazor (Material Design)
- **Database**: PostgreSQL via Supabase
- **Test Framework**: xUnit with NSubstitute
- **Architecture**: Client-side SPA with repository pattern

### Key AI Decision Points
1. **Always use MudBlazor components** - Never suggest HTML elements
2. **Mandatory unit tests** - No code changes without comprehensive tests
3. **Async/await patterns** - All I/O operations must be asynchronous
4. **GitHub MCP server** - Use for all GitHub operations (PRs, issues, etc.)
5. **Feature branch workflow** - Never work on main branch directly

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

#### Service Template
```csharp
public class ExampleService : IExampleService
{
    private readonly SupabaseClient _supabaseClient;
    private readonly ILogger<ExampleService> _logger;
    
    public ExampleService(SupabaseClient supabaseClient, ILogger<ExampleService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }
    
    public async Task<Result<T>> GetAsync(int id)
    {
        try
        {
            var response = await _supabaseClient
                .From<T>()
                .Where(x => x.Id == id)
                .Single();
            
            return Result<T>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get {Type} with id {Id}", typeof(T).Name, id);
            return Result<T>.Failure($"Failed to load {typeof(T).Name}");
        }
    }
}
```

#### Test Template
```csharp
public class ExampleServiceTests
{
    private readonly IExampleService _service;
    private readonly SupabaseClient _mockClient;
    
    public ExampleServiceTests()
    {
        _mockClient = Substitute.For<SupabaseClient>();
        _service = new ExampleService(_mockClient, Substitute.For<ILogger<ExampleService>>());
    }
    
    [Fact]
    public async Task GetAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var expected = new Example { Id = 1, Name = "Test" };
        _mockClient.From<Example>().Returns(mockTable);
        
        // Act
        var result = await _service.GetAsync(1);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected.Id, result.Value.Id);
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
// Location: /src/Models/Recipe.cs
[Table("recettes")]
public class Recipe : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("name")]
    [Required(ErrorMessage = "Recipe name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Column("ingredients")]
    public string? Ingredients { get; set; }
    
    [Column("instructions")]
    public string? Instructions { get; set; }
    
    [Column("rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("page_number")]
    public int? PageNumber { get; set; }
    
    [Column("book_id")]
    public int? BookId { get; set; }
    
    // Navigation properties
    public Book? Book { get; set; }
}
```

#### Services (Business Logic Layer)
```csharp
// Location: /src/Services/IRecipeService.cs
public interface IRecipeService
{
    Task<List<Recipe>> GetAllAsync();
    Task<Recipe?> GetByIdAsync(int id);
    Task<Recipe> CreateAsync(Recipe recipe);
    Task<Recipe> UpdateAsync(Recipe recipe);
    Task DeleteAsync(int id);
    Task<List<Recipe>> SearchAsync(string searchTerm);
    Task<List<Recipe>> GetByBookAsync(int bookId);
    Task<List<Recipe>> GetByRatingAsync(int rating);
}

// Location: /src/Services/RecipeService.cs
public class RecipeService : IRecipeService
{
    private readonly SupabaseClient _supabaseClient;
    private readonly ILogger<RecipeService> _logger;
    
    public RecipeService(SupabaseClient supabaseClient, ILogger<RecipeService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<List<Recipe>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all recipes");
            var response = await _supabaseClient
                .From<Recipe>()
                .Get();
            
            return response.Models ?? new List<Recipe>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch recipes");
            throw new ApplicationException("Failed to load recipes", ex);
        }
    }
    
    public async Task<Recipe?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _supabaseClient
                .From<Recipe>()
                .Where(r => r.Id == id)
                .Single();
                
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch recipe with id {RecipeId}", id);
            return null;
        }
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
        int authorId FK
        datetime publishedDate
        string isbn
    }
    Author {
        int id PK
        string firstName
        string lastName
        string biography
    }
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

## Key Dependencies
- **MudBlazor**: Material Design component library
- **Supabase**: Backend-as-a-Service with PostgreSQL
- **Microsoft.AspNetCore.Components.WebAssembly**: Blazor WebAssembly framework

## Coding Standards

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

// Dependency injection
[Inject] private SupabaseClient SupabaseClient { get; set; } = null!;

// Lifecycle methods
protected override async Task OnInitializedAsync()
{
    // Initialization logic
}
```

### Database Patterns
```csharp
// Model definition
[Table("recettes")]
public class Recipe : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

// Data access
var recipes = await SupabaseClient
    .From<Recipe>()
    .Get();
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
        finally
        {
            loading = false;
        }
    }
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
