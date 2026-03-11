using NUnit.Framework;
using UserCreatorTask.UserValidators;

namespace UserValidatorTests;

public class EmailValidatorTests
{
    [TestCaseSource(nameof(TestCases))]
    public bool Test_IsValidMethod(string email)
    {
        return _validator.IsValid(email);
    }

    [Test]
    public void TestIsValid_ThrowsArgumentNullException_WhenNameIsNull()
    {
        TestDelegate action = () => _validator.IsValid(null);
        Assert.Throws<ArgumentNullException>(action, "Ожидали ошибку при проверке на валидность null");
    }

    private readonly EmailValidator _validator = new();

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData("aboba@gmail.com").SetName("IsValid_ShouldReturnTrue_WhenEmailIsValid")
            .Returns(true);
        yield return new TestCaseData("abobagmail.com").SetName("IsValid_ShouldReturnFalse_WithoutAtSymbol")
            .Returns(false);
        yield return new TestCaseData("aboba@gmail.comma").SetName("IsValid_ShouldReturnFalse_WhenDomainIsTooLong")
            .Returns(false);
        yield return new TestCaseData("aboba@gmail.").SetName("IsValid_ShouldReturnFalse_WithoutDomain").Returns(false);
        yield return new TestCaseData("aboba@gmailcom").SetName("IsValid_ShouldReturnFalse_WithoutDotBeforeDomain")
            .Returns(false);
        yield return new TestCaseData("@gmail.com").SetName("IsValid_ShouldReturnFalse_WithoutInboxAddress")
            .Returns(false);
        yield return new TestCaseData("aboba@.com").SetName("IsValid_ShouldReturnFalse_WithoutProviderAddress")
            .Returns(false);
        yield return new TestCaseData("12das312@12asd321.com").SetName("IsValid_ShouldReturnTrue_WhenAddressHasNumbers")
            .Returns(true);
        yield return new TestCaseData("12312@12321.com").SetName("IsValid_ShouldReturnTrue_WhenAddressIsNumbers")
            .Returns(true);
        yield return new TestCaseData("a@b.com").SetName("IsValid_ShouldReturnTrue_WhenAddressIsShort")
            .Returns(true);
        yield return new TestCaseData("&e3()@!?#@.com").SetName("IsValid_ShouldReturnFalse_WhenAddressIsSpecialSymbols")
            .Returns(false);
        yield return new TestCaseData("ab@oba@gmail.com").SetName("IsValid_ShouldReturnFalse_WithTooManyAtSymbols")
            .Returns(false);
    }
}