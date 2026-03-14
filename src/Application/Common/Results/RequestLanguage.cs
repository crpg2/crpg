using System.Threading;

namespace Crpg.Application.Common.Results;

public static class RequestLanguage
{
    public const string Default = "en";

    public static string Current => CurrentValue.Value ?? Default;

    private static readonly AsyncLocal<string?> CurrentValue = new();

    public static IDisposable Use(string? language)
    {
        string? previous = CurrentValue.Value;
        CurrentValue.Value = string.IsNullOrWhiteSpace(language) ? Default : language;
        return new Scope(previous);
    }

    private sealed class Scope(string? previous) : IDisposable
    {
        public void Dispose()
        {
            CurrentValue.Value = previous;
        }
    }
}
