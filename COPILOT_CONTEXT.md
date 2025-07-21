# Project Context for AI Assistant

## Application Purpose
Mes Recettes is a personal recipe management system that allows users to:
- Store and organize their favorite recipes
- Associate recipes with cookbooks and authors
- Rate recipes for future reference
- Track page numbers for physical cookbook references
- Print recipes in a clean format

## Business Domain Knowledge

### Recipe Management Concepts
- **Recipe**: A cooking instruction with ingredients and steps
- **Rating**: 1-5 star system for recipe evaluation
- **Notes**: Personal comments or modifications to recipes
- **Book Reference**: Connection to physical or digital cookbooks
- **Page Number**: Specific location in a cookbook

### User Workflows
1. **Adding Recipes**: Users input recipe details, optionally linking to books
2. **Browsing**: Users can filter and search through their recipe collection
3. **Rating**: Users rate recipes after trying them
4. **Printing**: Users can generate print-friendly versions of recipes
5. **Organization**: Users organize recipes by book, author, or rating

### Data Relationships
```
Author (1) -> (Many) Books (1) -> (Many) Recipes
```

## UI/UX Guidelines
- Use Material Design principles via MudBlazor
- Maintain consistent spacing and typography
- Provide clear navigation between related entities
- Include confirmation dialogs for destructive actions
- Show loading states for async operations
- Display meaningful error messages to users

## Technical Constraints
- Client-side Blazor WebAssembly (no server-side rendering)
- Supabase requires internet connectivity
- Browser storage limitations for offline scenarios
- Single-page application navigation patterns

## Common User Stories
- "As a user, I want to quickly find recipes by rating"
- "As a user, I want to see all recipes from a specific cookbook"
- "As a user, I want to add my own notes to existing recipes"
- "As a user, I want to print a recipe without the website formatting"
- "As a user, I want to rate recipes after I've tried them"

## When suggesting features or fixes:
1. Consider the home cook user persona
2. Prioritize simplicity and ease of use
3. Maintain data integrity between related entities
4. Provide helpful validation messages
5. Consider both digital and physical cookbook workflows
6. Keep the interface clean and uncluttered

## Development Practices
- **Always create a new branch** for any changes or features - never modify main directly
- Use clear, descriptive branch names that indicate the purpose of the changes
- Test functionality thoroughly before suggesting merge to main branch

### Documentation Maintenance
- **Update Copilot Instruction Files**: When implementing features or modifications, update these files as necessary:
  - `/.github/copilot-instructions.md` - Detailed development guidelines and patterns
  - `/.copilot-instructions.md` - Quick reference for coding standards
  - `/COPILOT_CONTEXT.md` - Business context and user workflows (this file)
- **Update Project Documentation**: Maintain the `/docs` folder with current information about:
  - Feature specifications and user guides
  - Technical architecture and setup instructions
  - API documentation and configuration details
  - Deployment and maintenance procedures
- **Documentation Consistency**: Ensure all documentation reflects the current state of the application and development practices
