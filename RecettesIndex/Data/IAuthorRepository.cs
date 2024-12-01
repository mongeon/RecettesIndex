using RecettesIndex.Shared;

namespace RecettesIndex.Data;

public interface IAuthorRepository
{
    Task<Author[]> GetAuthors();
    // Task<Author?> Insert(Author book);
}
