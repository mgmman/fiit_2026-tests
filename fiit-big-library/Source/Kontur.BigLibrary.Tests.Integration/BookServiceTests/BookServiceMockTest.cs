using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Events;
using Kontur.BigLibrary.Service.Exceptions;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Service.Services.EventService.Repository;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace Kontur.BigLibrary.Tests.Integration.BookServiceTests;

[Parallelizable(ParallelScope.Children)]
public class BookServiceMockTest
{
    #region WithContainer

    private static readonly IServiceProvider container = new ContainerForMockTests().Build();
    private static readonly IBookService bookService = container.GetRequiredService<IBookService>();
    private static readonly IBookRepository bookRepository = container.GetRequiredService<IBookRepository>();
    private static readonly IEventRepository eventRepository = container.GetRequiredService<IEventRepository>();

    #endregion


    [Test]
    public async Task SaveBookAsync_ReturnSameBook_WhenSaveCorrectBook()
    {
        var book = new Book
        {
            Name = "Database Systems. The Complete Book",
            Author = "Hector Garcia-Molina, Jeffrey D.Ullman, Jennifer Widom",
            RubricId = 1,
            ImageId = 1,
            Description = "New_book"
        };
        var result = await bookService.SaveBookAsync(book, CancellationToken.None);

        result.Name.Should().Be(book.Name);
        await bookRepository.Received(Quantity.Exactly(1)).SaveBookAsync(book, CancellationToken.None);
        await eventRepository.ReceivedWithAnyArgs(1).SaveAsync(new ChangedEvent(),  CancellationToken.None);
    }
    
    [Test]
    public async Task SaveBookAsync_ShouldNotSave_WhenRubrickDoesntExists()
    {
        var id = -1;
        
        var book = new Book
        {
            Id = id,
            Name = "Database Systems. The Complete Book",
            Author = "Hector Garcia-Molina, Jeffrey D.Ullman, Jennifer Widom",
            RubricId = -1,
            ImageId = 1,
            Description = "New_book"
        };
        var action = async () => await bookService.SaveBookAsync(book, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Указана несуществующая рубрика.");
        
        var savedBook = await bookRepository.GetBookAsync(id, CancellationToken.None);
        
        savedBook.Should().BeNull();
        await bookRepository.DidNotReceive().SaveBookAsync(book, CancellationToken.None);
    }
    
    [Test]
    public async Task SaveBookAsync_ShouldNotSave_WhenImageDoesntExists()
    {
        var id = -1;
        
        var book = new Book
        {
            Id = id,
            Name = "Database Systems. The Complete Book",
            Author = "Hector Garcia-Molina, Jeffrey D.Ullman, Jennifer Widom",
            RubricId = 1,
            ImageId = -1,
            Description = "New_book"
        };
        var action = async () => await bookService.SaveBookAsync(book, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Указана несуществующая картинка.");
        
        var savedBook = await bookRepository.GetBookAsync(id, CancellationToken.None);
        
        savedBook.Should().BeNull();
        await bookRepository.DidNotReceive().SaveBookAsync(book, CancellationToken.None);
    }
}