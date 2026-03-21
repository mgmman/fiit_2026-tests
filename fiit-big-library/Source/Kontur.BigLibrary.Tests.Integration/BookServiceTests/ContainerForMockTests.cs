using System;
using System.Threading;
using System.Threading.Tasks;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Service.Services.EventService;
using Kontur.BigLibrary.Service.Services.EventService.Repository;
using Kontur.BigLibrary.Service.Services.ImageService;
using Kontur.BigLibrary.Service.Services.ImageService.Repository;
using Kontur.BigLibrary.Service.Services.SynonimMaker;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Kontur.BigLibrary.Tests.Integration.BookServiceTests;

public class ContainerForMockTests
{
    private IServiceCollection _collection;

    public ContainerForMockTests()
    {
        _collection = new ServiceCollection();
        var imageRepository = Substitute.For<IImageRepository>();
        imageRepository.GetAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Image { Data = Array.Empty<byte>() }));
        var bookRepository = Substitute.For<IBookRepository>();
        bookRepository.GetRubricAsync(1,  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Rubric()));
        bookRepository.SaveBookAsync(Arg.Any<Book>(), Arg.Any<CancellationToken>())
            .Returns(x => Task.FromResult((Book)x[0]));
        _collection.AddSingleton(imageRepository);
        _collection.AddSingleton(Substitute.For<IEventRepository>());
        _collection.AddSingleton(bookRepository);

        _collection.AddSingleton<ISynonymMaker, SynonymMaker>();
        _collection.AddSingleton<IBookService, BookService>();
        _collection.AddSingleton<IImageService, ImageService>();
        _collection.AddSingleton<IImageTransformer, ImageTransformer>();
        _collection.AddSingleton<IEventService, EventService>();

        _collection.AddTransient<BookBuilder>();
    }

    public IServiceProvider Build()
    {
        return _collection.BuildServiceProvider();
    }
}