using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Results;

public class ErrorTextTest
{
    [Test]
    public void ShouldUseCurrentCultureForUserNotFoundById()
    {
        using IDisposable languageScope = RequestLanguage.Use("ru");

        Error error = CommonErrors.UserNotFound(42);

        Assert.That(error.Title, Is.EqualTo("Пользователь не найден"));
        Assert.That(error.Detail, Is.EqualTo("Пользователь с идентификатором '42' не найден"));
    }

    [Test]
    public void ShouldUseCurrentCultureForNotEnoughGold()
    {
        using IDisposable languageScope = RequestLanguage.Use("zh-CN");

        Error error = CommonErrors.NotEnoughGold(100, 25);

        Assert.That(error.Title, Is.EqualTo("金币不足"));
        Assert.That(error.Detail, Is.EqualTo("需要 100 金币，但当前只有 25"));
    }

    [Test]
    public void ShouldFallbackToEnglishForUnsupportedCulture()
    {
        using IDisposable languageScope = RequestLanguage.Use("fr");

        Error error = CommonErrors.UserNotFound(Platform.Steam, "abc");

        Assert.That(error.Title, Is.EqualTo("User was not found"));
        Assert.That(error.Detail, Is.EqualTo("User with 'abc' on platform 'Steam' was not found"));
    }
}
