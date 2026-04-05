using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kontur.BigLibrary.DataAccess;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Exceptions;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Service.Services.EventService;
using Kontur.BigLibrary.Service.Services.EventService.Repository;
using Kontur.BigLibrary.Service.Services.ImageService;
using Kontur.BigLibrary.Service.Services.ImageService.Repository;
using Kontur.BigLibrary.Service.Services.SynonimMaker;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Kontur.BigLibrary.Tests.Integration.BookServiceTests;

[Parallelizable(ParallelScope.Children)]
public class BookServiceTest
{
    #region WithContainer

    private static readonly IServiceProvider container = new ContainerForBdTests().Build();
    private static readonly IBookService bookService = container.GetRequiredService<IBookService>();
    private static readonly IImageService imageService = container.GetRequiredService<IImageService>();
    private static readonly IBookRepository bookRepository = container.GetRequiredService<IBookRepository>();

    #endregion

    #region CreateWithConstructor

    private readonly IImageService imageServiceOld = new ImageService(
        new ImageRepository(new DbConnectionFactory(DbHelper.ConnectionString)),
        new ImageTransformer());

    private readonly IBookService bookServiceOld =
        new BookService(new BookRepository(new DbConnectionFactory(DbHelper.ConnectionString)),
            imageService,
            new EventService(new EventRepository(new DbConnectionFactory(DbHelper.ConnectionString))),
            new SynonymMaker());

    #endregion


    [Test]
    public async Task SaveBookAsync_ReturnSameBook_WhenSaveCorrectBook()
    {
        var imageForSave = new Image { Data = Array.Empty<byte>() };
        var image = await imageService.SaveAsync(imageForSave, new CancellationToken()).ConfigureAwait(false);
        
        var builder = container.GetRequiredService<BookBuilder>();
        
        var book = builder.WithImage(image.Id!.Value).Build();
        var result = await bookService.SaveBookAsync(book, CancellationToken.None);

        result.Name.Should().Be(book.Name);
        
        var savedBookId = result.Id;
        
        var savedBook = await bookRepository.GetBookAsync(savedBookId!.Value, CancellationToken.None);
        
        savedBook.Should().NotBeNull();
        savedBook.Should().BeEquivalentTo(book);
    }
    
    [Test]
    public async Task SaveBookAsync_ShouldNotSave_WhenRubrickDoesntExists()
    {
        var imageForSave = new Image { Data = Array.Empty<byte>() };
        var image = await imageService.SaveAsync(imageForSave, new CancellationToken()).ConfigureAwait(false);

        var id = -1;
        
        var builder = container.GetRequiredService<BookBuilder>();
        
        var book = builder.WithImage(image.Id!.Value)
            .WithRubricId(-1)
            .WithId(id)
            .Build();
        var action = async () => await bookService.SaveBookAsync(book, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Указана несуществующая рубрика.");
        
        var savedBook = await bookRepository.GetBookAsync(id, CancellationToken.None);
        
        savedBook.Should().BeNull();
    }
    
    [Test]
    public async Task SaveBookAsync_ShouldNotSave_WhenImageDoesntExists()
    {
        var id = -1;
        
        var builder = container.GetRequiredService<BookBuilder>();
        
        var book = builder.WithImage(-1)
            .WithId(id)
            .Build();
        var action = async () => await bookService.SaveBookAsync(book, CancellationToken.None);

        await action.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("Указана несуществующая картинка.");
        
        var savedBook = await bookRepository.GetBookAsync(id, CancellationToken.None);
        
        savedBook.Should().BeNull();
    }
}