namespace RecettesIndex.Api.Data.Converter;

public static class AuthorConverter
{
    public static Models.Author Convert(this Shared.Author author)
    {
        return new Models.Author
        {
            Id = author.Id,
            Name = author.Name,
            CreatedAt = author.CreatedAt
        };
    }
    public static Shared.Author? Convert(this Models.Author author)
    {
        if (author == null)
        {
            return null;
        }

        return new Shared.Author
        {
            Id = author.Id,
            Name = author.Name,
            CreatedAt = author.CreatedAt
        };
    }
}
