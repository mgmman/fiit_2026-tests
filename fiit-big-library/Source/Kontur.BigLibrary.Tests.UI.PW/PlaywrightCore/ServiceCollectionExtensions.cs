using Kontur.BigLibrary.DataAccess;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Service.Services.EventService;
using Kontur.BigLibrary.Service.Services.EventService.Repository;
using Kontur.BigLibrary.Service.Services.ImageService;
using Kontur.BigLibrary.Service.Services.ImageService.Repository;
using Kontur.BigLibrary.Service.Services.SynonimMaker;
using Kontur.BigLibrary.Tests.Core.ApiClients;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Kontur.BigLibrary.Tests.Integration.BookServiceTests;
using Kontur.BigLibrary.Tests.UI.PW.Helpers;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Kontur.BigLibrary.Tests.UI.PW.PlaywrightCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlaywright(this IServiceCollection services)
        => services.AddSingleton<IPlaywrightGetter, PlaywrightSingleton>()
            .AddScoped<IBrowserGetter, ChromiumGetter>()
            .AddSingleton<IAuthContextProvider, AuthContextProvider>()
            .AddScoped<IBrowserContextGetter, BrowserContextGetter>()
            .AddScoped<IPlaywrightPageGetter, PlaywrightPageGetter>()
            .AddScoped<IPageFactory, PageFactory>()
            .AddScoped<IControlFactory, SimpleControlFactory>()
            .AddScoped<IDependenciesFactory, DependencyFactory>()
            .AddScoped<Navigation>();

    public static IServiceCollection AddTestDataHelpers(this IServiceCollection services)
        => services.AddSingleton<AuthApiClient>()
            .AddSingleton<BooksApiClient>()
            .AddSingleton<BooksTestDataService>();

    public static IServiceCollection AddDBServices(this IServiceCollection services)
    {
        services.AddTransient<BookBuilder>();
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new DbConnectionFactory(
                "Data Source=../../../../../Build/Kontur.BigLibrary.Service/biglibrary.db;Mode=ReadWriteCreate;Cache=Shared"));
        services.AddSingleton<ISynonymMaker, SynonymMaker>();
        services.AddSingleton<IBookRepository, BookRepository>();
        services.AddSingleton<IBookService, BookService>();
        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<IImageTransformer, ImageTransformer>();
        services.AddSingleton<IImageRepository, ImageRepository>();
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IEventService, EventService>();

        return services;
    }
}