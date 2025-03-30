namespace RecettesIndex.Api.Data.Converter;

public static class AuthorConverter
{
    public static Models.Author Convert(this Shared.Author author)
    {
        return new Models.Author
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            CreatedAt = author.CreatedAt
        };
    }
    public static Shared.Author Convert(this Models.Author author)
    {
        return new Shared.Author
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            CreatedAt = author.CreatedAt
        };
    }
}
