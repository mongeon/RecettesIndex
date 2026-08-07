using RecettesIndex.Services;
using Xunit;

namespace RecettesIndex.Tests.Services;

public class SearchTextTests
{
    [Theory]
    [InlineData("Crème brûlée", "creme brulee")]
    [InlineData("TARTE", "tarte")]
    [InlineData("  Pâté   chinois  ", "pate chinois")]
    [InlineData("Æquo", "æquo")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void Normalize_FoldsCaseAccentsAndSpacing(string? input, string expected)
    {
        Assert.Equal(expected, SearchText.Normalize(input));
    }

    [Fact]
    public void Normalize_MakesAccentedAndUnaccentedQueriesEqual()
    {
        // Le cas qui motive la normalisation : personne ne tape les accents au clavier
        // d'un téléphone, et la recherche doit quand même aboutir.
        Assert.Equal(SearchText.Normalize("creme"), SearchText.Normalize("crème"));
    }

    [Theory]
    [InlineData("chat", "chat", 0)]
    [InlineData("chat", "chats", 1)]
    [InlineData("tarte", "tartes", 1)]
    [InlineData("tarte", "trate", 2)]
    [InlineData("", "abc", 3)]
    [InlineData("abc", "", 3)]
    public void Levenshtein_MeasuresEditDistance(string a, string b, int expected)
    {
        Assert.Equal(expected, SearchText.Levenshtein(a, b));
    }

    [Fact]
    public void Levenshtein_StopsCountingPastTheCap()
    {
        // Le plafond n'est pas une optimisation gratuite : sans lui, chaque frappe
        // compare intégralement des mots longs sans rapport.
        var capped = SearchText.Levenshtein("bouillabaisse", "gnocchis", 2);

        Assert.True(capped > 2);
    }

    [Theory]
    [InlineData("tarte au sucre", "tarte")]      // sous-chaîne exacte
    [InlineData("tarte au sucre", "sucre")]      // mot au milieu
    [InlineData("tarte au sucre", "tartes")]     // une faute, mot de plus de 4 lettres
    [InlineData("gnocchis a la sauge", "gnochis")]
    public void ContainsTerm_AcceptsExactAndNearMisses(string haystack, string term)
    {
        Assert.True(SearchText.ContainsTerm(haystack, term));
    }

    [Theory]
    [InlineData("tarte au sucre", "poulet")]
    [InlineData("", "tarte")]
    public void ContainsTerm_RejectsUnrelatedTerms(string haystack, string term)
    {
        Assert.False(SearchText.ContainsTerm(haystack, term));
    }

    [Fact]
    public void ContainsTerm_DoesNotFuzzyMatchShortTerms()
    {
        // « pat » ne doit pas ramener « pain » : à trois lettres, une tolérance de deux
        // rapproche presque tous les mots les uns des autres.
        Assert.False(SearchText.ContainsTerm("pain complet", "pat"));
    }

    [Fact]
    public void ContainsTerm_EmptyTermMatchesAnything()
    {
        Assert.True(SearchText.ContainsTerm("tarte", ""));
    }
}
