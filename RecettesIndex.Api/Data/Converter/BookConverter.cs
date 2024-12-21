namespace RecettesIndex.Api.Data.Converter;

public static class BookConverter
{
    public static Models.Book Convert(this Shared.Book book)
    {
        return new Models.Book
        {
            Id = book.Id,
            Title = book.Title,
            Authors = book.Authors.Select(x => x.Convert()).ToList(),
            CreatedAt = book.CreatedAt
        };
    }
    public static Shared.Book? Convert(this Models.Book book)
    {
        if (book == null)
        {
            return null;
        }

        return new Shared.Book
        {
            Id = book.Id,
            Title = book.Title,
            Authors = book.Authors.Select(x => x.Convert()).ToList(),
            CreatedAt = book.CreatedAt
        };
    }
}
