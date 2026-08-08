using Newtonsoft.Json;
using RecettesIndex.Models;
using Xunit;

namespace RecettesIndex.Tests.Models;

/// <summary>
/// The rating column is nullable in the database, and one NULL used to take down every
/// list at once: the whole response is deserialised in one go, so a single unrated recipe
/// threw for all of them.
/// </summary>
public class RecipeRatingTests
{
    [Fact]
    public void DeserializingANullRating_ReadsAsNotRated()
    {
        // Ce que produit une insertion SQL qui ne renseigne pas la colonne.
        var json = """{"id":1,"name":"Crème brûlée","rating":null}""";

        var recipe = JsonConvert.DeserializeObject<Recipe>(json);

        Assert.NotNull(recipe);
        Assert.Equal(0, recipe.Rating);
    }

    [Fact]
    public void DeserializingAnAbsentRating_ReadsAsNotRated()
    {
        var json = """{"id":1,"name":"Crème brûlée"}""";

        var recipe = JsonConvert.DeserializeObject<Recipe>(json);

        Assert.NotNull(recipe);
        Assert.Equal(0, recipe.Rating);
    }

    [Fact]
    public void DeserializingARating_KeepsIt()
    {
        var json = """{"id":1,"name":"Crème brûlée","rating":4}""";

        var recipe = JsonConvert.DeserializeObject<Recipe>(json);

        Assert.NotNull(recipe);
        Assert.Equal(4, recipe.Rating);
    }

    [Fact]
    public void SettingTheRating_WritesANumberBack()
    {
        // La lecture tolère l'absence, l'écriture ne la reproduit pas.
        var recipe = new Recipe { Name = "Dal", Rating = 5 };

        Assert.Equal(5, recipe.RatingValue);
    }

    [Fact]
    public void SettingZero_IsStoredAsZeroNotAsAbsent()
    {
        var recipe = new Recipe { Name = "Dal", Rating = 0 };

        Assert.Equal(0, recipe.RatingValue);
    }
}
