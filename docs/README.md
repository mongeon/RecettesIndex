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
    A --> F[Hosting - Static Web Host]
    
    subgraph "Development Tools"
        G[VS Code + C# Dev Kit]
        H[GitHub Actions CI/CD]
        I[GitHub MCP Server]
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
├── 📁 Configuration/          # App configuration files
│   └── SupabaseConfig.cs
├── 📁 docs/                   # Project documentation
│   ├── README.md
│   ├── API.md
│   ├── ARCHITECTURE.md
│   └── DEVELOPMENT.md
├── 📁 Layout/                 # Application layout components
│   ├── MainLayout.razor
│   └── NavMenu.razor
├── 📁 Models/                 # Data models
│   ├── Recette.cs
│   ├── Book.cs
│   └── Author.cs
├── 📁 Pages/                  # Blazor pages and dialogs
│   ├── Home.razor
│   ├── Recipes.razor
│   ├── Books.razor
│   ├── Authors.razor
│   └── *Dialog.razor
├── 📁 Services/               # Business logic and data access
│   ├── AuthService.cs
│   └── BookAuthorService.cs
├── 📁 wwwroot/                # Static assets
│   ├── index.html
│   ├── css/
│   └── icons/
├── 📄 Program.cs              # Application entry point
└── 📄 _Imports.razor          # Global using statements
```

## ✨ Features

### Core Functionality

#### Recipe Management
- **CRUD Operations**: Create, read, update, and delete recipes
- **Rich Text Support**: Store ingredients, instructions, and personal notes
- **Rating System**: 1-5 star rating system for recipe evaluation
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
        int rating
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

4. **Run the application**
   ```bash
   dotnet run
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
    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
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

# Run tests (when available)
dotnet test

# Publish for deployment
dotnet publish -c Release
```

### Development Workflow

```mermaid
flowchart TD
    A[Create Feature Branch] --> B[Make Changes]
    B --> C[Validate Changes]
    C --> D{git diff, git show}
    D --> E[Run Application]
    E --> F{Test Functionality}
    F --> G[Get User Approval]
    G --> H[Commit Changes]
    H --> I[Push to Remote]
    I --> J[Create Pull Request]
    J --> K[Code Review]
    K --> L[Merge to Main]
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
