using RecettesIndex.Api.Data.Models;

namespace RecettesIndex.Api.Data;

public interface IAuthorRepository
{
    Task<Author?> GetAuthor(int id);
    Task<IEnumerable<Author>> GetAuthors();
    Task<Author?> Insert(Author author);
}