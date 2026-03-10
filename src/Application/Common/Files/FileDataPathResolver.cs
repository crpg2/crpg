namespace Crpg.Application.Common.Files;

internal static class FileDataPathResolver
{
    internal static string Resolve(string relativePath)
    {
        string baseDir = AppContext.BaseDirectory;
        string candidate = Path.Combine(baseDir, relativePath);
        if (File.Exists(candidate))
        {
            return candidate;
        }

        foreach (string dir in Directory.EnumerateDirectories(baseDir, "net*", SearchOption.TopDirectoryOnly))
        {
            candidate = Path.Combine(dir, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(baseDir, relativePath);
    }
}
