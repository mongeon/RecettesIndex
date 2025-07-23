using RecettesIndex.Models;
using Supabase;

namespace RecettesIndex.Services
{
    public class BookAuthorService
    {
        private readonly Client _supabaseClient;

        public BookAuthorService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        /// <summary>
        /// Creates associations between a book and multiple authors (for new books)
        /// </summary>
        public async Task CreateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> authors)
        {
            if (!authors.Any()) return;

            try
            {
                var bookAuthors = authors.Select(author => new BookAuthor
                {
                    BookId = bookId,
                    AuthorId = author.Id
                }).ToList();

                await _supabaseClient.From<BookAuthor>().Insert(bookAuthors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating book-author associations: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates associations between a book and authors (for existing books)
        /// Only modifies what has changed for better performance
        /// </summary>
        public async Task UpdateBookAuthorAssociationsAsync(int bookId, IEnumerable<Author> newAuthors)
        {
            try
            {
                // Get current associations
                var currentAssociationsResponse = await _supabaseClient.From<BookAuthor>()
                    .Where(x => x.BookId == bookId)
                    .Get();

                var currentAuthorIds = currentAssociationsResponse.Models?.Select(x => x.AuthorId).ToHashSet() ?? new HashSet<int>();
                var newAuthorIds = newAuthors.Select(x => x.Id).ToHashSet();

                // Find authors to remove and add
                var authorsToRemove = currentAuthorIds.Except(newAuthorIds).ToList();
                var authorsToAdd = newAuthorIds.Except(currentAuthorIds).ToList();

                // Remove associations that are no longer needed
                foreach (var authorIdToRemove in authorsToRemove)
                {
                    await _supabaseClient.From<BookAuthor>()
                        .Where(x => x.BookId == bookId && x.AuthorId == authorIdToRemove)
                        .Delete();
                }

                // Add new associations
                if (authorsToAdd.Any())
                {
                    var bookAuthorsToAdd = authorsToAdd.Select(authorId => new BookAuthor
                    {
                        BookId = bookId,
                        AuthorId = authorId
                    }).ToList();

                    await _supabaseClient.From<BookAuthor>().Insert(bookAuthorsToAdd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book-author associations: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads authors for a specific book using the junction table
        /// </summary>
        public async Task LoadAuthorsForBookAsync(Book book)
        {
            try
            {
                // Get book-author associations
                var bookAuthorsResponse = await _supabaseClient.From<BookAuthor>()
                    .Where(x => x.BookId == book.Id)
                    .Get();

                if (bookAuthorsResponse.Models?.Any() == true)
                {
                    var authorIds = bookAuthorsResponse.Models.Select(ba => ba.AuthorId).ToList();

                    // Get the actual author objects
                    var authorsResponse = await _supabaseClient.From<Author>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.In, authorIds)
                        .Get();

                    book.Authors = authorsResponse.Models ?? new List<Author>();
                }
                else
                {
                    book.Authors = new List<Author>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading authors for book {book.Id}: {ex.Message}");
                book.Authors = new List<Author>();
            }
        }
    }
}
