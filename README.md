# Mes Recettes 🍽️

A modern, personal recipe management application built with Blazor WebAssembly and Supabase. Organize your favorite recipes, associate them with cookbooks and authors, and never lose track of your culinary treasures again!

## ✨ Features

- 📝 **Recipe Management**: Create, edit, and organize your favorite recipes
- 📚 **Cookbook Integration**: Associate recipes with physical or digital cookbooks
- ⭐ **Rating System**: Rate recipes from 1-5 stars for easy favorites tracking
- 📄 **Page References**: Track page numbers for cookbook recipes
- 🖨️ **Print-Friendly**: Generate clean, printable versions of recipes
- 📱 **Responsive Design**: Works beautifully on desktop, tablet, and mobile
- 🔍 **Search & Filter**: Find recipes by name, rating, cookbook, or author

## 🚀 Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- A [Supabase](https://supabase.com) account (free tier available)

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
   - Update `wwwroot/appsettings.json`:
   ```json
   {
     "Supabase": {
       "Url": "YOUR_SUPABASE_URL",
       "Key": "YOUR_SUPABASE_ANON_KEY"
     }
   }
   ```

4. **Set up the database**
   Run this SQL in your Supabase SQL editor:
   ```sql
   -- Create tables
   CREATE TABLE authors (
       id SERIAL PRIMARY KEY,
       name VARCHAR(255) NOT NULL,
       creation_date TIMESTAMP DEFAULT NOW()
   );

   CREATE TABLE books (
       id SERIAL PRIMARY KEY,
       title VARCHAR(255) NOT NULL,
       author_id INTEGER REFERENCES authors(id),
       creation_date TIMESTAMP DEFAULT NOW()
   );

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

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Open in browser**
   Navigate to `http://localhost:5000`

## 🏗️ Tech Stack

- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **UI Framework**: MudBlazor (Material Design)
- **Backend**: Supabase (PostgreSQL + REST API)
- **Authentication**: Supabase Auth
- **Hosting**: Static web hosting (GitHub Pages, Netlify, Vercel, etc.)

## 📖 Documentation

For detailed information, visit our comprehensive documentation:

- 📋 **[Complete Documentation](docs/README.md)** - Full project overview and guides
- 🛠️ **[Development Guide](docs/DEVELOPMENT.md)** - Coding standards and best practices  
- 🔌 **[API Reference](docs/API.md)** - Data models and service documentation
- 🏗️ **[Architecture Guide](docs/ARCHITECTURE.md)** - System design and technical decisions
- 🚀 **[Deployment Guide](docs/DEPLOYMENT.md)** - Hosting and CI/CD setup

## 🛠️ Development

### Development Workflow

1. **Always create a feature branch** (never work directly on main)
2. **Validate all changes** using `git diff` and `git show --stat`
3. **Run and test the application** before committing
4. **Update documentation** when making changes
5. **Get user approval** before creating commits or PRs
6. **Use GitHub MCP server** for all GitHub operations

### Quick Commands

```bash
# Clean and rebuild
dotnet clean && dotnet build

# Run with hot reload
dotnet run

# Run on specific port
dotnet run --urls "http://localhost:5030"

# Publish for production
dotnet publish -c Release
```

### Project Structure

```
RecettesIndex/
├── 📁 Configuration/     # App configuration
├── 📁 docs/             # Project documentation  
├── 📁 Layout/           # App layout components
├── 📁 Models/           # Data models
├── 📁 Pages/            # Blazor pages and dialogs
├── 📁 Services/         # Business logic
├── 📁 wwwroot/          # Static assets
├── 📄 Program.cs        # App entry point
└── 📄 _Imports.razor    # Global imports
```

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Follow our development workflow** (see documentation)
4. **Make your changes and test thoroughly**
5. **Update documentation** to reflect your changes
6. **Commit with clear messages**: `git commit -m "Add amazing feature"`
7. **Push to your fork**: `git push origin feature/amazing-feature`
8. **Create a Pull Request**

### Development Guidelines

- Follow C# conventions and use nullable reference types
- Use MudBlazor components for consistent UI
- Implement proper async/await patterns
- Include error handling and loading states
- Write tests for new features
- Keep documentation up to date

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- 📚 Check the [documentation](docs/)
- 🐛 Report issues on [GitHub](https://github.com/mongeon/RecettesIndex/issues)
- 💬 Ask questions in [Discussions](https://github.com/mongeon/RecettesIndex/discussions)

## 🙏 Acknowledgments

- Built with [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- UI powered by [MudBlazor](https://mudblazor.com/)
- Backend services by [Supabase](https://supabase.com/)
- Icons from [Material Design Icons](https://materialdesignicons.com/)

---

**Made with ❤️ for home cooks who love to organize their recipes**

