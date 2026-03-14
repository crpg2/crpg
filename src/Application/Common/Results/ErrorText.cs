using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;

namespace Crpg.Application.Common.Results;

internal static class ErrorText
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Resources = LoadResources();

    public static string Title(string key, string fallback, params object[] args) =>
        GetString($"{key}.Title", fallback, args);

    public static string Detail(string key, string fallback, params object[] args) =>
        GetString($"{key}.Detail", fallback, args);

    private static string GetString(string key, string fallback, params object[] args)
    {
        string template = GetLocalizedString(RequestLanguage.Current, key)
                          ?? GetLocalizedString(RequestLanguage.Default, key)
                          ?? fallback;

        return args.Length == 0 ? template : string.Format(template, args);
    }

    private static string? GetLocalizedString(string language, string key) =>
        Resources.TryGetValue(language, out IReadOnlyDictionary<string, string>? values)
            && values.TryGetValue(key, out string? value)
            ? value
            : null;

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadResources()
    {
        Assembly assembly = typeof(ErrorText).Assembly;
        return new ReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>(new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["en"] = LoadLocale(assembly, "en"),
            ["ru"] = LoadLocale(assembly, "ru"),
            ["zh-CN"] = LoadLocale(assembly, "zh-CN"),
        });
    }

    private static IReadOnlyDictionary<string, string> LoadLocale(Assembly assembly, string language)
    {
        string resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith($"ErrorTexts.{language}.json", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Missing error localization resource for '{language}'");

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException($"Missing error localization resource for '{language}'");
        using var reader = new StreamReader(stream);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())
               ?? throw new InvalidOperationException($"Invalid error localization resource for '{language}'");
    }
}
