using System.IO;
using System.Net;
using System.Net.Http;

#pragma warning disable SYSLIB0014 // ServicePointManager is obsolete

namespace LauncherV3.LauncherHelper;
public class CrpgChunkedRequestWithHttpClient
{
    private const long KB = 1024L;

    private const long MB = 1048576L;

    private const long GB = 1073741824L;

    private readonly string _eTag = string.Empty;

    private string _sourceUrl = string.Empty;

    private string _targetPath = string.Empty;

    private CancellationTokenSource? _cancellationSource;

    private bool _downloading;

    private string _tempPath = string.Empty;

    private IProgress<double>? _progress;

    static CrpgChunkedRequestWithHttpClient()
    {
        if (ServicePointManager.DefaultConnectionLimit < Environment.ProcessorCount)
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
        }
    }

    private CrpgChunkedRequestWithHttpClient()
    {
    }

    public static CrpgChunkedRequestWithHttpClient Create(string sourceUrl)
    {
        return new CrpgChunkedRequestWithHttpClient
        {
            _sourceUrl = sourceUrl,
            _downloading = false,
        };
    }

    public CrpgChunkedRequestWithHttpClient TempFile(string tempFile)
    {
        _tempPath = new FileInfo(tempFile).FullName;
        return this;
    }

    public async Task PauseAsync()
    {
        _cancellationSource?.Cancel(throwOnFirstException: false);
        await Task.Delay(0);
    }

    public async Task ResumeAsync()
    {
        if (_progress != null)
        {
            await DownloadAsync(_targetPath, _progress);
        }
    }

    public async Task AbortAsync()
    {
        if (!_cancellationSource?.IsCancellationRequested ?? false)
        {
            _cancellationSource?.Cancel(throwOnFirstException: false);
        }

        SpinWait.SpinUntil(() => !_downloading, 5000);
        string tempFile = GetTempFile("*");
        string[] files = Directory.GetFiles(Path.GetDirectoryName(tempFile)!, Path.GetFileName(tempFile));
        foreach (string path in files)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        await Task.Delay(0);
    }

    public async Task DownloadAsync(string targetPath, IProgress<double> overallProgress)
    {
        _progress = overallProgress;

        if (_downloading)
        {
            throw new Exception("Download already in progress.");
        }

        _cancellationSource = new CancellationTokenSource();
        var cancellationToken = _cancellationSource.Token;
        _targetPath = new FileInfo(targetPath).FullName;

        using var httpClient = new HttpClient();
        var headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _sourceUrl), cancellationToken);
        headResponse.EnsureSuccessStatusCode();

        bool supportsRange = headResponse.Headers.AcceptRanges.Contains("bytes");
        long contentLength = headResponse.Content.Headers.ContentLength ?? -1;

        int chunks = Environment.ProcessorCount;
        long minChunkSize = 10485760L; // 10 MB
        long maxChunkSize = 536870912L; // 512 MB

        long chunkSize;
        if (!supportsRange || contentLength == -1)
        {
            chunkSize = contentLength;
            chunks = 1;
        }
        else
        {
            chunkSize = Math.Max(minChunkSize, Math.Min(maxChunkSize, contentLength / chunks));
            chunks = (int)(contentLength / chunkSize);
            if (contentLength % chunkSize > 0)
            {
                chunks++;
            }
        }

        Task<string>[] chunkTasks = new Task<string>[chunks];
        _downloading = true;
        double[] individualProgress = new double[chunks];
        Progress<double>[] progressReporters = new Progress<double>[chunks];

        for (int i = 0; i < chunks; i++)
        {
            int localI = i;
            progressReporters[localI] = new Progress<double>(p =>
            {
                individualProgress[localI] = p;
                overallProgress.Report(individualProgress.Average());
            });

            chunkTasks[i] = DownloadChunkAsync(httpClient, targetPath, i, chunkSize, contentLength, cancellationToken, progressReporters[i]);
        }

        await Task.WhenAll(chunkTasks);
        _downloading = false;

        if (_cancellationSource.Token.IsCancellationRequested)
        {
            return;
        }

        string tempFile = GetTempFile("file");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        using (FileStream outFile = File.Open(tempFile, FileMode.Create, FileAccess.Write))
        {
            int i = 0;
            for (int j = chunks; i < j; i++)
            {
                using (FileStream inFile = File.Open(chunkTasks[i].Result, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1048576];
                    while (true)
                    {
                        int num4;
                        int bytes = num4 = await inFile.ReadAsync(buffer, 0, buffer.Length);
                        if (num4 <= 0)
                        {
                            break;
                        }

                        if (_cancellationSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        await outFile.WriteAsync(buffer, 0, bytes);
                    }

                    outFile.Seek(-1L, SeekOrigin.Current);
                }

                File.Delete(chunkTasks[i].Result);
            }
        }

        if (!_cancellationSource.Token.IsCancellationRequested)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_targetPath)!);
            if (File.Exists(_targetPath))
            {
                File.Delete(_targetPath);
            }

            File.Move(tempFile, _targetPath);
        }
    }

    private async Task<string> DownloadChunkAsync(HttpClient httpClient, string targetPath, int chunkID, long chunkSize, long contentLength, CancellationToken cancellationToken, IProgress<double> progress)
    {
        int maxDelay = 300000;
        int retryDelay = 5000;
        int maxRetries = 6;
        int retries = 0;
        string tempFile = GetTempFile($"{chunkID:0000}");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        long totalBytesRead = 0L;
        long totalBytesToRead = chunkSize;

        while (retries++ < maxRetries)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return tempFile;
                }

                using FileStream outFile = File.Open(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                long offset = chunkID * chunkSize;
                if (contentLength != -1 && chunkSize != -1)
                {
                    if (outFile.Length >= chunkSize || offset + outFile.Length >= contentLength)
                    {
                        return tempFile;
                    }
                }

                outFile.Position = outFile.Length;

                var request = new HttpRequestMessage(HttpMethod.Get, _sourceUrl);
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset + outFile.Length, offset + chunkSize - 1);

                using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    using var responseStream = await response.Content.ReadAsStreamAsync();
                    byte[] buffer = new byte[1048576];

                    while (true)
                    {
                        int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        totalBytesRead += bytesRead;
                        progress.Report((double)totalBytesRead / totalBytesToRead);

                        await outFile.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return tempFile;
                }

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                await Task.Delay(Math.Min(maxDelay, retryDelay * retries * retries), cancellationToken);
                if (retries >= maxRetries)
                {
                    throw new Exception("Failed to download chunk", ex);
                }
            }
        }

        return tempFile;
    }

    private string GetTempFile(string extension)
    {
        string arg = string.Empty;
        if (!string.IsNullOrWhiteSpace(_eTag))
        {
            arg = string.Format(".{0}", _eTag.Replace("\"", "").Replace(":", ""));
        }

        if (string.IsNullOrWhiteSpace(_tempPath))
        {
            return Path.Combine(Path.GetTempPath(), $"{typeof(CrpgChunkedRequestWithHttpClient).FullName}", $"{(ulong)_targetPath.GetHashCode()}{arg}.{extension}");
        }

        return $"{_tempPath}{arg}.{extension}";
    }

    private static int PowerFloor(int val, int pow)
    {
        if (pow < 0)
        {
            return 0;
        }

        if (pow == 0)
        {
            return val;
        }

        if (val >> 1 > 0)
        {
            return PowerFloor(val >> 1, 2) << 1;
        }

        return val;
    }

    private static long PowerFloor(long val, int pow)
    {
        if (pow < 0)
        {
            return 0L;
        }

        if (pow == 0)
        {
            return val;
        }

        if (val >> 1 > 0)
        {
            return PowerFloor(val >> 1, 2) << 1;
        }

        return val;
    }
}
