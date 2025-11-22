using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class AuthorModelTests
{
    [Fact]
    public void Author_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedFirstName = "Julia";
        var expectedLastName = "Child";
        var expectedCreationDate = DateTime.Now;

        // Act
        var author = new Author
        {
            Id = 1,
            Name = expectedFirstName,
            LastName = expectedLastName,
            CreationDate = expectedCreationDate
        };

        // Assert
        Assert.Equal(1, author.Id);
        Assert.Equal(expectedFirstName, author.Name);
        Assert.Equal(expectedLastName, author.LastName);
        Assert.Equal(expectedCreationDate, author.CreationDate);
    }

    [Fact]
    public void Author_FullName_ReturnsCorrectFormat()
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
    public void Author_FullName_WithNullLastName_ReturnsCorrectFormat()
    {
        // Arrange
        var author = new Author
        {
            Name = "Julia",
            LastName = null
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal("Julia", fullName); // Trimmed - no trailing space
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("Marie", "Curie", "Marie Curie")]
    [InlineData("Leonardo", "da Vinci", "Leonardo da Vinci")]
    public void Author_FullName_VariousNames_ReturnsCorrectFormat(string firstName, string lastName, string expected)
    {
        // Arrange
        var author = new Author
        {
            Name = firstName,
            LastName = lastName
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal(expected, fullName);
    }

    [Fact]
    public void Author_DefaultValues_AreSetCorrectly()
    {
        // Act
        var author = new Author();

        // Assert
        Assert.Equal(string.Empty, author.Name);
        Assert.Null(author.LastName);
        // Books collection is initialized as [] (empty list) in the model
        Assert.NotNull(author.Books);
        Assert.Empty(author.Books);
    }

    [Theory]
    [InlineData("", "", "")] // Both empty - returns empty string (trimmed)
    [InlineData("SingleName", "", "SingleName")] // Only first name (trimmed)
    [InlineData("", "LastOnly", "LastOnly")] // Only last name (trimmed)
    public void Author_FullName_EdgeCases_ReturnsCorrectFormat(string firstName, string lastName, string expected)
    {
        // Arrange
        var author = new Author
        {
            Name = firstName,
            LastName = string.IsNullOrEmpty(lastName) ? null : lastName
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal(expected, fullName);
    }

    [Theory]
    [InlineData("JOHN", "DOE", "JOHN DOE")]
    [InlineData("maría", "garcía", "maría garcía")]
    [InlineData("Jean-Claude", "Van Damme", "Jean-Claude Van Damme")]
    public void Author_FullName_PreservesOriginalCasing(string firstName, string lastName, string expected)
    {
        // Arrange
        var author = new Author
        {
            Name = firstName,
            LastName = lastName
        };

        // Act
        var fullName = author.FullName;

        // Assert
        Assert.Equal(expected, fullName);
    }

    [Fact]
    public void Author_CanHaveMultipleBooks()
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
        Assert.Equal(2, author.Books.Count);
        Assert.Contains(author.Books, b => b.Name == "Mastering the Art of French Cooking");
        Assert.Contains(author.Books, b => b.Name == "My Life in France");
    }
}
