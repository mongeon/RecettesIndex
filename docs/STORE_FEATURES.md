# Store & Restaurant Integration

## Overview

The Store feature allows users to track recipes from stores, restaurants, prepared meal vendors, and other food establishments. This complements the existing Book feature, providing a complete picture of recipe sources.

## Features

### Store Management

- **Create/Edit/Delete Stores**: Full CRUD operations for store management
- **Store Information**: Name, address, phone, website, and custom notes
- **Recipe Association**: Link recipes to specific stores or restaurants
- **Store Details Page**: View all recipes from a specific store
- **Dashboard Integration**: Statistics showing most popular stores

### Recipe Source Tracking

Recipes can now come from three sources:

1. **Books** 📖 - Traditional cookbooks with authors and page numbers
2. **Stores** 🏪 - Prepared meals, restaurant recipes, store-bought items
3. **Homemade** 🏠 - Personal recipes without external sources

## User Interface

### Visual Badges

Recipes display color-coded badges indicating their source:

- **Blue Badge (Primary)**: Recipe from a cookbook
- **Orange Badge (Secondary)**: Recipe from a store/restaurant
- **Tertiary Badge**: Homemade recipe

Example on Recipe Card:
```
┌─────────────────────────┐
│  Poulet Rôti           │
│  ⭐⭐⭐⭐⭐              │
│  🏪 Le Gourmet Express │  ← Store badge
│  Great for weeknights  │
└─────────────────────────┘
```

### Stores Page (`/stores`)

- Grid view of all stores
- Add/Edit/Delete operations
- Quick stats showing recipe count per store
- Search and filter capabilities
- Click to view store details

### Store Details Page (`/stores/{id}`)

Similar to BookDetails page, showing:

- Store name and information
- Contact details (address, phone, website)
- List of all recipes from this store
- Store notes
- Edit functionality

## Database Schema

### stores Table

```sql
CREATE TABLE stores (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT,
    phone VARCHAR(50),
    website TEXT,
    notes TEXT,
    creation_date TIMESTAMP DEFAULT NOW()
);
```

### Recipe Association

Recipes link to stores via `store_id`:

```sql
ALTER TABLE recettes
ADD COLUMN store_id INTEGER REFERENCES stores(id);
```

## Service Layer

### IStoreService Interface

```csharp
public interface IStoreService
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default);
    Task<Result<Store>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Store>> CreateAsync(Store store, CancellationToken ct = default);
    Task<Result<Store>> UpdateAsync(Store store, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default);
}
```

### Caching

Store data is cached using `ICacheService` with:
- Cache key: `CacheConstants.StoresListKey`
- TTL: `CacheConstants.DefaultTtl` (5 minutes)
- Invalidation on Create/Update/Delete operations

## Recipe Service Updates

### SearchAsync Method

Enhanced to support store filtering:

```csharp
Task<Result<(IReadOnlyList<Recipe> Items, int Total)>> SearchAsync(
    string? term, 
    int? rating, 
    int? bookId, 
    int? storeId,  // ← New parameter
    int? authorId, 
    int page, 
    int pageSize, 
    string? sortLabel = null, 
    bool sortDescending = false, 
    CancellationToken ct = default);
```

### Helper Methods

```csharp
Task<IReadOnlyList<Store>> GetStoresAsync(CancellationToken ct = default);
```

Returns cached list of all stores for filter dropdowns.

## UI Components

### Store Filter

Added to Recipes page advanced filters:

```razor
<MudSelect T="string" 
           Value="@StoreFilter" 
           ValueChanged="OnStoreChanged" 
           Label="Magasin/Restaurant">
    <MudSelectItem Value="all">Tous</MudSelectItem>
    @foreach (var store in Stores)
    {
        <MudSelectItem Value="@store.Id.ToString()">
            @store.Name (@GetRecipeCount(store.Id))
        </MudSelectItem>
    }
</MudSelect>
```

### Recipe Card Updates

RecipeCard component now displays appropriate source badge:

```csharp
@if (Recipe.IsFromBook && Book != null)
{
    <MudChip Icon="@Icons.Material.Filled.MenuBook" Color="Color.Primary">
        @Book.Name
    </MudChip>
}
else if (Recipe.IsFromStore && Recipe.Store != null)
{
    <MudChip Icon="@Icons.Material.Filled.Storefront" Color="Color.Secondary">
        @Recipe.Store.Name
    </MudChip>
}
else
{
    <MudChip Icon="@Icons.Material.Filled.Home" Color="Color.Tertiary">
        Maison
    </MudChip>
}
```

## Recipe Details Integration

### Source Information Display

RecipeDetails page shows store information when applicable:

```razor
@if (store != null)
{
    <MudListItem Icon="@Icons.Material.Filled.Storefront">
        <MudText>Magasin: <MudLink Href="/stores/@store.Id">@store.Name</MudLink></MudText>
    </MudListItem>
    
    @if (!string.IsNullOrWhiteSpace(store.Address))
    {
        <MudListItem Icon="@Icons.Material.Filled.LocationOn">
            <MudText>Adresse: @store.Address</MudText>
        </MudListItem>
    }
    
    @if (!string.IsNullOrWhiteSpace(store.Phone))
    {
        <MudListItem Icon="@Icons.Material.Filled.Phone">
            <MudText>Téléphone: <MudLink Href="tel:@store.Phone">@store.Phone</MudLink></MudText>
        </MudListItem>
    }
}
```

### Similar Recipes

The similarity algorithm now considers store matching:

```csharp
// Same store (highest priority) - 100 points
if (candidate.StoreId.HasValue && candidate.StoreId == recipe.StoreId)
{
    score += 100;
}
```

## Print Functionality

### Print Options

Added store information toggle:

```razor
<MudCheckBox @bind-Value="showStoreInfo" 
            Label="Afficher le magasin" 
            Disabled="@(store == null)" />
```

### Printed Output

When enabled, store details appear in the print view:

```
╔════════════════════════════╗
║        Recette Name        ║
╠════════════════════════════╣
║ Magasin                    ║
║ Nom: Le Gourmet Express    ║
║ Adresse: 123 Rue Example   ║
║ Téléphone: 514-555-1234    ║
╚════════════════════════════╝
```

## Dashboard Integration

### Statistics Card

Added "Magasins/Restaurants" stat card showing total stores.

### Most Used Stores Section

Displays top stores by recipe count:

```
╔═══════════════════════════╗
║ Magasins Populaires       ║
╠═══════════════════════════╣
║ 🏪 Le Gourmet Express  15 ║
║ 🏪 Restaurant Italia    8 ║
║ 🏪 Chez Marie           5 ║
╚═══════════════════════════╝
```

### Quick Actions

Added "Gérer les magasins" button linking to `/stores` page.

## Data Migration

### Removed RecipeType Field

The `recipe_type` field has been removed as redundant:

- Source information (book_id/store_id) already indicates recipe origin
- No need for separate type classification
- Cleaner data model

Migration script: `database/migrations/remove_recipe_type.sql`

## Testing

### Model Tests

- Store model validation
- Store property constraints
- Store-Recipe associations

### Service Tests

- StoreService CRUD operations
- Store caching behavior
- Store query integration
- Error handling

### Integration Tests

- Store-Recipe relationships
- Multi-source recipe scenarios
- Store filtering and search

## Best Practices

### When to Use Stores vs Books

**Use Books for:**
- Traditional cookbooks
- Recipe collections with authors
- Recipes with page references
- Chef-authored content

**Use Stores for:**
- Prepared meals from grocery stores
- Restaurant take-out recipes
- Meal kit services
- Food vendor products

**Use Neither (Homemade) for:**
- Personal family recipes
- Original creations
- Modified recipes without attribution

### Store Data Guidelines

- **Name**: Clear, recognizable store/restaurant name
- **Address**: Full address for reference (optional)
- **Phone**: Include area code for international support
- **Website**: Full URL with https:// prefix
- **Notes**: Opening hours, specialties, personal notes

## Future Enhancements

Potential additions:

- [ ] Store categories (Restaurant, Grocery, Meal Kit, etc.)
- [ ] Store ratings (separate from recipe ratings)
- [ ] Favorite stores feature
- [ ] Store location mapping integration
- [ ] Store hours tracking
- [ ] Price tracking for store recipes
- [ ] Multi-location support for chain stores

## API Reference

See [API.md](API.md) for complete Store model and service documentation.

## Related Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) - System design including store integration
- [DEVELOPMENT.md](DEVELOPMENT.md) - Development guidelines for store features
- [API.md](API.md) - Complete API reference for Store services
