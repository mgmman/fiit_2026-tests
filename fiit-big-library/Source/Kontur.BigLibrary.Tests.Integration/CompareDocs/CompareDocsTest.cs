using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Services.BookService;
using Kontur.BigLibrary.Service.Services.ImageService;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Kontur.BigLibrary.Tests.Integration.BookServiceTests;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Kontur.BigLibrary.Tests.Integration.CompareDocs;

[Parallelizable(ParallelScope.None)]
public class CompareDocsTest
{
    private static readonly IServiceProvider container = new ContainerForBdTests().Build();
    private static readonly IBookService bookService = container.GetRequiredService<IBookService>();
    private static readonly IImageService imageService = container.GetRequiredService<IImageService>();
    private int imageId;

    [OneTimeSetUp]
    public void SetUp()
    {
        var image = imageService
            .SaveAsync(new Image { Id = 1, Data = Array.Empty<byte>() }, CancellationToken.None)
            .GetAwaiter().GetResult();
        imageId = image.Id!.Value;
    }

    [Test]
    public async Task ShouldBeInOrder_WithLinq()
    {
        var name = "Ordered book";
        var bookBuilder = new BookBuilder().WithName(name);
        for (var i = 0; i < 5; i++)
        {
            var book = bookBuilder.WithId(i).WithAuthor("Author" + i).Build();
            await bookService.SaveBookAsync(book, CancellationToken.None);
        }
        
        var xmlResult =
            await bookService.ExportBooksToXmlAsync(CreateFilter(name), CancellationToken.None);
        var xDoc = XDocument.Parse(xmlResult);

        var titles = xDoc.Descendants("Book")
            .Elements("Author")
            .Select(t => t.Value)
            .ToList();
        
        titles.Should().ContainInOrder(new [] {0, 1, 2, 3, 4}.Reverse().Select(x => "Author" + x));
    }
    
    [Test]
    public async Task ShouldBeInOrder_WithRegexp()
    {
        var name = "Ordered book";
        var bookBuilder = new BookBuilder().WithName(name);
        for (var i = 0; i < 5; i++)
        {
            var book = bookBuilder.WithId(i).WithAuthor("Author" + i).Build();
            await bookService.SaveBookAsync(book, CancellationToken.None);
        }
        
        var xmlResult =
            await bookService.ExportBooksToXmlAsync(CreateFilter(name), CancellationToken.None);
        var regex = new Regex("<Author>(.*?)</Author>");
        var matches = regex.Matches(xmlResult);
        var titles = matches.Select(m => m.Groups[1].Value).ToList();
        
        titles.Should().HaveCount(5);
        titles.Should().ContainInOrder(new [] {0, 1, 2, 3, 4}.Reverse().Select(x => "Author" + x));
    }

    [Test] 
    public async Task ShouldBeInOrder_WithFullXmlComparison()
    {
        List<Book> books = new ();
        var name = "Ordered book";
        for (var i = 0; i < 5; i++)
        {
            var book = new BookBuilder()
                .WithName(name)
                .WithId(i)
                .WithAuthor("Author" + i)
                .WithImage(imageId)
                .Build();
            await bookService.SaveBookAsync(book, CancellationToken.None);
            books.Add(book);
        }
        books.Reverse();
        var exportTime = DateTime.Now;
        var xmlResult =
            await bookService.ExportBooksToXmlAsync(CreateFilter(name), CancellationToken.None);
        var xDoc = XDocument.Parse(xmlResult);
        
        var expDoc = new XDocument(
            new XElement("Books",
                new XElement("ExportTime", exportTime.ToString("yyyy-MM-dd HH:mm:ss")),
                books.Select(book => new XElement("Book",
                    new XElement("Title", book.Name),
                    new XElement("Author", book.Author),
                    new XElement("Description", book.Description),
                    new XElement("RubricId", book.RubricId),
                    new XElement("ImageId", book.ImageId.ToString()),
                    new XElement("Price", book.Price),
                    new XElement("IsBusy", "false")
                ))
            )
        );
        
        xDoc.Should().BeEquivalentTo(expDoc);
    }

    private BookFilter CreateFilter(string query = "", string rubric = "", int? limit = 10, bool isBusy = false,
        BookOrder order = BookOrder.ByLastAdding, int offset = 0)
    {
        return new()
        {
            Query = query,
            RubricSynonym = rubric,
            IsBusy = isBusy,
            Limit = limit,
            Order = order,
            Offset = offset
        };
    }
}