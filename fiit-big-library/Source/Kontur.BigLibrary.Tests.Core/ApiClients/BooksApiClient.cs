using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Newtonsoft.Json;
using RestSharp;

namespace Kontur.BigLibrary.Tests.Core.ApiClients;

public class BooksApiClient : ApiClientBase
{
    public RestResponse AddBookToLibrary(Book book, string token)
    {
        var request = new RestRequest("api/books", Method.Post);

        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddJsonBody(new Book
        {
            Id = book.Id ?? IntGenerator.Get(),
            Name = book.Name,
            Description = book.Description,
            Author = book.Author,
            RubricId = book.RubricId,
            Count = 1,
            Price = book.Price,
            ImageId = book.ImageId
        });
        RestResponse response = client.ExecutePost(request);
        return response;
    }

    private static readonly object Lock = new object();
    
    public RestResponse CreateImage(string filePath, string token)
    {
        var request = new RestRequest("api/images", Method.Post);
        
        request.AddHeader("Authorization", $"Bearer {token}");

        var file = FileParameter.FromFile(filePath);
        using var memoryStream = new MemoryStream();
        lock (Lock)
        {
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            fileStream.CopyTo(memoryStream);
            fileStream.Close();
        }
        request.AddFile("File", () => memoryStream, file.Name);
        var response = client.ExecutePost(request);
        return response;
    }

    public RestResponse AddBookToLibraryUnauthorized(Book book)
    {
        var request = new RestRequest("api/books", Method.Post);

        request.AddHeader("Authorization", $"Bearer");
        request.AddJsonBody(new Book
        {
            Name = book.Name,
            Description = book.Description,
            Author = book.Author,
            RubricId = book.RubricId,
            Count = book.Count,
            Price = book.Price,
            ImageId = book.ImageId
        });
        RestResponse response = client.Post(request);
        return response;
    }

    public RestResponse GetAllBooksFromLibrary(string token)
    {
        var request = new RestRequest($"api/books/summary/select");

        request.AddHeader("Authorization", $"Bearer {token}");
        
        request.AddJsonBody(new BookFilter
        {
            Query = "", 
            RubricSynonym = "",
            Order = BookOrder.ByLastAdding
        });

        return client.ExecuteGet(request);
    }

    public RestResponse CheckoutBook(string bookId, string userName, string token)
    {
        var request = new RestRequest($"api/books/checkout");
        
        request.AddHeader("Authorization", $"Bearer {token}");

        request.AddParameter("bookId", bookId);
        request.AddParameter("userName", userName);
        
        return client.ExecuteGet(request);
    }
    
    public RestResponse ReturnBook(string bookId, string userName, string token)
    {
        var request = new RestRequest($"api/books/return");
        
        request.AddHeader("Authorization", $"Bearer {token}");

        request.AddParameter("bookId", bookId);
        request.AddParameter("userName", userName);
        
        return client.ExecuteGet(request);
    }

    public RestResponse EnqueueBook(string bookId, string userName, string token)
    {
        var request = new RestRequest("api/books/enqueue");
        request.AddHeader("Authorization", $"Bearer {token}");

        request.AddParameter("bookId", bookId);
        request.AddParameter("userName", userName);
        
        return client.ExecuteGet(request);
    }

    public RestResponse GetReadersInQueue(string bookId, string token)
    {
        var request = new RestRequest($"api/books/readersInQueue/{bookId}");
        
        request.AddHeader("Authorization", $"Bearer {token}");

        return client.ExecuteGet(request);
    }

    public ReaderInQueue[] GetReadersInQueueDeserialized(string bookId, string token)
    {
        var queue = GetReadersInQueue(bookId, token);
        return JsonConvert.DeserializeObject<ReaderInQueue[]>(queue.Content);
    }
    
    public RestResponse GetBookReaders(string bookId, string token)
    {
        var request = new RestRequest($"api/books/readers/{bookId}");
        
        request.AddHeader("Authorization", $"Bearer {token}");

        return client.ExecuteGet(request);
    } 
    
    public Reader[] GetBookReadersDeserialized(string bookId, string token)
    {
        var queue = GetBookReaders(bookId, token);
        return JsonConvert.DeserializeObject<Reader[]>(queue.Content);
    }
}