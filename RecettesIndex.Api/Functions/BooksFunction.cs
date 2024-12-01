using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;
using RecettesIndex.Api.Data.Converter;

namespace RecettesIndex.Api.Functions;

public class BooksFunction(ILogger<BooksFunction> logger, IBookRepository bookRepository)
{
    private readonly ILogger<BooksFunction> _logger = logger;

    [Function("GetAllBooks")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "books")] HttpRequestData req)
    {
        _logger.LogInformation("Get All Books");
        var books = await bookRepository.GetBooks();
        Shared.Book[] booksDTO = books.Select(r => r.Convert()!).ToArray();
        _logger.LogInformation("Books found: {Count}", booksDTO.Length);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(booksDTO);

        return response;
    }

    [Function("GetBook")]
    public async Task<HttpResponseData> RunGetBook([HttpTrigger(AuthorizationLevel.Function, "get", Route = "books/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Get Book {Id}", id);
        var book = await bookRepository.GetBook(id);

        Shared.Book? bookDTO = book?.Convert();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(bookDTO);

        return response;
    }

    [Function("GetBooksByAuthor")]
    public async Task<HttpResponseData> RunGetBooksByAuthor([HttpTrigger(AuthorizationLevel.Function, "get", Route = "books/author/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Get Books by Author {AuthorId}", id);
        var books = await bookRepository.GetBooksByAuthor(id);
        Shared.Book[] booksDTO = books.Select(r => r.Convert()!).ToArray();
        _logger.LogInformation("Books found: {Count}", booksDTO.Length);
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(booksDTO);
        return response;
    }
}
