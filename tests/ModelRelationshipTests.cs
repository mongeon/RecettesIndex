using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class ModelRelationshipTests
{
    [Fact]
    public void Recipe_BookRelationship_CanBeEstablished()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "The Joy of Cooking" };
        var recipe = new Recipe { Id = 1, Name = "Chocolate Chip Cookies" };

        // Act
        recipe.BookId = book.Id;
        recipe.Book = book;

        // Assert
        Assert.Equal(book.Id, recipe.BookId);
        Assert.Equal(book, recipe.Book);
        Assert.Equal("The Joy of Cooking", recipe.Book.Name);
    }

    [Fact]
    public void Recipe_CanExistWithoutBook()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Family Secret Recipe",
            Rating = 5,
            Notes = "Passed down through generations"
        };

        // Assert
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.Book);
        Assert.Null(recipe.BookPage);
        Assert.Equal("Family Secret Recipe", recipe.Name);
    }

    [Fact]
    public void BookAuthor_CreatesProperManyToManyRelationship()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "Mastering the Art of French Cooking" };
        var author1 = new Author { Id = 1, Name = "Julia", LastName = "Child" };
        var author2 = new Author { Id = 2, Name = "Louisette", LastName = "Bertholle" };

        // Act
        var bookAuthor1 = new BookAuthor { BookId = book.Id, AuthorId = author1.Id };
        var bookAuthor2 = new BookAuthor { BookId = book.Id, AuthorId = author2.Id };

        // Assert
        Assert.Equal(book.Id, bookAuthor1.BookId);
        Assert.Equal(author1.Id, bookAuthor1.AuthorId);
        Assert.Equal(book.Id, bookAuthor2.BookId);
        Assert.Equal(author2.Id, bookAuthor2.AuthorId);
    }

    [Fact]
    public void Author_CanHaveBooksCollection()
    {
        // Arrange
        var author = new Author { Id = 1, Name = "Julia", LastName = "Child" };
        var books = new List<Book>
        {
            new Book { Id = 1, Name = "Mastering the Art of French Cooking" },
            new Book { Id = 2, Name = "My Life in France" }
        };

        // Act
        author.Books = books;

        // Assert
        Assert.NotNull(author.Books);
        Assert.Equal(2, author.Books.Count);
        Assert.Contains(author.Books, b => b.Name.Contains("Mastering"));
        Assert.Contains(author.Books, b => b.Name.Contains("My Life"));
    }

    [Fact]
    public void Book_CanHaveAuthorsCollection()
    {
        // Arrange
        var book = new Book { Id = 1, Name = "Mastering the Art of French Cooking" };
        var authors = new List<Author>
        {
            new Author { Id = 1, Name = "Julia", LastName = "Child" },
            new Author { Id = 2, Name = "Louisette", LastName = "Bertholle" },
            new Author { Id = 3, Name = "Simone", LastName = "Beck" }
        };

        // Act
        book.Authors = authors;

        // Assert
        Assert.NotNull(book.Authors);
        Assert.Equal(3, book.Authors.Count);
        Assert.Contains(book.Authors, a => a.FullName == "Julia Child");
        Assert.Contains(book.Authors, a => a.FullName == "Louisette Bertholle");
        Assert.Contains(book.Authors, a => a.FullName == "Simone Beck");
    }

    [Theory]
    [InlineData(1, 5, 150)]
    [InlineData(2, 3, 75)]
    [InlineData(null, 4, null)]
    public void Recipe_BookPageReference_WorksWithVariousValues(int? bookId, int rating, int? page)
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Test Recipe",
            Rating = rating
        };

        // Act
        recipe.BookId = bookId;
        recipe.BookPage = page;

        // Assert
        Assert.Equal(bookId, recipe.BookId);
        Assert.Equal(page, recipe.BookPage);
        Assert.Equal(rating, recipe.Rating);
    }
}
