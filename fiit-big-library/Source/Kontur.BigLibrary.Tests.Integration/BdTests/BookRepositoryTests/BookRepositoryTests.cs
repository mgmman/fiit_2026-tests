using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Kontur.BigLibrary.Tests.Integration.BdTests.BookRepositoryTests;

[Parallelizable(ParallelScope.All)]
public class BookRepositoryTests
{
    private readonly IBookRepository bookRepository;

    #region CleanUpAfterEveryTest
    
    private ConcurrentBag<int> bookIds = new();
    
    [TearDown]
    public async Task Teardown()
    {
        foreach (var id in bookIds)
        {
            await bookRepository.DeleteBookAsync(id, CancellationToken.None);
            await bookRepository.DeleteBookIndexAsync(id, CancellationToken.None);
        }
    }
    
    #endregion


    public BookRepositoryTests()
    {
        var container = new Container().Build();
        bookRepository = container.GetRequiredService<IBookRepository>();
    }

    [Test]
    public async Task GetBook_Exists_ReturnBook()
    {
        var expectedBook = new BookBuilder().Build();
        await SaveBook(expectedBook);

        var actualBook = await bookRepository.GetBookAsync(expectedBook.Id.Value, CancellationToken.None);

        actualBook.Should().BeEquivalentTo(expectedBook);
        
        bookIds.Add(expectedBook.Id.Value);
    }
    
    [Test]
    public async Task GetBook_WithLongDescription_ReturnBook()
    {
        var expectedBook = new BookBuilder().WithDescription(longDescription).Build();
        await SaveBook(expectedBook);

        var actualBook = await bookRepository.GetBookAsync(expectedBook.Id.Value, CancellationToken.None);

        actualBook.Should().BeEquivalentTo(expectedBook);
        
        bookIds.Add(expectedBook.Id.Value);
    }
    
    [Test]
    public async Task GetNonexistentBook_ReturnsNull()
    {
        var nonexistentBook = await bookRepository.GetBookAsync(-1, CancellationToken.None);

        nonexistentBook.Should().BeNull();
    }
    
    [Test]
    public async Task SelectBook_ByName_ReturnsOnlyMatchedBook()
    {
        var expectedBook = new BookBuilder().WithName("BookWeSearchFor").Build();
        await bookRepository.SaveBookAsync(expectedBook, CancellationToken.None);
        var anotherBook = new BookBuilder().Build();
        await SaveBook(expectedBook);
        await SaveBook(anotherBook);

        var filter = DbHelper.CreateFilter(expectedBook.Name);

        var foundBooks = await bookRepository.SelectBooksAsync(filter, CancellationToken.None);
        
        foundBooks.Single().Should().BeEquivalentTo(expectedBook);
        
        bookIds.Add(expectedBook.Id.Value);
    }
    
    [Test]
    public async Task SelectBook_ThatWasDeleted_ReturnsNoBook()
    {
        var expectedBook = new BookBuilder().Delete().WithName("DeletedBook").Build();
        await SaveBook(expectedBook);

        var filter = DbHelper.CreateFilter(expectedBook.Name);

        var foundBooks = await bookRepository.SelectBooksAsync(filter, CancellationToken.None);
        
        foundBooks.Should().BeEmpty();
        
        bookIds.Add(expectedBook.Id.Value);
    }
    
    [Test]
    public async Task SelectBook_WithOrderByAndOffset()
    {
        var book = new BookBuilder().WithName("Book").Build();
        var book1 = new BookBuilder().WithId(book.Id.Value + 1).WithName("Book").Build();
        var book2 = new BookBuilder().WithId(book.Id.Value + 2).WithName("Book").Build();
        await SaveBook(book);
        await SaveBook(book1);
        await SaveBook(book2);

        var filter = DbHelper.CreateFilter(book.Name, offset:1, order: BookOrder.ByLastAdding);

        var foundBooks = await bookRepository.SelectBooksAsync(filter, CancellationToken.None);
        
        foundBooks.Count.Should().Be(2);
        foundBooks[0].Should().BeEquivalentTo(book1);
        foundBooks[1].Should().BeEquivalentTo(book);
        
        bookIds.Add(book.Id.Value);
        bookIds.Add(book1.Id.Value);
        bookIds.Add(book2.Id.Value);
    }

    [Test]
    public async Task GetBookSummaryBySynonymAsync_ShouldReturn_SameAsSelectBooksSummaryAsync()
    {
        var expectedBook = new BookBuilder().WithName("BookWithSummary").Build();
        var synonym = await SaveBook(expectedBook);
        
        var filter = DbHelper.CreateFilter(expectedBook.Name);

        var summaryBySynonym = await bookRepository.GetBookSummaryBySynonymAsync(synonym, CancellationToken.None);
        var summaryByBook = await bookRepository.SelectBooksSummaryAsync(filter, CancellationToken.None);
        
        summaryBySynonym.Should().BeEquivalentTo(summaryByBook.Single());
        
        bookIds.Add(expectedBook.Id.Value);
    }
    
    [Test]
    public async Task GetBookSummaryBySynonymAsync_ShouldReturnBusy_IfBookWasTakenByReader()
    {
        var expectedBook = new BookBuilder().WithName("BookWithReader").Build();
        var synonym = await SaveBook(expectedBook);

        var reader = new ReaderBuilder().WithBook(expectedBook.Id.Value).Build();

        await bookRepository.SaveReaderAsync(reader, CancellationToken.None);
        
        var summaryBySynonym = await bookRepository.GetBookSummaryBySynonymAsync(synonym, CancellationToken.None);

        summaryBySynonym.IsBusy.Should().BeTrue();
        
        bookIds.Add(expectedBook.Id.Value);
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task GetMaxBookId_Exists_ReturnBooksCount()
    {
        await CleanBooks();
        var expectedBook = new BookBuilder().WithId(1).Build();
        await SaveBook(expectedBook);

        var maxId = await bookRepository.GetMaxBookIdAsync(CancellationToken.None);

        maxId.Should().Be(expectedBook.Id);
    }

    public async Task CleanBooks()
    {
        var books = await bookRepository.SelectBooksAsync(new BookFilter(), CancellationToken.None);
        foreach (var book in books)
        {
            await bookRepository.DeleteBookAsync(book.Id!.Value, CancellationToken.None);
            await bookRepository.DeleteBookIndexAsync(book.Id!.Value, CancellationToken.None);
        }
    }
    
    private async Task<string> SaveBook(Book expectedBook)
    {
        var synonym = $"book {expectedBook.Id}";
        
        await bookRepository.SaveBookAsync(expectedBook, CancellationToken.None);
        
        await bookRepository.SaveBookIndexAsync(expectedBook.Id!.Value, expectedBook.GetTextForFts(),
            synonym, CancellationToken.None);
        return synonym;
    }

    private const string longDescription =
        "Some very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "very very very very very very very very very very very very very very very very very very very very" +
        "long description";
}