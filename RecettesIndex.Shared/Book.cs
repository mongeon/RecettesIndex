namespace RecettesIndex.Shared;

public record Book
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required List<Author> Authors { get; set; }

    public DateTime CreatedAt { get; set; }
}