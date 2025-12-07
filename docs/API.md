# API Reference

This document provides detailed information about the data models, services, and API patterns used in Mes Recettes.

## 📋 Table of Contents

- [Data Models](#data-models)
- [Validation Rules](#validation-rules)
- [Services](#services)
- [Supabase Integration](#supabase-integration)
- [API Patterns](#api-patterns)
- [Error Handling](#error-handling)
- [Service Reuse & Patterns](#service-reuse--patterns)

## 🏗️ Data Models

### Recipe Model

```csharp
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
    
    [Column("created_at")]
    public DateTime CreationDate { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("book_id")]
    public int? BookId { get; set; }
    
    [Column("page")]
    public int? BookPage { get; set; }

    [Column("store_id")]
    public int? StoreId { get; set; }
    
    [Column("url")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? Url { get; set; }

**Validation Rules:**
- `Name`: Required (cannot be empty)
- `Rating`: Required, must be between 1 and 5 (inclusive)
- `BookPage`: Optional, must be positive if provided
- `Url`: Optional, must be a valid URL format if provided
- `BookId`: Optional foreign key (reference to Book)
- `StoreId`: Optional foreign key (reference to Store)

### Book Model

```csharp
[Table("books")]
public class Book : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("title")]
    public string Name { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    [Reference(typeof(Author), includeInQuery: true, useInnerJoin: false)]
    public List<Author> Authors { get; set; } = new();
}
```

**Validation Rules:**
- `Title`: Required (stored as `title` column)
- `Authors`: Many-to-many via `books_authors`

### Store Model

```csharp
[Table("stores")]
public class Store : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required(ErrorMessage = "The store name is required.")]
    [MaxLength(255, ErrorMessage = "Store name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Column("phone")]
    [MaxLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [Column("website")]
    [MaxLength(500, ErrorMessage = "Website URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? Website { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreationDate { get; set; }
}
```

**Validation Rules:**
- `Name`: Required, max 255 chars
- `Address`: Optional, max 500 chars
- `Phone`: Optional, max 50 chars
- `Website`: Optional, max 500 chars, must be valid URL
- `Notes`: Optional

### Author Model

```csharp
[Table("authors")]
public class Author : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("first_name")]
    public string Name { get; set; } = string.Empty;

    [Column("last_name")]
    public string? LastName { get; set; }
    
    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    [Reference(typeof(Book), useInnerJoin: false, includeInQuery: true)]
    public List<Book> Books { get; set; } = [];

    [JsonIgnore]
    public string FullName => string.IsNullOrWhiteSpace(LastName)
        ? Name
        : $"{Name} {LastName}".Trim();
}
```

**Validation Rules:**
- `FirstName`: Required
- `LastName`: Optional

### Entity Relationships

```mermaid
erDiagram
    Author ||--o{ BookAuthor : "has"
    Book ||--o{ BookAuthor : "has"
    BookAuthor }o--|| Author : "references"
    BookAuthor }o--|| Book : "references"
    Book ||--o{ Recipe : "contains"
    Store ||--o{ Recipe : "sells"
    
    Author {
        int id PK
        string first_name
        string last_name
        datetime created_at
    }
    
    Book {
        int id PK
        string title
        datetime created_at
    }

    BookAuthor {
        int book_id FK
        int author_id FK
        datetime created_at
    }
    
    Store {
        int id PK
        string name
        string address
        string phone
        string website
        string notes
        datetime created_at
    }
    
    Recipe {
        int id PK
        string name
        text notes
        int rating "1-5 stars with validation"
        int book_id FK
        int store_id FK
        int page
        string url "optional website URL"
        datetime created_at
    }
```

## ✅ Validation Rules

### Data Validation Overview

The application uses comprehensive validation through System.ComponentModel.DataAnnotations and custom business rules.

#### Recipe Validation

| Property | Validation Rule | Error Message |
|----------|----------------|---------------|
| `Name` | Required | "The Name field is required." |
| `Rating` | Range(1, 5) | "Rating must be between 1 and 5" |
| `PageNumber` | Optional, validated positive when provided | "Book page number must be positive" |
| `Url` | Url format | "Please enter a valid URL" |
| `StoreId`/`BookId` | Optional foreign keys | N/A |

#### Author Validation

| Property | Validation Rule | Error Message |
|----------|----------------|---------------|
| `FirstName` | Required | "The Name field is required." |
| `LastName` | Optional | N/A |

#### Book Validation

| Property | Validation Rule | Error Message |
|----------|----------------|---------------|
| `Title` | Required | "Book title is required" |
| `Authors` | Optional collection | N/A |

#### Store Validation

| Property | Validation Rule | Error Message |
|----------|----------------|---------------|
| `Name` | Required, MaxLength(255) | "The store name is required." / "Store name cannot exceed 255 characters" |
| `Address` | MaxLength(500) | "Address cannot exceed 500 characters" |
| `Phone` | MaxLength(50) | "Phone number cannot exceed 50 characters" |
| `Website` | MaxLength(500), Url | "Please enter a valid URL" / "Website URL cannot exceed 500 characters" |
| `Notes` | Optional | N/A |

### Validation Testing

Our comprehensive test suite includes **533 tests** covering all validation scenarios, services, components, and integration testing (measured via `dotnet test --list-tests | Measure-Object`):

```csharp
// Example: Rating validation test
[Theory]
[InlineData(1, true)]   // Valid: minimum
[InlineData(3, true)]   // Valid: middle  
[InlineData(5, true)]   // Valid: maximum
[InlineData(0, false)]  // Invalid: below range
[InlineData(6, false)]  // Invalid: above range
[InlineData(-1, false)] // Invalid: negative
public void Rating_ShouldValidateRange_ForAllValues(int rating, bool isValid)
{
    // Validation testing implementation
}
```

## 🛠️ Services

The service layer follows a query/service pattern and now relies on shared helpers to reduce duplication and standardize behavior:

- `CrudServiceBase<TModel, TService>` centralizes common CRUD flows with consistent logging and error mapping
- `ValidationGuards` provides reusable validation helpers used by all services
- `CacheServiceExtensions` adds `GetOrEmptyAsync` for resilient list caching and `RemoveMany` for cache invalidation

Currently, `AuthorService`, `BookService`, and `StoreService` derive from the CRUD base and use these helpers; `RecipeService` uses caching helpers for related cache invalidation.

### Recipe Service (excerpt)

```csharp
public interface IRecipeService
{
    Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(...);
    Task<Result<Recipe>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Recipe>> CreateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<Recipe>> UpdateAsync(Recipe recipe, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct = default);
}
```

### Author, Book, Store Services

These services implement their respective interfaces and derive from the CRUD base to:
- Validate inputs using `ValidationGuards`
- Use Supabase for data persistence
- Invalidate relevant caches on mutations
- Keep domain-specific logic (e.g., book-author associations)

## 🔌 Supabase Integration

### Configuration

```csharp
public class SupabaseConfig
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

// In Program.cs
var supabaseConfig = builder.Configuration.GetSection("Supabase").Get<SupabaseConfig>();
builder.Services.AddScoped(_ => new SupabaseClient(supabaseConfig.Url, supabaseConfig.Key));
```

### Query Patterns

```csharp
// Get all records
var recipes = await supabaseClient.From<Recipe>().Get();

// Get by ID
var recipe = await supabaseClient.From<Recipe>().Where(x => x.Id == id).Single();

// Update
var updated = await supabaseClient.From<Recipe>().Update(recipe);

// Delete
await supabaseClient.From<Recipe>().Where(x => x.Id == id).Delete();
```

## 📝 API Patterns

The application uses a `Result<T>` pattern for service results, combined with structured logging and consistent error messages.

## ⚠️ Error Handling

Common exception handling patterns are centralized via the base class helpers where possible. Network errors surface a standard message; unexpected errors map to a user-friendly message with logged details.

---

For more information, see:
- [Main Documentation](README.md)
- [Development Guide](DEVELOPMENT.md)
- [Architecture Guide](ARCHITECTURE.md)
