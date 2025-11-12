using RecettesIndex.Models;

namespace RecettesIndex.Tests;

public class BookAuthorModelTests
{
    [Fact]
    public void BookAuthor_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedBookId = 1;
        var expectedAuthorId = 2;
        var expectedCreationDate = DateTime.Now;

        // Act
        var bookAuthor = new BookAuthor
        {
            BookId = expectedBookId,
            AuthorId = expectedAuthorId,
            CreationDate = expectedCreationDate
        };

        // Assert
        Assert.Equal(expectedBookId, bookAuthor.BookId);
        Assert.Equal(expectedAuthorId, bookAuthor.AuthorId);
        Assert.Equal(expectedCreationDate, bookAuthor.CreationDate);
    }

    [Fact]
    public void BookAuthor_DefaultValues_AreSetCorrectly()
    {
        // Act
        var bookAuthor = new BookAuthor();

        // Assert
        Assert.Equal(0, bookAuthor.BookId);
        Assert.Equal(0, bookAuthor.AuthorId);
        Assert.Equal(default(DateTime), bookAuthor.CreationDate);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 10)]
    [InlineData(100, 200)]
    public void BookAuthor_Ids_AcceptValidValues(int bookId, int authorId)
    {
        // Arrange
        var bookAuthor = new BookAuthor();

        // Act
        bookAuthor.BookId = bookId;
        bookAuthor.AuthorId = authorId;

        // Assert
        Assert.Equal(bookId, bookAuthor.BookId);
        Assert.Equal(authorId, bookAuthor.AuthorId);
    }

    [Fact]
    public void BookAuthor_CreationDate_CanBeSet()
    {
        // Arrange
        var bookAuthor = new BookAuthor();
        var expectedDate = new DateTime(2023, 5, 15);

        // Act
        bookAuthor.CreationDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, bookAuthor.CreationDate);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void BookAuthor_AcceptsEdgeCaseIds(int bookId, int authorId)
    {
        // Arrange
        var bookAuthor = new BookAuthor();

        // Act
        bookAuthor.BookId = bookId;
        bookAuthor.AuthorId = authorId;

        // Assert
        Assert.Equal(bookId, bookAuthor.BookId);
        Assert.Equal(authorId, bookAuthor.AuthorId);
    }

    [Fact]
    public void BookAuthor_CanRepresentManyToManyRelationship()
    {
        // Arrange & Act
        var bookAuthor1 = new BookAuthor { BookId = 1, AuthorId = 1 }; // Book 1 has Author 1
        var bookAuthor2 = new BookAuthor { BookId = 1, AuthorId = 2 }; // Book 1 has Author 2
        var bookAuthor3 = new BookAuthor { BookId = 2, AuthorId = 1 }; // Book 2 has Author 1

        // Assert
        Assert.Equal(1, bookAuthor1.BookId);
        Assert.Equal(1, bookAuthor1.AuthorId);

        Assert.Equal(1, bookAuthor2.BookId);
        Assert.Equal(2, bookAuthor2.AuthorId);

        Assert.Equal(2, bookAuthor3.BookId);
        Assert.Equal(1, bookAuthor3.AuthorId);
    }
}
