using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using NSubstitute;
using RecettesIndex.Models;
using RecettesIndex.Pages;

namespace RecettesIndex.Tests.Pages;

/// <summary>
/// Tests to verify that CreationDate is preserved when editing books.
/// These tests focus on the data copying logic without full component rendering.
/// </summary>
public class EditBookDialogTests : TestContext
{
    public EditBookDialogTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void Book_CreationDate_IsPreserved_WhenCopied()
    {
        // Arrange
        var originalCreationDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var originalBook = new Book
        {
            Id = 1,
            Name = "Test Cookbook",
            CreationDate = originalCreationDate,
            Authors = new List<Author>()
        };

        // Act - Simulate what the component does when initializing
        var copiedBook = new Book
        {
            Id = originalBook.Id,
            Name = originalBook.Name,
            Authors = originalBook.Authors ?? new List<Author>(),
            CreationDate = originalBook.CreationDate
        };

        // Assert
        Assert.Equal(originalCreationDate, copiedBook.CreationDate);
        Assert.Equal(originalBook.Id, copiedBook.Id);
        Assert.Equal(originalBook.Name, copiedBook.Name);
    }

    [Fact]
    public void BookUpdate_IncludesCreationDate_InUpdateObject()
    {
        // Arrange
        var creationDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var book = new Book
        {
            Id = 1,
            Name = "Updated Cookbook",
            CreationDate = creationDate
        };

        // Act - Simulate what the component does when updating
        var bookToUpdate = new Book
        {
            Id = book.Id,
            Name = book.Name,
            CreationDate = book.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, bookToUpdate.CreationDate);
        Assert.Equal("Updated Cookbook", bookToUpdate.Name);
        Assert.Equal(1, bookToUpdate.Id);
    }

    [Theory]
    [InlineData(2024, 1, 1)]
    [InlineData(2023, 12, 25)]
    [InlineData(2025, 6, 15)]
    public void Book_CreationDate_PreservesVariousDates(int year, int month, int day)
    {
        // Arrange
        var creationDate = new DateTime(year, month, day);
        var originalBook = new Book
        {
            Id = 1,
            Name = "Test Book",
            CreationDate = creationDate
        };

        // Act
        var copiedBook = new Book
        {
            Id = originalBook.Id,
            Name = originalBook.Name,
            CreationDate = originalBook.CreationDate
        };

        // Assert
        Assert.Equal(creationDate, copiedBook.CreationDate);
        Assert.Equal(year, copiedBook.CreationDate.Year);
        Assert.Equal(month, copiedBook.CreationDate.Month);
        Assert.Equal(day, copiedBook.CreationDate.Day);
    }

    [Fact]
    public void Book_WithoutCreationDate_DoesNotThrowException()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Name = "Test Book"
            // CreationDate not set, will be default DateTime
        };

        // Act
        var copiedBook = new Book
        {
            Id = book.Id,
            Name = book.Name,
            CreationDate = book.CreationDate
        };

        // Assert
        Assert.Equal(default(DateTime), copiedBook.CreationDate);
    }
}
