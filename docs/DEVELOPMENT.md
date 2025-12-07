# Development Guide (Updated December 2025)

Current development practices for Mes Recettes: Blazor WebAssembly + MudBlazor, Supabase backend, service/query pattern, and Result<T> for error handling.

## Development Environment
- VS Code with C# Dev Kit and GitLens
- .NET 9.0 SDK
- Git + GitHub MCP Server for PRs/issues
- Recommended setting: `dotnet.defaultSolution = "RecettesIndex.sln"`

## Coding Standards
- MudBlazor for all UI (no raw HTML)
- Async/await everywhere; pass CancellationToken
- Services return Result<T>; log failures with structured messages
- C# 12 primary constructors + file-scoped namespaces
- Nullable enabled; guard against nulls
- Constants in dedicated classes (CacheConstants, PaginationConstants, etc.)
- Use ICacheService for frequently accessed data

## Project Patterns
- Recipes page is modular: RecipeQuickFilters, RecipeAdvancedFilters, RecipeActiveFilters, RecipeLoadingState, RecipeGridView, RecipeTableView, EmptyState
- Service/query pattern for data access; Supabase client wrapped in query classes
- LocalStorage for view/favorite persistence
- Feature branch workflow only; never commit to main directly

## Development Workflow
1) Create feature branch (`docs/cleanup`, `feature/…`, `fix/…`)
2) Implement with MudBlazor components and async services
3) Update/ add tests alongside code changes
4) Run tests before PR
5) Use GitHub MCP Server for PR creation/review

## Testing Guidelines
- Frameworks: xUnit, bUnit (components), NSubstitute (mocks)
- Follow Arrange/Act/Assert; use [Theory]/[InlineData] for matrix cases
- Cover validation, error paths, and caching behaviors
- Run `dotnet test` before pushing

## Troubleshooting
- Verify CancellationToken flows for all async methods
- Ensure Result<T> is used for service responses; return friendly messages
- Check caching keys/invalidations when data changes
- Confirm UI uses MudBlazor components and consistent spacing/typography
- If tests fail, start with service mocks and component parameter binding

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() => authService.SignInAsync("test", "test"));
}
```

### Test Examples

```csharp
// ✅ Good: Comprehensive validation testing
[Theory]
[InlineData(1, true)]
[InlineData(3, true)]
[InlineData(5, true)]
[InlineData(0, false)]
[InlineData(6, false)]
[InlineData(-1, false)]
public void Rating_ShouldValidateRange_ForAllValues(int rating, bool isValid)
{
    // Arrange
    var recipe = new Recipe { Rating = rating };
    var context = new ValidationContext(recipe);
    var results = new List<ValidationResult>();

    // Act
    var actualIsValid = Validator.TryValidateObject(recipe, context, results, true);

    // Assert
    Assert.Equal(isValid, actualIsValid);
    if (!isValid)
    {
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Rating must be between 1 and 5"));
    }
}

// ✅ Good: Model relationship testing
[Fact]
public void Author_Books_ShouldBeInitialized()
{
    // Arrange & Act
    var author = new Author();

    // Assert
    Assert.NotNull(author.Books);
    Assert.Empty(author.Books);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=RecipeModelTests"

# Run tests for specific namespace
dotnet test --filter "FullyQualifiedName~RecettesIndex.Tests"

# Generate code coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD Testing

Our GitHub Actions workflow automatically:
1. Runs all unit tests before deployment
2. Prevents merging if tests fail
3. Ensures code quality through automated validation

```yaml
# Testing job in GitHub Actions
test_job:
  runs-on: ubuntu-latest
  name: Test
  steps:
    - name: Checkout
      uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
```

### Dependencies for Testing Complex Types

When testing services that depend on complex external libraries (like Supabase.Client), consider:
- Creating wrapper interfaces for better testability
- Testing integration points separately from unit tests  
- Using partial mocks or test doubles for complex scenarios
- Some complex types may require constructor parameters that are difficult to mock

### Test Coverage Requirements

- **Model Validation**: Test all DataAnnotation validation rules
- **Business Logic**: Test all service methods and their edge cases
- **Error Scenarios**: Test exception handling and error conditions
- **Relationships**: Test entity relationships and navigation properties
- **Authentication**: Test authentication workflows and security

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=RecipeModelTests"

# Run tests for specific namespace
dotnet test --filter "FullyQualifiedName~RecettesIndex.Tests"

# Generate code coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```
        _mockSupabaseClient.Setup(x => x.From<Recipe>().Get())
            .ReturnsAsync(new ModelResponse<Recipe> { Models = expectedRecipes });
        
        // Act
        var result = await _recipeService.GetRecipesAsync();
        
        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test", result[0].Name);
    }
}
```

### Component Testing with bUnit

```csharp
[TestClass]
public class RecipeCardTests : TestContext
{
    [TestMethod]
    public void RecipeCard_ShouldDisplayName_WhenRecipeProvided()
    {
        // Arrange
        var recipe = new Recipe { Name = "Chocolate Cake", Rating = 5 };
        
        // Act
        var component = RenderComponent<RecipeCard>(parameters => parameters
            .Add(p => p.Recipe, recipe));
        
        // Assert
        Assert.IsTrue(component.Markup.Contains("Chocolate Cake"));
        Assert.IsTrue(component.Markup.Contains("★★★★★"));
    }
}
```

### Integration Testing

```csharp
[TestMethod]
public async Task AddRecipe_ShouldPersistToDatabase()
{
    // Test with actual Supabase test database
    var recipe = new Recipe 
    { 
        Name = "Integration Test Recipe",
        Rating = 4,
        Notes = "Test notes"
    };
    
    var result = await _recipeService.AddRecipeAsync(recipe);
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Id > 0);
    
    // Cleanup
    await _recipeService.DeleteRecipeAsync(result.Id);
}
```

## 🐛 Troubleshooting

### Common Issues

#### Build Errors

```bash
# Clear build cache
dotnet clean
rm -rf bin/ obj/
dotnet restore
dotnet build
```

#### Port Conflicts

```bash
# Use specific port
dotnet run --urls "http://localhost:5030"

# Kill existing processes
taskkill /f /im dotnet.exe  # Windows
pkill -f dotnet             # macOS/Linux
```

#### Supabase Connection Issues

1. **Check configuration**: Verify URL and API key in `appsettings.json`
2. **Network connectivity**: Ensure internet connection
3. **API limits**: Check Supabase dashboard for usage limits
4. **CORS settings**: Verify domain is allowed in Supabase

#### Hot Reload Not Working

```bash
# Restart with clean build
dotnet clean
dotnet run --no-hot-reload
```

### Debugging Tips

1. **Use browser dev tools** for client-side debugging
2. **Check console logs** for JavaScript errors
3. **Use Blazor Server debugging** for complex issues
4. **Enable detailed logging** in `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "YourNamespace": "Debug"
    }
  }
}
```

## 📊 Performance Guidelines

### Best Practices

- **Lazy loading**: Use `@page` directive with lazy loading
- **Virtualization**: Use `MudVirtualize` for large data sets
- **Debouncing**: Implement debounced search inputs
- **Caching**: Cache frequently accessed data
- **Minimal API calls**: Batch operations when possible

### Memory Management

```csharp
// ✅ Good: Dispose resources
public void Dispose()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
}

// ✅ Good: Use using statements
using var httpClient = new HttpClient();
var response = await httpClient.GetAsync(url);
```

---

For more information, see:
- [Main Documentation](README.md)
- [API Reference](API.md)
- [Architecture Guide](ARCHITECTURE.md)
