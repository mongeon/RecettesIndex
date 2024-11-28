using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RecettesIndex.Api.Data;

namespace RecettesIndex.Api.Functions;

public class BooksFunction(ILogger<BooksFunction> logger, IBookRepository bookRepository)
{
    private readonly ILogger<BooksFunction> _logger = logger;

    [Function("GetAllBooks")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "books")] HttpRequestData req)
    {
        var books = await bookRepository.GetBooks();
        Shared.Book[] booksDTO = books.Select(r => new Shared.Book
        {
            Id = r.Id,
            Name = r.Name,
            CreatedAt = r.CreatedAt,
        }).ToArray();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

        await response.WriteAsJsonAsync(booksDTO);

        return response;
    }
}
