# Mes Recettes - Documentation

Mes Recettes is a personal recipe management application built with Blazor WebAssembly, designed to help home cooks organize their favorite recipes, cookbooks, and authors in a digital format.

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Features](#features)
- [Getting Started](#getting-started)
- [Development Guide](#development-guide)
- [API Reference](#api-reference)
- [Deployment](#deployment)
- [Contributing](#contributing)

## 🎯 Project Overview

Mes Recettes is a modern web application that bridges the gap between traditional cookbook collections and digital recipe management. Users can:

- Store and organize their favorite recipes
- Associate recipes with physical or digital cookbooks
- Rate recipes for future reference
- Track page numbers for cookbook references
- Generate print-friendly recipe formats
- Browse recipes by author, book, or rating

### Tech Stack

```mermaid
graph TB
    A[Frontend - Blazor WebAssembly] --> B[UI - MudBlazor Components]
    A --> C[Backend - Supabase]
    C --> D[Database - PostgreSQL]
    C --> E[Authentication - Supabase Auth]
    A --> F[Hosting - Azure Static Web Apps]
    
    subgraph "Development Tools"
        G[VS Code + C# Dev Kit]
        H[GitHub Actions CI/CD]
        I[GitHub MCP Server]
        J[xUnit Testing Framework]
        K[243 Unit Tests]
    end
    
    subgraph "Quality Assurance"
        L[Automated Testing Pipeline]
        M[Code Validation]
        N[Rating Validation 1-5]
    end
```

## 🏗️ Architecture

### System Architecture

```mermaid
graph LR
    A[User Browser] --> B[Blazor WebAssembly Client]
    B --> C[Supabase API]
    C --> D[PostgreSQL Database]
    
    subgraph "Client-Side Components"
        B1[Pages/Components]
        B2[Services Layer]
        B3[Models]
        B --> B1
        B --> B2
        B --> B3
    end
    
    subgraph "Database Schema"
        D1[recettes table]
        D2[books table]
        D3[authors table]
        D --> D1
        D --> D2
        D --> D3
    end
```

### Project Structure

```
RecettesIndex/
├── 📁 .github/
│   └── 📁 workflows/          # GitHub Actions CI/CD pipelines
│       └── azure-static-web-apps-*.yml # Automated testing & deployment
├── 📁 src/                    # Main application source
│   ├── 📁 Configuration/      # App configuration files
│   │   └── SupabaseConfig.cs
│   ├── 📁 Layout/             # Application layout components
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── 📁 Models/             # Data models with validation
│   │   └── Recette.cs         # Recipe, Book, Author models
│   ├── 📁 Pages/              # Blazor pages and dialogs
│   │   ├── Home.razor
│   │   ├── Recipes.razor
│   │   ├── Books.razor
│   │   ├── Authors.razor
│   │   └── *Dialog.razor
│   ├── 📁 Services/           # Business logic and data access
│   │   ├── AuthService.cs
│   │   └── BookAuthorService.cs
│   ├── 📁 wwwroot/            # Static assets
│   │   ├── index.html
│   │   ├── css/
│   │   └── icons/
│   ├── 📄 Program.cs          # Application entry point
│   └── 📄 _Imports.razor      # Global using statements
├── 📁 tests/                  # Comprehensive unit test suite
│   ├── 📄 RecipeModelTests.cs           # Recipe model validation tests
│   ├── 📄 AuthorModelTests.cs           # Author model and FullName tests
│   ├── 📄 BookModelTests.cs             # Book model functionality tests
│   ├── 📄 BookAuthorModelTests.cs       # Junction table relationship tests
│   ├── 📄 RecipeValidationTests.cs      # DataAnnotation validation tests
│   ├── 📄 RecipeRatingValidationTests.cs # Rating constraint tests (1-5)
│   ├── 📄 ModelRelationshipTests.cs     # Cross-model relationship tests
│   └── 📄 RecettesIndex.Tests.csproj    # Test project configuration
├── 📁 docs/                   # Project documentation
│   ├── README.md              # Complete project overview
│   ├── API.md                 # Data models and API reference
│   ├── ARCHITECTURE.md        # System design and decisions
│   ├── DEVELOPMENT.md         # Development guidelines and setup
│   └── DEPLOYMENT.md          # Deployment and hosting guide
├── 📄 RecettesAI.sln          # Solution file
└── 📄 README.md               # Quick start guide
```

## ✨ Features

### Core Functionality

#### Recipe Management
- **CRUD Operations**: Create, read, update, and delete recipes with comprehensive validation
- **Rich Text Support**: Store ingredients, instructions, and personal notes
- **Rating System**: 1-5 star rating system with enforced validation constraints
- **Data Validation**: Business rules validation with user-friendly error messages
- **Search & Filter**: Find recipes by name, rating, or cookbook

#### Cookbook Integration
- **Book Association**: Link recipes to physical or digital cookbooks
- **Page Tracking**: Store page numbers for easy reference
- **Author Management**: Organize books by authors
- **Collection Overview**: View all recipes from a specific book or author

#### User Experience
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Print-Friendly**: Generate clean print versions of recipes
- **Material Design**: Modern UI using MudBlazor components
- **Dark/Light Themes**: Automatic theme support

### Data Relationships

```mermaid
erDiagram
    Author ||--o{ Book : "writes"
    Book ||--o{ Recipe : "contains"
    
    Author {
        int id PK
        string name
        datetime creation_date
    }
    
    Book {
        int id PK
        string title
        int author_id FK
        datetime creation_date
    }
    
    Recipe {
        int id PK
        string name
        text notes
        int rating "1-5 stars with validation"
        int book_id FK
        int page_number
        datetime creation_date
    }
```

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit extension
- [Git](https://git-scm.com/)
- A Supabase account (for backend services)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/mongeon/RecettesIndex.git
   cd RecettesIndex
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Supabase**
   - Create a new project at [supabase.com](https://supabase.com)
   - Update `wwwroot/appsettings.json` with your Supabase URL and API key
   ```json
   {
     "Supabase": {
       "Url": "YOUR_SUPABASE_URL",
       "Key": "YOUR_SUPABASE_ANON_KEY"
     }
   }
   ```

4. **Run tests** (recommended before development)
   ```bash
   dotnet test
   ```

5. **Run the application**
   ```bash
   dotnet run --project src
   ```

5. **Open in browser**
   Navigate to `http://localhost:5000` (or the URL shown in the terminal)

### Database Setup

The application expects the following database schema in Supabase:

```sql
-- Authors table
CREATE TABLE authors (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    creation_date TIMESTAMP DEFAULT NOW()
);

-- Books table
CREATE TABLE books (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    author_id INTEGER REFERENCES authors(id),
    creation_date TIMESTAMP DEFAULT NOW()
);

-- Recipes table
CREATE TABLE recettes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    notes TEXT,
    rating INTEGER CHECK (rating >= 1 AND rating <= 5), -- Enforced validation
    book_id INTEGER REFERENCES books(id),
    page_number INTEGER,
    creation_date TIMESTAMP DEFAULT NOW()
);
```

## 🛠️ Development Guide

For detailed development information, see:
- [Development Guide](DEVELOPMENT.md)
- [API Reference](API.md)
- [Architecture Details](ARCHITECTURE.md)

### Quick Development Commands

```bash
# Clean and rebuild
dotnet clean && dotnet build

# Run with specific port
dotnet run --urls "http://localhost:5030"

# Run tests (comprehensive unit test suite - 243 tests)
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test file
dotnet test --filter "ClassName=RecipeModelTests"

# Publish for deployment
dotnet publish -c Release
```

### Development Workflow

```mermaid
flowchart TD
    A[Create Feature Branch] --> B[Make Changes]
    B --> C[Add Unit Tests]
    C --> D[Run Test Suite]
    D --> E{All Tests Pass?}
    E -->|No| F[Fix Tests/Code]
    F --> D
    E -->|Yes| G[Validate Changes]
    G --> H{git diff, git show}
    H --> I[Run Application]
    I --> J{Test Functionality}
    J --> K[Get User Approval]
    K --> L[Commit Changes]
    L --> M[Push to Remote]
    M --> N[Create Pull Request]
    N --> O[GitHub Actions CI/CD]
    O --> P{Tests Pass?}
    P -->|No| Q[Fix Issues]
    Q --> L
    P -->|Yes| R[Code Review]
    R --> S[Merge to Main]
    S --> T[Auto-deploy to Azure]
```

## 📚 Documentation Structure

- **README.md** (this file): Complete project overview and getting started guide
- **[DEVELOPMENT.md](DEVELOPMENT.md)**: Detailed development guidelines and best practices
- **[API.md](API.md)**: API reference and data models documentation
- **[ARCHITECTURE.md](ARCHITECTURE.md)**: System architecture and design decisions
- **[DEPLOYMENT.md](DEPLOYMENT.md)**: Deployment and hosting instructions

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

1. **Read the development docs**: Familiarize yourself with [DEVELOPMENT.md](DEVELOPMENT.md)
2. **Follow the workflow**: Always create feature branches, never work directly on main
3. **Validate changes**: Use git tools and run the application before committing
4. **Update documentation**: Keep all docs current with your changes
5. **Use GitHub MCP server**: For all GitHub operations (PRs, issues, etc.)

### Code Style

- Follow C# naming conventions (PascalCase for public members)
- Use nullable reference types throughout
- Implement async/await patterns for I/O operations
- Use MudBlazor components consistently
- Include proper error handling and loading states

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For questions or issues:
1. Check the [documentation](docs/)
2. Search existing [GitHub issues](https://github.com/mongeon/RecettesIndex/issues)
3. Create a new issue with detailed information

---

**Made with ❤️ for home cooks who love to organize their recipes**
