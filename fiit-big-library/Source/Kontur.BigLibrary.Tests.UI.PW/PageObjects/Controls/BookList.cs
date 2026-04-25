using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Controls.Base;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Factories;
using Microsoft.Playwright;

namespace Kontur.BigLibrary.Tests.UI.PW.PageObjects.Controls;

public class BookList : ControlBase
{
    // public BookList(ILocator locator) : base(locator)
    public BookList(ILocator locator, IControlFactory controlFactory, IPageFactory pageFactory) : base(locator, controlFactory, pageFactory)
    {
    }

    private string bookItemLocator => "[data-tid*='bookItem']";
    
    public ListControl<BookItem> Books => 
        ControlFactory.CreateList<BookItem>(Locator.Locator(bookItemLocator));

    public async Task<int> GetBooksCountAsync()
    {
        return await Books.CountAsync();
    }

    public async Task ExpectAnyBookAsync()
    {
        await Expect(Locator.Locator(bookItemLocator).First).ToBeVisibleAsync();
    }
    
    public BookItem GetBookItem(string bookName)
    {
        bookName = bookName.Replace(" ", "_");
        // return new BookItem(Locator.Locator($"[data-tid='bookItem-{bookName}']"));
        return ControlFactory.Create<BookItem>(Locator.Locator($"[data-tid='bookItem-{bookName}']"));
    }

    public async Task<bool> CheckBookPresence(string bookName)
    {
        bookName = bookName.Replace(" ", "_");
        var locator = Locator.Locator($"[data-tid='bookItem-{bookName}']");
        return await locator.IsVisibleAsync();
    }
}