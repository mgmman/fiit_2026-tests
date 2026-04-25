using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kontur.BigLibrary.Service.Contracts;

namespace Kontur.BigLibrary.Service.Services.BookService.Repository
{
    public interface IBookRepository
    {
        Task<Book> GetBookAsync(int id, CancellationToken cancellation);
        Task<Book> GetBookBySynonymAsync(string synonym, CancellationToken cancellation);
        Task<BookSummary> GetBookSummaryBySynonymAsync(string synonym, CancellationToken cancellation);
        Task<int?> GetMaxBookIdAsync(CancellationToken cancellation);
        Task<IReadOnlyList<Book>> SelectBooksAsync(BookFilter filter, CancellationToken cancellation);
        Task<string> ExportBooksToXmlAsync(BookFilter filter, CancellationToken cancellation);
        Task<IReadOnlyList<BookSummary>> SelectBooksSummaryAsync(BookFilter filter, CancellationToken cancellation);
        Task<Book> SaveBookAsync(Book book, CancellationToken cancellation);
        Task SaveBookIndexAsync(int id, string ftsLexems, string synonym, CancellationToken cancellation);
        Task DeleteBookAsync(int id, CancellationToken cancellation);
        Task DeleteAllBooksAsync(CancellationToken cancellation);
        
        Task DeleteBookIndexAsync(int id, CancellationToken cancellation);
        Task SaveRubricIndexAsync(int id, string synonym, CancellationToken cancellation);
        Task<Reader> SaveReaderAsync(Reader reader, CancellationToken cancellation);
        Task<ReaderInQueue> SaveReaderInQueueAsync(ReaderInQueue reader, CancellationToken cancellation);
        Task<IReadOnlyList<Reader>> SelectReadersAsync(int bookId, CancellationToken cancellation);
        Task<IReadOnlyList<ReaderInQueue>> SelectReadersInQueueAsync(int bookId, CancellationToken cancellation);
        Task<int?> GetMaxReaderIdAsync(CancellationToken cancellation);
        Task<int?> GetMaxReaderInQueueIdAsync(CancellationToken cancellation);
        Task<Rubric> GetRubricAsync(int id, CancellationToken cancellation);
        Task<Rubric> GetRubricBySynonymAsync(string synonym, CancellationToken cancellation);
        Task<RubricSummary> GetRubricSummaryBySynonymAsync(string synonym, CancellationToken cancellation);
        Task<IReadOnlyList<Rubric>> SelectRubricsAsync(CancellationToken cancellation);
        Task<IReadOnlyList<RubricSummary>> SelectRubricsSummaryAsync(CancellationToken cancellation);
        Task<IReadOnlyList<RubricSummary>> SelectParentRubricsSummaryAsync(CancellationToken cancellation);
        Task<IReadOnlyList<RubricSummary>> SelectChildRubricsSummaryAsync(CancellationToken cancellation);
        Task<int> GetNextRubricIdAsync(CancellationToken cancellation);
        Task<Rubric> SaveRubricAsync(Rubric rubric, CancellationToken cancellation);
        Task<Librarian> GetLibrarianAsync(int id, CancellationToken cancellation);
        Task<IReadOnlyList<Librarian>> SelectLibrariansAsync(CancellationToken cancellation);
        Task<int> GetNextLibrarianIdAsync(CancellationToken cancellation);
        Task<Librarian> SaveLibrarianAsync(Librarian librarian, CancellationToken cancellation);
    }
}