using FluentAssertions;
using Kontur.BigLibrary.Tests.Core.Helpers.StringGenerator;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Pages;
using Microsoft.Playwright;

namespace Kontur.BigLibrary.Tests.UI.PW.PlaywrightTests;

public class LocatorTests : TestBase
{
    private string email;
    private string password;
    private const string firstBookSelector = "(//*[@data-tid='book-link'])[1]";
    
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        email = StringGenerator.GetEmail();
        password = StringGenerator.GetValidPassword();
        var page = await Navigation.GoToPageAsync<RegisterPage>();
        await page.EmailInput.FillAsync(email);
        await page.PasswordInput.FillAsync(password);
        await page.PasswordConfirmationInput.FillAsync(password);
        await page.SubmitButton.ClickAndOpenPageAsync<MainPage>();
    }

    [Test]
    public async Task TestLocators_BooksList_SearchBar()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var searchBar =  page.Locator("[data-tid='search-input'] input");
        await searchBar.FillAsync("Название книги");
        
        await searchBar.ClearAsync();
        playwright.Dispose();
    }
    
    [Test]
    public async Task TestLocators_BooksList_SecondBook()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var secondBook = page.Locator("(//*[@data-tid='book-link'])[2]");
        await secondBook.ClickAsync();
        page.Url.Should().Contain("/books/");
        await page.GoBackAsync();
        playwright.Dispose();
    }
    
    [Test]
    public async Task TestLocators_BooksList_FreeOnlyToggle()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var onlyFreeBooks = page.Locator("label:has(input[type=checkbox])");
        await onlyFreeBooks.SetCheckedAsync(true);
        (await onlyFreeBooks.IsCheckedAsync()).Should().BeTrue();
        playwright.Dispose();
    }
    
    [Test]
    public async Task TestLocators_BookPage_AllBooksLink()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var firstBook = page.Locator(firstBookSelector);
        await firstBook.ClickAsync();
        var allBooksLink = page.GetByRole(AriaRole.Link, new () { Name = "Все книги" });
        await allBooksLink.ClickAsync(new LocatorClickOptions
        {
            Force = true
        });
        page.Url.Should().Be("http://localhost:5000/");
        playwright.Dispose();
    }
    
    [Test]
    public async Task TestLocators_BookPage_TakeBook()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var firstBook = page.Locator(firstBookSelector);
        await firstBook.ClickAsync();
        var takeBookButton = page.GetByRole(AriaRole.Button, new () { Name = "Взять книгу" });
        await takeBookButton.ClickAsync(new LocatorClickOptions
        {
            Force = true
        });
        playwright.Dispose();
    }
    
    [Test]
    public async Task TestLocators_CreateBookPage()
    {
        var (page, playwright) = await CreatePage();
        await SignIn(page);
        
        var createBookButton = page.Locator("[data-tid='book-add']");
        await createBookButton.ClickAsync();
        var title = page.GetByText("Добавить книгу");
        var bookNameInput = page.GetByLabel("Название книги");
        bookNameInput.FillAsync("Название книги");
        var submitButton = page.GetByRole(AriaRole.Button, new() { Name = "Добавить" });
        await submitButton.ClickAsync();
        
        playwright.Dispose();
    }

    private async Task<(IPage page, IPlaywright playwright)> CreatePage()
    {
        var playwright = await Playwright.CreateAsync();
        var launchOptions = new BrowserTypeLaunchOptions { Headless = false };
        var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        return (page, playwright);
    }
    
    private async Task SignIn(IPage page)
    {
        await page.GotoAsync("http://localhost:5000/");
        var emailInput = page.Locator("[data-tid='email']>input");
        await emailInput.FillAsync(email);
        var passwordInput = page.Locator("[data-tid='password']>input");
        await passwordInput.FillAsync(password);
        var submitButton = page.Locator("[type='submit']");
        await submitButton.ClickAsync();
    }
}