using NUnit.Framework;
using UserCreatorTask.UserValidators;

namespace UserValidatorTests;

[Parallelizable(ParallelScope.Fixtures)]
public class NameValidatorTests
{
    [TestCaseSource(nameof(TestCases))]
    public bool Test_IsValidMethod(string email) => _validator.IsValid(email);

    [Test]
    public void TestIsValid_ThrowsArgumentNullException_WhenNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.IsValid(null), "Ожидали ошибку при получении null");
    }

    private readonly NameValidator _validator = new();

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData("Aboba Abobov").SetName("IsValid_ShouldReturnTrue_WhenNameIsValid").Returns(true);
        yield return new TestCaseData("It Am").SetName("IsValid_ShouldReturnTrue_WithShortValidName").Returns(true);
        yield return new TestCaseData("Aboba Abobovich Abobov")
            .SetName("IsValid_ShouldReturnFalse_WhenNameIsThreeWords").Returns(false);
        yield return new TestCaseData("ПростоАбоба").SetName("IsValid_ShouldReturnFalse_WhenNameIsOneWord")
            .Returns(false);
        yield return new TestCaseData("R2 D2").SetName("IsValid_ShouldReturnFalse_WhenNameContainsNumbers")
            .Returns(false);
        yield return new TestCaseData("!@##@# #!@$!#")
            .SetName("IsValid_ShouldReturnFalse_WhenNameContainsSpecialSymbols").Returns(false);
        yield return new TestCaseData("Абоба Абобов").SetName("IsValid_ShouldReturnFalse_WhenNameIsNotInEnglish")
            .Returns(false);
        yield return new TestCaseData("S. Grant").SetName("IsValid_ShouldReturnFalse_WhenNameIsShortenedWithDot")
            .Returns(false);
        yield return new TestCaseData("S Grant").SetName("IsValid_ShouldReturnTrue_WhenNameIsShortenedWithoutDot")
            .Returns(true);
    }
}