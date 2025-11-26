using RecettesIndex.Models;
using System.ComponentModel.DataAnnotations;

namespace RecettesIndex.Tests.Models;

public class StoreModelTests
{
    [Fact]
    public void Store_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var expectedName = "Le Gourmet Express";
        var expectedAddress = "123 Main St, Montreal, QC";
        var expectedPhone = "514-555-1234";
        var expectedWebsite = "https://www.legourmet.com";
        var expectedNotes = "Great prepared meals";
        var expectedCreationDate = DateTime.Now;

        // Act
        var store = new Store
        {
            Id = 1,
            Name = expectedName,
            Address = expectedAddress,
            Phone = expectedPhone,
            Website = expectedWebsite,
            Notes = expectedNotes,
            CreationDate = expectedCreationDate
        };

        // Assert
        Assert.Equal(1, store.Id);
        Assert.Equal(expectedName, store.Name);
        Assert.Equal(expectedAddress, store.Address);
        Assert.Equal(expectedPhone, store.Phone);
        Assert.Equal(expectedWebsite, store.Website);
        Assert.Equal(expectedNotes, store.Notes);
        Assert.Equal(expectedCreationDate, store.CreationDate);
    }

    [Fact]
    public void Store_DefaultValues_AreSetCorrectly()
    {
        // Act
        var store = new Store();

        // Assert
        Assert.Equal(string.Empty, store.Name);
        Assert.Null(store.Address);
        Assert.Null(store.Phone);
        Assert.Null(store.Website);
        Assert.Null(store.Notes);
    }

    [Theory]
    [InlineData("https://www.example.com")]
    [InlineData("http://example.com")]
    [InlineData("https://sub.example.com/path")]
    [InlineData("https://example.com:8080/page")]
    public void Store_Website_AcceptsValidUrls(string url)
    {
        // Arrange
        var store = new Store { Name = "Test Store", Website = url };
        var context = new ValidationContext(store);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(store, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Equal(url, store.Website);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Store_Name_RequiredValidation_Fails(string name)
    {
        // Arrange
        var store = new Store { Name = name };
        var context = new ValidationContext(store);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(store, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Store.Name)));
    }

    [Fact]
    public void Store_Name_ValidValue_Passes()
    {
        // Arrange
        var store = new Store { Name = "Le Gourmet Express" };
        var context = new ValidationContext(store);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(store, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Le Gourmet", "123 Main St", "514-555-1234")]
    [InlineData("Restaurant Chez Pierre", null, null)]
    [InlineData("Quick Meals", "456 Oak Ave", null)]
    public void Store_OptionalFields_AcceptNullValues(string name, string? address, string? phone)
    {
        // Arrange
        var store = new Store
        {
            Name = name,
            Address = address,
            Phone = phone
        };

        // Act & Assert
        Assert.Equal(name, store.Name);
        Assert.Equal(address, store.Address);
        Assert.Equal(phone, store.Phone);
    }

    [Fact]
    public void Store_MaxLength_Validation()
    {
        // Arrange
        var longName = new string('A', 256); // Exceeds 255 char limit
        var store = new Store { Name = longName };
        var context = new ValidationContext(store);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(store, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Store.Name)));
    }

    [Theory]
    [InlineData("514-555-1234")]
    [InlineData("(514) 555-1234")]
    [InlineData("+1-514-555-1234")]
    [InlineData("5145551234")]
    public void Store_Phone_AcceptsVariousFormats(string phone)
    {
        // Arrange
        var store = new Store { Name = "Test Store", Phone = phone };

        // Act & Assert
        Assert.Equal(phone, store.Phone);
    }

    [Fact]
    public void Store_Notes_AcceptsLongText()
    {
        // Arrange
        var longNotes = new string('X', 1000);
        var store = new Store { Name = "Test Store", Notes = longNotes };

        // Act & Assert
        Assert.Equal(longNotes, store.Notes);
    }
}
