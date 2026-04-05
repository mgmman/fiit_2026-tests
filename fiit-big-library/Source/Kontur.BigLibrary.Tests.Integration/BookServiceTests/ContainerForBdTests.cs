using System;
using Kontur.BigLibrary.DataAccess;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Service.Services.EventService;
using Kontur.BigLibrary.Service.Services.EventService.Repository;
using Kontur.BigLibrary.Service.Services.ImageService;
using Kontur.BigLibrary.Service.Services.ImageService.Repository;
using Kontur.BigLibrary.Service.Services.SynonimMaker;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Kontur.BigLibrary.Tests.Integration.BookServiceTests;

public class ContainerForBdTests
{
    public IServiceCollection _collection;

    public ContainerForBdTests()
    {
        _collection = new ServiceCollection();

        _collection.AddTransient<BookBuilder>();

        _collection.AddSingleton<IDbConnectionFactory>(x => new DbConnectionFactory(DbHelper.ConnectionString));

        _collection.AddSingleton<ISynonymMaker, SynonymMaker>();
        
        _collection.AddSingleton<IBookRepository, BookRepository>();
        _collection.AddSingleton<IBookService, BookService>();

        _collection.AddSingleton<IImageService, ImageService>();
        _collection.AddSingleton<IImageTransformer, ImageTransformer>();
        _collection.AddSingleton<IImageRepository, ImageRepository>();

        _collection.AddSingleton<IEventRepository, EventRepository>();
        _collection.AddSingleton<IEventService, EventService>();
    }

    public IServiceProvider Build()
    {
        return _collection.BuildServiceProvider();
    }
}