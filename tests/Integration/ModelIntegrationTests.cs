using RecettesIndex.Models;

namespace RecettesIndex.Tests.Integration;

/// <summary>
/// Integration tests that validate interactions between models and their relationships.
/// These tests ensure data integrity across the domain model.
/// </summary>
public class ModelIntegrationTests
{
    [Fact]
    public void Recipe_Book_Author_FullRelationship_WorksCorrectly()
    {
        // Arrange - Create a complete relationship chain
        var author = new Author
        {
            Id = 1,
            Name = "Julia",
            LastName = "Child",
            CreationDate = DateTime.Now
        };

        var book = new Book
        {
            Id = 1,
            Name = "Mastering the Art of French Cooking",
            CreationDate = DateTime.Now,
            Authors = new List<Author> { author }
        };

        var recipe = new Recipe
        {
            Id = 1,
            Name = "Boeuf Bourguignon",
            Rating = 5,
            BookId = book.Id,
            Book = book,
            BookPage = 315,
            Notes = "Classic French stew",
            CreationDate = DateTime.Now
        };

        // Act - Verify the chain
        var bookFromRecipe = recipe.Book;
        var authorsFromBook = bookFromRecipe?.Authors;
        var firstAuthor = authorsFromBook?.FirstOrDefault();

        // Assert
        Assert.NotNull(bookFromRecipe);
        Assert.Equal("Mastering the Art of French Cooking", bookFromRecipe.Name);
        Assert.NotNull(authorsFromBook);
        Assert.Single(authorsFromBook);
        Assert.NotNull(firstAuthor);
        Assert.Equal("Julia", firstAuthor.Name);
        Assert.Equal("Child", firstAuthor.LastName);
    }

    [Fact]
    public void MultipleRecipes_SameBook_ShareReference()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Name = "Joy of Cooking",
            CreationDate = DateTime.Now
        };

        var recipe1 = new Recipe
        {
            Id = 1,
            Name = "Recipe 1",
            Rating = 4,
            BookId = book.Id,
            Book = book
        };

        var recipe2 = new Recipe
        {
            Id = 2,
            Name = "Recipe 2",
            Rating = 5,
            BookId = book.Id,
            Book = book
        };

        // Act & Assert
        Assert.Same(recipe1.Book, recipe2.Book);
        Assert.Equal(recipe1.BookId, recipe2.BookId);
    }

    [Fact]
    public void Author_MultipleBooks_ManyToManyRelationship()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "Gordon",
            LastName = "Ramsay"
        };

        var book1 = new Book { Id = 1, Name = "Book 1", Authors = new List<Author> { author } };
        var book2 = new Book { Id = 2, Name = "Book 2", Authors = new List<Author> { author } };
        var book3 = new Book { Id = 3, Name = "Book 3", Authors = new List<Author> { author } };

        // Act
        var books = new List<Book> { book1, book2, book3 };
        var allHaveSameAuthor = books.All(b => b.Authors.Contains(author));

        // Assert
        Assert.True(allHaveSameAuthor);
        Assert.All(books, b => Assert.Single(b.Authors));
    }

    [Fact]
    public void Book_MultipleAuthors_CollaborativeWork()
    {
        // Arrange
        var author1 = new Author { Id = 1, Name = "Author", LastName = "One" };
        var author2 = new Author { Id = 2, Name = "Author", LastName = "Two" };
        var author3 = new Author { Id = 3, Name = "Author", LastName = "Three" };

        var book = new Book
        {
            Id = 1,
            Name = "Collaborative Cookbook",
            Authors = new List<Author> { author1, author2, author3 }
        };

        // Act
        var authorCount = book.Authors.Count;
        var allAuthorsHaveDistinctIds = book.Authors.Select(a => a.Id).Distinct().Count() == authorCount;

        // Assert
        Assert.Equal(3, authorCount);
        Assert.True(allAuthorsHaveDistinctIds);
    }

    [Fact]
    public void BookAuthor_Junction_CreatesValidAssociation()
    {
        // Arrange - Simulate many-to-many relationship
        var book1 = new Book { Id = 1, Name = "Book 1" };
        var book2 = new Book { Id = 2, Name = "Book 2" };
        var author1 = new Author { Id = 1, Name = "Author 1" };
        var author2 = new Author { Id = 2, Name = "Author 2" };

        // Create junction table entries
        var associations = new List<BookAuthor>
        {
            new() { BookId = book1.Id, AuthorId = author1.Id },
            new() { BookId = book1.Id, AuthorId = author2.Id },
            new() { BookId = book2.Id, AuthorId = author1.Id }
        };

        // Act
        var book1Authors = associations.Where(ba => ba.BookId == book1.Id).Select(ba => ba.AuthorId).ToList();
        var book2Authors = associations.Where(ba => ba.BookId == book2.Id).Select(ba => ba.AuthorId).ToList();
        var author1Books = associations.Where(ba => ba.AuthorId == author1.Id).Select(ba => ba.BookId).ToList();

        // Assert
        Assert.Equal(2, book1Authors.Count); // Book 1 has 2 authors
        Assert.Single(book2Authors); // Book 2 has 1 author
        Assert.Equal(2, author1Books.Count); // Author 1 has 2 books
    }

    [Fact]
    public void Recipe_WithoutBook_IsIndependent()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Family Recipe",
            Rating = 5,
            Notes = "Passed down through generations",
            BookId = null,
            Book = null
        };

        // Assert
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.Book);
        Assert.NotNull(recipe.Name);
        Assert.NotNull(recipe.Notes);
    }

    [Fact]
    public void CreationDates_AreConsistent_AcrossRelatedEntities()
    {
        // Arrange
        var baseDate = new DateTime(2024, 1, 1, 10, 0, 0);

        var author = new Author
        {
            Id = 1,
            Name = "Test",
            CreationDate = baseDate
        };

        var book = new Book
        {
            Id = 1,
            Name = "Test Book",
            CreationDate = baseDate.AddMinutes(5),
            Authors = new List<Author> { author }
        };

        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Rating = 3,
            Book = book,
            BookId = book.Id,
            CreationDate = baseDate.AddMinutes(10)
        };

        // Act & Assert
        Assert.True(author.CreationDate < book.CreationDate);
        Assert.True(book.CreationDate < recipe.CreationDate);
        Assert.Equal(TimeSpan.FromMinutes(10), recipe.CreationDate - author.CreationDate);
    }

    [Fact]
    public void Recipe_Collection_CanBeGroupedByBook()
    {
        // Arrange
        var book1 = new Book { Id = 1, Name = "Book 1" };
        var book2 = new Book { Id = 2, Name = "Book 2" };

        var recipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Recipe 1", Rating = 3, BookId = 1, Book = book1 },
            new() { Id = 2, Name = "Recipe 2", Rating = 4, BookId = 1, Book = book1 },
            new() { Id = 3, Name = "Recipe 3", Rating = 5, BookId = 2, Book = book2 },
            new() { Id = 4, Name = "Recipe 4", Rating = 3, BookId = null, Book = null }
        };

        // Act
        var groupedByBook = recipes
            .Where(r => r.BookId.HasValue)
            .GroupBy(r => r.BookId)
            .ToList();

        var book1Recipes = recipes.Count(r => r.BookId == 1);
        var book2Recipes = recipes.Count(r => r.BookId == 2);
        var noBookRecipes = recipes.Count(r => r.BookId == null);

        // Assert
        Assert.Equal(2, groupedByBook.Count);
        Assert.Equal(2, book1Recipes);
        Assert.Equal(1, book2Recipes);
        Assert.Equal(1, noBookRecipes);
    }

    [Fact]
    public void Recipe_Rating_CanBeAveragedAcrossCollection()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Name = "R1", Rating = 5 },
            new() { Name = "R2", Rating = 4 },
            new() { Name = "R3", Rating = 3 },
            new() { Name = "R4", Rating = 5 },
            new() { Name = "R5", Rating = 3 }
        };

        // Act
        var averageRating = recipes.Average(r => r.Rating);

        // Assert
        Assert.Equal(4.0, averageRating);
    }

    [Fact]
    public void Author_FullName_DisplaysCorrectly_InCollections()
    {
        // Arrange
        var authors = new List<Author>
        {
            new() { Id = 1, Name = "Julia", LastName = "Child" },
            new() { Id = 2, Name = "Gordon", LastName = "Ramsay" },
            new() { Id = 3, Name = "Jamie", LastName = "Oliver" }
        };

        // Act
        var fullNames = authors.Select(a => a.FullName).ToList();

        // Assert
        Assert.Equal(3, fullNames.Count);
        Assert.Contains("Julia Child", fullNames);
        Assert.Contains("Gordon Ramsay", fullNames);
        Assert.Contains("Jamie Oliver", fullNames);
    }
}
