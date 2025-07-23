# GitHub Copilot Instructions for Mes Recettes

## Project Context
Mes Recettes is a personal recipe management application built with Blazor WebAssembly. Users can organize their recipes, associate them with cookbooks and authors, and rate their favorites.

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
- Write unit tests for business logic
- Test Blazor components with bUnit
- Mock Supabase client for testing
- Test error scenarios

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
- **Confirm changes with user before creating commits or PRs** - show validation results and get approval
- Make atomic commits with clear messages
- Test changes before suggesting merge to main
- **Use GitHub MCP server** for all GitHub operations (creating PRs, issues, reviews, etc.)
- Follow existing pull request patterns if established

### GitHub Operations
- **Always use GitHub MCP server tools** for:
  - Creating pull requests (`mcp_github_create_pull_request`)
  - Managing issues (`mcp_github_create_issue`, `mcp_github_update_issue`)
  - Adding comments (`mcp_github_add_issue_comment`)
  - Repository management and operations
- **Never use git CLI commands** for GitHub-specific operations when MCP server is available
- **Example**: Instead of `gh pr create`, use `mcp_github_create_pull_request`

### Documentation Requirements
- **Update Copilot Instructions**: When creating a new branch for any feature or modification, update these instruction files as necessary:
  - `/.github/copilot-instructions.md` - Comprehensive development guidelines and patterns (this file)
  - `/.copilot-instructions.md` - Quick reference for common coding patterns
  - `/COPILOT_CONTEXT.md` - Business domain context and user workflows
- **Update Project Documentation**: For every modification, ensure documentation in the `/docs` folder is updated to reflect:
  - New features and functionality
  - API changes or new endpoints
  - Configuration changes
  - Setup or deployment procedures
  - User-facing changes or new workflows
- **Documentation Consistency**: Keep all documentation synchronized with code changes to maintain accuracy and usefulness for future development
