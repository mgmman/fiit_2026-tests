using FluentAssertions;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Services.BookService.Repository;
using Kontur.BigLibrary.Tests.Core.Helpers.StringGenerator;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Controls;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Pages;
using Kontur.BigLibrary.Tests.UI.PW.PlaywrightCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kontur.BigLibrary.Tests.UI.PW.PlaywrightTests;

[NonParallelizable]
[WithAuth]
public class BookListPageTests : TestBase
{
    [SetUp]
    public void Setup()
    {
        var repository = ServiceProvider.GetRequiredService<IBookRepository>();
        repository.DeleteAllBooksAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Test]
    public async Task AddBook_SuccessTest()
    {
        var booksPage = await Navigation.GoToPageAsync<MainPage>();
        var book = await AddBook(booksPage);
        await booksPage.RefreshAsync();

        var bookViewPage = await booksPage.BookList.GetBookItem(book.Name).BookLink.ClickAndOpenPageAsync<BookPage>();

        await bookViewPage.BookName.CheckTextAsync(book.Name);
        await bookViewPage.BookAuthor.CheckTextAsync(book.Author);
        await bookViewPage.BookDescription.CheckTextAsync(book.Description);
        await bookViewPage.FreeState.WaitVisibleAsync();
    }

    // 1. Фильтрация по кнопке "только свободные". Добавить по умолчанию занятую книгу
    [Test]
    public async Task BookFilter_WhenOnlyAvailableSelected_ThenShowsOnlyFreeBooks()
    {
        var booksPage = await Navigation.GoToPageAsync<MainPage>();
        var book = await AddBook(booksPage);
        var bookTaken = await AddBook(booksPage);
        await booksPage.RefreshAsync();

        var bookToTakePage =
            await booksPage.BookList.GetBookItem(bookTaken.Name).BookLink.ClickAndOpenPageAsync<BookPage>();
        await bookToTakePage.CheckoutBook.ClickAsync();

        booksPage = await bookToTakePage.AllBooks.ClickAndOpenPageAsync<MainPage>();

        await booksPage.FreeOnlyFilter.ClickAsync();

        (await booksPage.BookList.CheckBookPresence(book.Name)).Should().BeTrue();
        (await booksPage.BookList.CheckBookPresence(bookTaken.Name)).Should().BeFalse();
    }

//  2. Переключение режима картинки/список
    [Test]
    public async Task ViewModeToggle_WhenSwitched_ChangesLayout()
    {
        var booksPage = await Navigation.GoToPageAsync<MainPage>();
        var book = await AddBook(booksPage);
        await booksPage.RefreshAsync();

        await booksPage.ChangeView.ClickAsync();
        var content = await booksPage.Page.ContentAsync();
        content.Should().ContainAll("Название", "Автор", "Рубрика", "Статус", "Цена", "<tr", "<th", "<td", "<thead",
            "<tbody", book.Name, book.Author);
    }

//  3. Переход в карточку книги - проверка информации
    [Test]
    public async Task BookDetails_WhenOpened_DisplaysValidInfo()
    {
        var booksPage = await Navigation.GoToPageAsync<MainPage>();
        var book = await AddBook(booksPage);
        await booksPage.RefreshAsync();

        var bookPage = await booksPage.BookList.GetBookItem(book.Name).BookLink.ClickAndOpenPageAsync<BookPage>();
        (await bookPage.BookName.GetTextAsync()).Should().Be(book.Name);
        (await bookPage.BookAuthor.GetTextAsync()).Should().Be(book.Author);
        (await bookPage.BookDescription.GetTextAsync()).Should().Be(book.Description);
    }

//  4. Проверка смены статусов книги свободна-занята-свободна
    [Test]
    public async Task BookStatusChange_WhenToggled_ThenUpdatesCorrectly()
    {
        var booksPage = await Navigation.GoToPageAsync<MainPage>();
        var book = await AddBook(booksPage);
        await booksPage.RefreshAsync();
        var bookObject = booksPage.BookList.GetBookItem(book.Name);
        (await booksPage.BookList.GetBookItem(book.Name).BookStatus.GetTextAsync()).Should().Be("Свободна");

        var bookPage = await bookObject.BookLink.ClickAndOpenPageAsync<BookPage>();
        await bookPage.CheckoutBook.ClickAsync();

        await CheckBookStatus(bookPage, book.Name, "Занята");

        bookPage = await bookObject.BookLink.ClickAndOpenPageAsync<BookPage>();
        await bookPage.ReturnBook.ClickAsync();

        await CheckBookStatus(bookPage, book.Name, "Свободна");
    }

    private static async Task CheckBookStatus(BookPage bookPage, string name, string state)
    {
        (await bookPage.FreeState.GetTextAsync()).Should().Be(state);
        var booksPage = await bookPage.AllBooks.ClickAndOpenPageAsync<MainPage>();
        (await booksPage.BookList.GetBookItem(name).BookStatus.GetTextAsync()).Should().Be(state);
    }

    private async Task<Book> AddBook(MainPage booksPage)
    {
        var bookName = StringGenerator.GetRandomString(10);
        var bookAuthor = StringGenerator.GetRandomString(10);
        var book = TestData.CreateBook(bookName, bookAuthor);

        var bookModal = await booksPage.AddBookButton.ClickAndOpenModalAsync<AddBookModal>();
        await bookModal.WaitVisibleAsync();

        await bookModal.NameInput.FillAsync(bookName);
        await bookModal.DescriptionInput.FillAsync(StringGenerator.GetRandomString(10));
        await bookModal.AuthorInput.FillAsync(bookAuthor);
        await bookModal.RubricDropdown.SelectByText("Администрирование");
        await bookModal.UploadImage.SetInputFilesAsync(TestData.ValidImagePath);
        await bookModal.AddBookSubmit.ClickAsync();
        await bookModal.WaitInvisibleAsync();
        return book;
    }
}