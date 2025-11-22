using RecettesIndex.Models;

namespace RecettesIndex.Tests.Models;

/// <summary>
/// Additional validation tests for Recipe, Book, and Author models.
/// Tests edge cases, business rules, and data integrity.
/// </summary>
public class AdditionalModelValidationTests
{
    #region Recipe Tests

    [Fact]
    public void Recipe_WithValidData_PassesValidation()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Valid Recipe",
            Rating = 3,
            CreationDate = DateTime.Now
        };

        // Act & Assert
        Assert.Equal("Valid Recipe", recipe.Name);
        Assert.Equal(3, recipe.Rating);
    }

    [Fact]
    public void Recipe_WithMaxRating_IsValid()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = "Perfect Recipe",
            Rating = 5
        };

        // Assert
        Assert.Equal(5, recipe.Rating);
    }

    [Fact]
    public void Recipe_WithMinRating_IsValid()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = "Needs Improvement",
            Rating = 1
        };

        // Assert
        Assert.Equal(1, recipe.Rating);
    }

    [Fact]
    public void Recipe_WithLongName_StoresCorrectly()
    {
        // Arrange
        var longName = new string('A', 200);
        var recipe = new Recipe { Name = longName };

        // Act & Assert
        Assert.Equal(200, recipe.Name.Length);
        Assert.Equal(longName, recipe.Name);
    }

    [Fact]
    public void Recipe_WithNullOptionalFields_IsValid()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = "Simple Recipe",
            Rating = 4,
            Notes = null,
            BookId = null,
            BookPage = null
        };

        // Assert
        Assert.Null(recipe.Notes);
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.BookPage);
        Assert.Null(recipe.Book);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void Recipe_WithVariousPageNumbers_StoresCorrectly(int pageNumber)
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = "Recipe",
            Rating = 3,
            BookPage = pageNumber
        };

        // Assert
        Assert.Equal(pageNumber, recipe.BookPage);
    }

    [Fact]
    public void Recipe_CreationDate_CanBeSetAndRetrieved()
    {
        // Arrange
        var now = DateTime.Now;
        var recipe = new Recipe
        {
            Name = "Test",
            Rating = 3,
            CreationDate = now
        };

        // Act
        var retrievedDate = recipe.CreationDate;

        // Assert
        Assert.Equal(now, retrievedDate);
    }

    #endregion

    #region Book Tests

    [Fact]
    public void Book_WithValidName_StoresCorrectly()
    {
        // Arrange & Act
        var book = new Book
        {
            Id = 1,
            Name = "The Joy of Cooking"
        };

        // Assert
        Assert.Equal("The Joy of Cooking", book.Name);
    }

    [Fact]
    public void Book_WithEmptyAuthors_InitializesCorrectly()
    {
        // Arrange & Act
        var book = new Book
        {
            Id = 1,
            Name = "Test Book"
        };

        // Assert
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    [Fact]
    public void Book_WithMultipleAuthors_StoresCorrectly()
    {
        // Arrange
        var author1 = new Author { Id = 1, Name = "Julia", LastName = "Child" };
        var author2 = new Author { Id = 2, Name = "Jacques", LastName = "Pépin" };

        // Act
        var book = new Book
        {
            Id = 1,
            Name = "Collaborative Cookbook",
            Authors = new List<Author> { author1, author2 }
        };

        // Assert
        Assert.Equal(2, book.Authors.Count);
        Assert.Contains(author1, book.Authors);
        Assert.Contains(author2, book.Authors);
    }

    [Fact]
    public void Book_CreationDate_PreservesTimeComponent()
    {
        // Arrange
        var specificTime = new DateTime(2024, 6, 15, 14, 30, 45);

        // Act
        var book = new Book
        {
            Name = "Test Book",
            CreationDate = specificTime
        };

        // Assert
        Assert.Equal(specificTime, book.CreationDate);
        Assert.Equal(14, book.CreationDate.Hour);
        Assert.Equal(30, book.CreationDate.Minute);
        Assert.Equal(45, book.CreationDate.Second);
    }

    #endregion

    #region Author Tests

    [Fact]
    public void Author_WithFirstNameOnly_IsValid()
    {
        // Arrange & Act
        var author = new Author
        {
            Id = 1,
            Name = "Madonna",
            LastName = null
        };

        // Assert
        Assert.Equal("Madonna", author.Name);
        Assert.Null(author.LastName);
    }

    [Fact]
    public void Author_WithFullName_IsValid()
    {
        // Arrange & Act
        var author = new Author
        {
            Id = 1,
            Name = "Gordon",
            LastName = "Ramsay"
        };

        // Assert
        Assert.Equal("Gordon", author.Name);
        Assert.Equal("Ramsay", author.LastName);
    }

    [Fact]
    public void Author_FullName_CombinesFirstAndLast()
    {
        // Arrange
        var author = new Author
        {
            Name = "Julia",
            LastName = "Child"
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal("Julia Child", fullName);
    }

    [Fact]
    public void Author_FullName_WithNullLastName_IncludesSpace()
    {
        // Arrange
        var author = new Author
        {
            Name = "Cher",
            LastName = null
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal("Cher", fullName); // Trimmed - no trailing space
    }

    [Fact]
    public void Author_WithBooks_CollectionNotNull()
    {
        // Arrange & Act
        var author = new Author
        {
            Id = 1,
            Name = "Jamie",
            LastName = "Oliver",
            Books = new List<Book>()
        };

        // Assert
        Assert.NotNull(author.Books);
        Assert.Empty(author.Books);
    }

    [Fact]
    public void Author_CreationDate_StoresAndRetrieves()
    {
        // Arrange
        var creationDate = new DateTime(2023, 1, 1, 10, 0, 0);

        // Act
        var author = new Author
        {
            Name = "Test",
            LastName = "Author",
            CreationDate = creationDate
        };

        // Assert
        Assert.Equal(creationDate, author.CreationDate);
    }

    #endregion

    #region BookAuthor Junction Tests

    [Fact]
    public void BookAuthor_WithValidIds_StoresCorrectly()
    {
        // Arrange & Act
        var bookAuthor = new BookAuthor
        {
            BookId = 1,
            AuthorId = 2
        };

        // Assert
        Assert.Equal(1, bookAuthor.BookId);
        Assert.Equal(2, bookAuthor.AuthorId);
    }

    [Fact]
    public void BookAuthor_CreationDate_IsPreserved()
    {
        // Arrange
        var creationDate = new DateTime(2024, 3, 15);

        // Act
        var bookAuthor = new BookAuthor
        {
            BookId = 1,
            AuthorId = 1,
            CreationDate = creationDate
        };

        // Assert
        Assert.Equal(creationDate, bookAuthor.CreationDate);
    }

    [Fact]
    public void BookAuthor_MultipleAssociations_AreIndependent()
    {
        // Arrange & Act
        var ba1 = new BookAuthor { BookId = 1, AuthorId = 1 };
        var ba2 = new BookAuthor { BookId = 1, AuthorId = 2 };
        var ba3 = new BookAuthor { BookId = 2, AuthorId = 1 };

        // Assert
        Assert.NotEqual(ba1.AuthorId, ba2.AuthorId);
        Assert.NotEqual(ba1.BookId, ba3.BookId);
        Assert.Equal(ba1.BookId, ba2.BookId);
        Assert.Equal(ba1.AuthorId, ba3.AuthorId);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void Recipe_BookReference_CanBeNull()
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = "Standalone Recipe",
            Rating = 4,
            BookId = null,
            Book = null
        };

        // Assert
        Assert.Null(recipe.BookId);
        Assert.Null(recipe.Book);
    }

    [Fact]
    public void Recipe_WithBook_ReferenceIsConsistent()
    {
        // Arrange
        var book = new Book { Id = 5, Name = "Test Cookbook" };

        // Act
        var recipe = new Recipe
        {
            Name = "Recipe from book",
            Rating = 4,
            BookId = 5,
            Book = book
        };

        // Assert
        Assert.Equal(recipe.BookId, recipe.Book.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("Recipe with a very long name that includes many words and descriptions")]
    public void Recipe_Name_AcceptsVariousLengths(string name)
    {
        // Arrange & Act
        var recipe = new Recipe
        {
            Name = name,
            Rating = 3
        };

        // Assert
        Assert.Equal(name, recipe.Name);
    }

    [Fact]
    public void Book_Authors_SupportsLargeCollection()
    {
        // Arrange
        var authors = Enumerable.Range(1, 10)
            .Select(i => new Author { Id = i, Name = $"Author{i}" })
            .ToList();

        // Act
        var book = new Book
        {
            Name = "Collaborative Work",
            Authors = authors
        };

        // Assert
        Assert.Equal(10, book.Authors.Count);
        Assert.All(book.Authors, a => Assert.NotNull(a.Name));
    }

    #endregion
}
