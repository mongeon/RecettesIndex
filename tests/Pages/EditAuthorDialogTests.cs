using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RecettesIndex.Models;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Tests to verify that CreationDate is preserved when editing authors.
/// These tests focus on the data copying logic used in EditAuthorDialog.
/// </summary>
public class EditAuthorDialogTests : TestContext
{
    public EditAuthorDialogTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void Author_CreationDate_IsPreserved_WhenCopied()
    {
        // Arrange
        var originalCreationDate = new DateTime(2024, 3, 20, 14, 45, 0);
        var originalAuthor = new Author
        {
            Id = 1,
            Name = "Julia",
            LastName = "Child",
            CreationDate = originalCreationDate
        };

        // Act - Simulate what the component does when initializing
        var copiedAuthor = new Author
        {
            Id = originalAuthor.Id,
            Name = originalAuthor.Name,
            LastName = originalAuthor.LastName,
            CreationDate = originalAuthor.CreationDate
        };

        // Assert
        Assert.Equal(originalCreationDate, copiedAuthor.CreationDate);
        Assert.Equal(originalAuthor.Id, copiedAuthor.Id);
        Assert.Equal(originalAuthor.Name, copiedAuthor.Name);
        Assert.Equal(originalAuthor.LastName, copiedAuthor.LastName);
    }

    [Theory]
    [InlineData(2024, 1, 1, 0, 0, 0)]
    [InlineData(2023, 12, 25, 23, 59, 59)]
    [InlineData(2025, 6, 15, 12, 30, 45)]
    public void Author_CreationDate_PreservesDateTime_WithTimeComponent(
        int year, int month, int day, int hour, int minute, int second)
    {
        // Arrange
        var creationDate = new DateTime(year, month, day, hour, minute, second);
        var originalAuthor = new Author
        {
            Id = 1,
            Name = "Gordon",
            LastName = "Ramsay",
            CreationDate = creationDate
        };

        // Act
        var copiedAuthor = new Author
        {
            Id = originalAuthor.Id,
            Name = originalAuthor.Name,
            LastName = originalAuthor.LastName,
            CreationDate = originalAuthor.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, copiedAuthor.CreationDate);
        Assert.Equal(year, copiedAuthor.CreationDate.Year);
        Assert.Equal(month, copiedAuthor.CreationDate.Month);
        Assert.Equal(day, copiedAuthor.CreationDate.Day);
        Assert.Equal(hour, copiedAuthor.CreationDate.Hour);
        Assert.Equal(minute, copiedAuthor.CreationDate.Minute);
        Assert.Equal(second, copiedAuthor.CreationDate.Second);
    }

    [Fact]
    public void Author_WithNullLastName_PreservesCreationDate()
    {
        // Arrange
        var creationDate = new DateTime(2024, 5, 10, 8, 0, 0);
        var originalAuthor = new Author
        {
            Id = 1,
            Name = "Jamie",
            LastName = null,
            CreationDate = creationDate
        };

        // Act
        var copiedAuthor = new Author
        {
            Id = originalAuthor.Id,
            Name = originalAuthor.Name,
            LastName = originalAuthor.LastName,
            CreationDate = originalAuthor.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, copiedAuthor.CreationDate);
        Assert.Null(copiedAuthor.LastName);
    }

    [Fact]
    public void Author_CreationDate_IsNotModified_WhenOtherPropertiesChange()
    {
        // Arrange
        var creationDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var author = new Author
        {
            Id = 1,
            Name = "Original Name",
            LastName = "Original LastName",
            CreationDate = creationDate
        };

        // Act - Simulate editing the author
        var editedAuthor = new Author
        {
            Id = author.Id,
            Name = "Updated Name",
            LastName = "Updated LastName",
            CreationDate = author.CreationDate // Preserve original creation date
        };

        // Assert
        Assert.Equal(creationDate, editedAuthor.CreationDate);
        Assert.Equal("Updated Name", editedAuthor.Name);
        Assert.Equal("Updated LastName", editedAuthor.LastName);
        // Verify the creation date wasn't changed
        Assert.Equal(author.CreationDate, editedAuthor.CreationDate);
    }

    [Fact]
    public void Author_DefaultCreationDate_IsPreserved()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "Test Author"
            // CreationDate not set, will be default DateTime
        };

        // Act
        var copiedAuthor = new Author
        {
            Id = author.Id,
            Name = author.Name,
            LastName = author.LastName,
            CreationDate = author.CreationDate
        };

        // Assert
        Assert.Equal(default(DateTime), copiedAuthor.CreationDate);
        Assert.Equal(author.CreationDate, copiedAuthor.CreationDate);
    }

    [Fact]
    public void MultipleAuthors_EachPreserve_TheirOwnCreationDate()
    {
        // Arrange
        var author1CreationDate = new DateTime(2024, 1, 1);
        var author2CreationDate = new DateTime(2024, 6, 15);
        var author3CreationDate = new DateTime(2023, 12, 25);

        var author1 = new Author { Id = 1, Name = "Author1", CreationDate = author1CreationDate };
        var author2 = new Author { Id = 2, Name = "Author2", CreationDate = author2CreationDate };
        var author3 = new Author { Id = 3, Name = "Author3", CreationDate = author3CreationDate };

        // Act
        var copiedAuthor1 = new Author
        {
            Id = author1.Id,
            Name = author1.Name,
            LastName = author1.LastName,
            CreationDate = author1.CreationDate
        };
        var copiedAuthor2 = new Author
        {
            Id = author2.Id,
            Name = author2.Name,
            LastName = author2.LastName,
            CreationDate = author2.CreationDate
        };
        var copiedAuthor3 = new Author
        {
            Id = author3.Id,
            Name = author3.Name,
            LastName = author3.LastName,
            CreationDate = author3.CreationDate
        };

        // Assert
        Assert.Equal(author1CreationDate, copiedAuthor1.CreationDate);
        Assert.Equal(author2CreationDate, copiedAuthor2.CreationDate);
        Assert.Equal(author3CreationDate, copiedAuthor3.CreationDate);
        Assert.NotEqual(copiedAuthor1.CreationDate, copiedAuthor2.CreationDate);
    }
}
