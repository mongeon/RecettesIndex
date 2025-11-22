# Component Architecture Diagram

## Before Refactoring
```
┌─────────────────────────────────────────────────────────────┐
│                      Recipes.razor                          │
│                      (800+ lines)                           │
│                                                             │
│  • View toggle logic                                        │
│  • Quick filters UI + logic                                 │
│  • Advanced filters UI + logic                              │
│  • Active filters display                                   │
│  • Loading skeletons                                        │
│  • Empty state                                              │
│  • Card grid rendering                                      │
│  • Table rendering                                          │
│  • Pagination                                               │
│  • Filter state management                                  │
│  • Recipe CRUD operations                                   │
│  • Data loading                                             │
│  • Event handlers                                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
        ↓
   Monolithic, hard to maintain
```

## After Refactoring
```
┌───────────────────────────────────────────────────────────────────┐
│                       Recipes.razor                               │
│                       (550 lines)                                 │
│                                                                   │
│  Responsibilities:                                                │
│  • State management                                               │
│  • Data loading orchestration                                     │
│  • Business logic                                                 │
│  • Child component coordination                                   │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ RecipeQuick      │  │ RecipeAdvanced   │  │ RecipeActive     │
│ Filters          │  │ Filters          │  │ Filters          │
│ (~70 lines)      │  │ (~90 lines)      │  │ (~60 lines)      │
│                  │  │                  │  │                  │
│ • Quick chips    │  │ • Search input   │  │ • Active chips   │
│ • Filter counts  │  │ • Dropdowns      │  │ • Remove buttons │
│ • Click handlers │  │ • Clear button   │  │                  │
└──────────────────┘  └──────────────────┘  └──────────────────┘

                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ RecipeLoading    │  │ EmptyState       │  │ RecipeGridView   │
│ State            │  │ (reused)         │  │ OR               │
│ (~25 lines)      │  │                  │  │ RecipeTableView  │
│                  │  │ • Icon           │  │ (~50/80 lines)   │
│ • Card skeletons │  │ • Message        │  │                  │
│ • Table skeleton │  │ • Action button  │  │ • Card grid      │
│                  │  │                  │  │ • Pagination     │
│                  │  │                  │  │ • Table + sort   │
└──────────────────┘  └──────────────────┘  └──────────────────┘
```

## Data Flow Diagram
```
┌────────────────────────────────────────────────────────────────┐
│                        User Actions                            │
└────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────────┐
│                     Recipes.razor (Parent)                     │
│                                                                │
│  ┌──────────────────┐     ┌──────────────────┐               │
│  │ State Management │────▶│ Event Handlers   │               │
│  └──────────────────┘     └──────────────────┘               │
│           │                         │                         │
│           │                         ▼                         │
│           │              ┌──────────────────┐                 │
│           │              │ IRecipeService   │                 │
│           │              └──────────────────┘                 │
│           │                         │                         │
│           ▼                         ▼                         │
│  ┌──────────────────┐     ┌──────────────────┐               │
│  │ Parameters       │────▶│ Child Components │               │
│  └──────────────────┘     └──────────────────┘               │
└────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────────┐
│                    Child Components                            │
│                                                                │
│  • Receive parameters (read-only)                             │
│  • Render UI based on parameters                              │
│  • Emit events via EventCallback                              │
│  • No direct state mutation                                   │
└────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────────┐
│                         UI Update                              │
└────────────────────────────────────────────────────────────────┘

Unidirectional Data Flow:
  User Action → Parent State → Parameters → Child Render → Events → Parent Handler
```

## Component Interaction Example
```
Example: User clicks "5 Étoiles" filter chip

1. RecipeQuickFilters
   └─→ OnApplyQuickFilter.InvokeAsync(5)

2. Recipes.razor
   ├─→ ApplyQuickFilter(5) handler
   ├─→ Update state: quickFilterRating = 5
   ├─→ Call LoadCardViewData() or Reload()
   └─→ Re-render with new data

3. RecipeGridView or RecipeTableView
   └─→ Receive updated Recipes parameter
   └─→ Render filtered recipes

4. RecipeQuickFilters
   └─→ Receive updated QuickFilterRating parameter
   └─→ Update chip highlighting
```

## Reusability Map
```
┌─────────────────────────────────────────────────────────────────┐
│              Reusable Across Application                        │
└─────────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┬────────────────┐
        │                   │                   │                │
        ▼                   ▼                   ▼                ▼
┌──────────────┐   ┌──────────────┐   ┌──────────────┐  ┌──────────────┐
│ RecipeCard   │   │ EmptyState   │   │ PizzaRating  │  │ RecipeCard   │
│ (existing)   │   │ (existing)   │   │ (existing)   │  │ Skeleton     │
│              │   │              │   │              │  │ (existing)   │
│ Used in:     │   │ Used in:     │   │ Used in:     │  │ Used in:     │
│ • Home       │   │ • Recipes    │   │ • Recipes    │  │ • Home       │
│ • Recipes    │   │ • Books      │   │ • RecipeCard │  │ • Recipes    │
│ • Details    │   │ • Authors    │   │ • Details    │  │              │
└──────────────┘   └──────────────┘   └──────────────┘  └──────────────┘

┌─────────────────────────────────────────────────────────────────┐
│           New Components (Potentially Reusable)                 │
└─────────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────┐   ┌──────────────┐   ┌──────────────┐
│ QuickFilters │   │ Advanced     │   │ ActiveFilters│
│              │   │ Filters      │   │              │
│ Could adapt  │   │ Could adapt  │   │ Could adapt  │
│ for:         │   │ for:         │   │ for:         │
│ • Books page │   │ • Books page │   │ • Books page │
│ • Authors    │   │ • Authors    │   │ • Authors    │
└──────────────┘   └──────────────┘   └──────────────┘
```

## Performance Characteristics
```
┌─────────────────────────────────────────────────────────────────┐
│                    Component Rendering                          │
└─────────────────────────────────────────────────────────────────┘

Scenario: User changes search term

Before Refactoring:
┌────────────────────────────────┐
│  Entire Recipes.razor          │  ◄── Full page re-render
│  (800+ lines)                  │      Everything updates
└────────────────────────────────┘

After Refactoring:
┌────────────────────────────────┐
│  Recipes.razor (Parent)        │  ◄── State change
└────────────────────────────────┘
           │
           ├─→ RecipeQuickFilters      (no change, skipped)
           ├─→ RecipeAdvancedFilters   (search term updated) ✓
           ├─→ RecipeActiveFilters     (filter added) ✓
           └─→ RecipeGridView          (new data) ✓
           
Result: Only 3 components re-render instead of entire page
```

## Code Organization Benefits
```
Before:
└── src/Pages/Recipes.razor
    └── 800+ lines of mixed concerns

After:
├── src/Pages/Recipes.razor (550 lines)
│   ├── #region Initialization Methods
│   ├── #region Data Loading Methods
│   ├── #region Filter Methods
│   ├── #region Quick Filter Event Handlers
│   ├── #region Clear Individual Filter Methods
│   ├── #region Filter Changed Event Handlers
│   ├── #region Recipe Action Methods
│   └── #region Helper Methods
│
└── src/Components/
    ├── RecipeQuickFilters.razor (~70 lines)
    ├── RecipeAdvancedFilters.razor (~90 lines)
    ├── RecipeActiveFilters.razor (~60 lines)
    ├── RecipeLoadingState.razor (~25 lines)
    ├── RecipeGridView.razor (~50 lines)
    └── RecipeTableView.razor (~80 lines)

Benefits:
✓ Each file has single responsibility
✓ Easy to locate specific functionality
✓ Reduced cognitive load
✓ Better git diffs
✓ Easier code reviews
```

## Testing Strategy
```
┌─────────────────────────────────────────────────────────────────┐
│                     Unit Testing Layers                         │
└─────────────────────────────────────────────────────────────────┘

Level 1: Component Tests (using bUnit)
├── RecipeQuickFiltersTests
│   ├── Chip rendering with correct counts
│   ├── Click handlers invoke callbacks
│   └── Highlighting based on active filter
│
├── RecipeAdvancedFiltersTests
│   ├── Search input debounce
│   ├── Dropdown options with counts
│   └── Clear all functionality
│
├── RecipeGridViewTests
│   ├── Card grid layout
│   ├── Pagination controls
│   └── Recipe action callbacks
│
└── RecipeTableViewTests
    ├── Table rendering
    ├── Sorting functionality
    └── Row color styling

Level 2: Integration Tests
└── RecipesPageTests
    ├── Filter combinations
    ├── View mode switching
    └── Data loading scenarios

Level 3: Service Tests (existing)
└── RecipeServiceTests
    ├── Search functionality
    ├── CRUD operations
    └── Cache behavior
```

## Maintenance Workflows
```
Adding a new filter:

Before Refactoring:
1. Find filter UI section in 800-line file  ❌ Hard to locate
2. Add filter chip                           ❌ Mixed with other code
3. Add filter state                          ❌ Lost in many fields
4. Add filter logic                          ❌ Buried in methods
5. Update clear filters                      ❌ Multiple places
6. Test entire page                          ❌ Hard to isolate

After Refactoring:
1. Add chip to RecipeQuickFilters            ✓ Clear location
2. Add parameter to component                ✓ Type-safe
3. Add state field in parent                 ✓ Organized in region
4. Add handler in parent                     ✓ In dedicated region
5. Component updates automatically           ✓ One place
6. Test component in isolation               ✓ Fast & focused
```
