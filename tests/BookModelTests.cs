using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class BookModelTests
{
    [Fact]
    public void Book_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedName = "The Joy of Cooking";
        var expectedCreationDate = DateTime.Now;

        // Act
        var book = new Book
        {
            Id = 1,
            Name = expectedName,
            CreationDate = expectedCreationDate
        };

        // Assert
        Assert.Equal(1, book.Id);
        Assert.Equal(expectedName, book.Name);
        Assert.Equal(expectedCreationDate, book.CreationDate);
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    [Fact]
    public void Book_DefaultValues_AreSetCorrectly()
    {
        // Act
        var book = new Book();

        // Assert
        Assert.Equal(string.Empty, book.Name);
        Assert.NotNull(book.Authors);
        Assert.Empty(book.Authors);
    }

    [Theory]
    [InlineData("The Joy of Cooking")]
    [InlineData("Mastering the Art of French Cooking")]
    [InlineData("The New York Times Cookbook")]
    public void Book_Name_AcceptsValidValues(string bookName)
    {
        // Arrange
        var book = new Book();

        // Act
        book.Name = bookName;

        // Assert
        Assert.Equal(bookName, book.Name);
    }

    [Fact]
    public void Book_CanHaveMultipleAuthors()
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
        Assert.Equal(3, book.Authors.Count);
        Assert.Contains(book.Authors, a => a.Name == "Julia" && a.LastName == "Child");
        Assert.Contains(book.Authors, a => a.Name == "Louisette" && a.LastName == "Bertholle");
        Assert.Contains(book.Authors, a => a.Name == "Simone" && a.LastName == "Beck");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("Very Long Book Title That Goes On And On To Test Maximum Length Handling")]
    public void Book_Name_AcceptsVariousStringLengths(string name)
    {
        // Arrange
        var book = new Book();

        // Act
        book.Name = name;

        // Assert
        Assert.Equal(name, book.Name);
    }

    [Fact]
    public void Book_CreationDate_CanBeSet()
    {
        // Arrange
        var book = new Book();
        var expectedDate = new DateTime(2023, 5, 15);

        // Act
        book.CreationDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, book.CreationDate);
    }
}
