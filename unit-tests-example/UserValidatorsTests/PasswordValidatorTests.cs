using NUnit.Framework;
using UserCreatorTask.UserValidators;

namespace UserValidatorTests;

[Parallelizable(ParallelScope.Children)]
public class PasswordValidatorTests
{
    [Test]
    public void IsValid_ShouldReturnTrue_WhenPasswordIsValid()
    {
        var password = new string('a', 100) + "1A!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.True, message: "Ожидали True при проверки правильного пароля на валидность");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordIsTooShort()
    {
        var password = "a1A!a";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False,
            message: "Ожидали False при проверки слишко короткого (< 100 символов) пароля на валидность");
    }

    [Test]
    public void IsValid_ShouldReturnTrue_WhenPasswordIsExactlyHundredSymbolsLong()
    {
        var password = new string('a', 97) + "1A!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.True, message: "Ожидали True при проверки правильного пароля на валидность");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordWithoutCapitalLetters()
    {
        var password = new string('a', 100) + "1!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False, message: "Ожидали False при проверки на валидность пароля без заглавных букв");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordWithoutNumbers()
    {
        var password = new string('a', 100) + "A!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False, message: "Ожидали False при проверки на валидность пароля без цифр");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordWithoutSpecialSymbols()
    {
        var password = new string('a', 100) + "1!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False, message: "Ожидали False при проверки на валидность пароля без спец. символов");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordWithoutLowercaseLetters()
    {
        var password = new string('A', 100) + "1!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False, message: "Ожидали False при проверки на валидность пароля без строчных букв");
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenPasswordIsNotInEnglish()
    {
        var password = new string('ф', 100) + "1Ф!";
        var result = _validator.IsValid(password);
        Assert.That(result, Is.False,
            message: "Ожидали False при проверки на валидность пароля с не английскими буквами");
    }

    [NonParallelizable]
    [Test]
    public void TestIsValid_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.IsValid(null), "Ожидали ошибку при получении null");
    }

    private readonly PasswordValidator _validator = new();
}