namespace RecettesIndex.Shared;

public record Book
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required Author Author { get; set; }

    public DateTime CreatedAt { get; set; }
}