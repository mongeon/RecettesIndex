namespace RecettesIndex.Shared;

public record Recette
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public int? BookId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Book? Book { get; set; }
    public int? Page { get; set; }
}