using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Factories;
using Kontur.BigLibrary.Tests.UI.PW.PageObjects.Pages;
using Microsoft.Playwright;

namespace Kontur.BigLibrary.Tests.UI.PW.PageObjects.Controls.Base;

/// <summary>
/// Базовый абстрактный класс для всех PageElements (UI-компонентов, контролов).
/// Инкапсулирует локатор элемента и предоставляет методы для работы с ним.
/// </summary>
/// <remarks>
/// Оборачивает низкоуровневый ILocator в семантически значимый объект предметной области.
/// </remarks>
// public abstract class ControlBase(ILocator locator)
public abstract class ControlBase(ILocator locator, IControlFactory controlFactory, IPageFactory pageFactory)
{
    /// <summary>
    /// Локатор элемента страницы. Предоставляет доступ к низкоуровневым операциям
    /// взаимодействия с UI-элементом через Playwright API.
    /// </summary>
    public ILocator Locator { get; } = locator;

    public IControlFactory ControlFactory { get; } = controlFactory;
    
    public IPageFactory PageFactory { get; } = pageFactory;

    /// <summary>
    /// Создает объект для утверждений (assertions) над текущим элементом.
    /// Позволяет выполнять проверки состояния элемента в тестах.
    /// </summary>
    /// <returns>Объект ILocatorAssertions для цепочки утверждений</returns>
    public ILocatorAssertions Expect()
        => Assertions.Expect(Locator);
    
    public Task<string?> GetTextAsync()
    {
        return Locator.TextContentAsync();
    }

    public Task CheckTextAsync(string text)
    {
        return Expect(Locator).ToHaveTextAsync(text);
    }
    
    public async Task<T> ClickAndOpenPageAsync<T>() where T : PageBase
    {
        await Locator.ClickAsync(_defaultClickOptions);
        
        var pageObject = PageFactory.Create<T>(Locator.Page);
        return pageObject;
    }
    
    public async Task<TModal> ClickAndOpenModalAsync<TModal>() where TModal : ModalBase<TModal>, IModal
    {
        await Locator.ClickAsync();
        
        var pageObject = ControlFactory.CreateModal<TModal>(Locator.Page);
        return pageObject;
    }
    
    public Task<bool> IsVisibleAsync(LocatorIsVisibleOptions? options = default)
    {
        return Locator.IsVisibleAsync(options);
    }

    public Task WaitVisibleAsync()
    {
        return Expect(Locator).ToBeVisibleAsync();
    }
    
    public Task WaitInvisibleAsync()
    {
        return Expect(Locator).ToBeHiddenAsync();
    }

    public async Task<bool> HasErrorAsync()
    {
        await Locator.WaitForAsync(
            new LocatorWaitForOptions {State = WaitForSelectorState.Visible}
        );
        return await Locator.Locator("_react=[error]").First.IsVisibleAsync();
    }

    public async Task<bool> HasWarningAsync()
    {
        await Locator.WaitForAsync(
            new LocatorWaitForOptions {State = WaitForSelectorState.Visible}
        );
        return await Locator.Locator("_react=[warning]").First.IsVisibleAsync();
    }

    public Task ClickAsync(LocatorClickOptions? options = null)
    {
        options ??= _defaultClickOptions;
        return Locator.ClickAsync(options);
    }

    public Task HoverAsync(LocatorHoverOptions? options = default)
    {
        return Locator.HoverAsync(options);
    }

    public Task WaitErrorAsync(LocatorAssertionsToBeVisibleOptions? options = default)
    {
        return Expect(Locator.Locator("_react=[error]").First).ToBeVisibleAsync(options);
    }

    public Task WaitErrorAbsenceAsync(LocatorAssertionsToBeVisibleOptions? options = default)
    {
        return Expect(Locator.Locator("_react=[error]").First).Not.ToBeVisibleAsync(options);
    }

    public Task WaitWarningAsync(LocatorAssertionsToBeVisibleOptions? options = default)
    {
        return Expect(Locator.Locator("_react=[warning]").First).ToBeVisibleAsync(options);
    }

    public Task WaitWarningAbsenceAsync(LocatorAssertionsToBeVisibleOptions? options = default)
    {
        return Expect(Locator.Locator("_react=[warning]").First).Not.ToBeVisibleAsync(options);
    }

    public Task<string?> GetAttributeValueAsync(string attributeName,
        LocatorGetAttributeOptions? options = default)
    {
        return GetAttributeValueAsync(Locator, attributeName, options);
    }

    protected static ILocatorAssertions Expect(ILocator locator)
    {
        return Assertions.Expect(locator);
    }

    protected static Task<string?> GetAttributeValueAsync(
        ILocator locator,
        string attributeName,
        LocatorGetAttributeOptions? options = default)
    {
        return locator.GetAttributeAsync(attributeName, options);
    }

    private LocatorClickOptions _defaultClickOptions = new()
    {
        Force = true
    };
}