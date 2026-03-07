using System.IO;
using System.Net;

#pragma warning disable SYSLIB0014 // HttpWebRequest is obsolete

namespace LauncherV3.LauncherHelper;

public class CrpgChunkedRequest
{
    private string _sourceUrl = string.Empty;

    private string _targetPath = string.Empty;

    private CancellationTokenSource? _cancellationSource;

    private bool _downloading;

    private string? _tempPath;

    private string? _eTag;

    private IProgress<double>? _progress;

    static CrpgChunkedRequest()
    {
        if (ServicePointManager.DefaultConnectionLimit < Environment.ProcessorCount)
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
        }
    }

    private CrpgChunkedRequest()
    {
    }

    public static CrpgChunkedRequest Create(string sourceUrl)
    {
        return new CrpgChunkedRequest
        {
            _sourceUrl = sourceUrl,
            _downloading = false,
        };
    }

    public CrpgChunkedRequest TempFile(string tempFile)
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
        _targetPath = new FileInfo(targetPath).FullName;
        long contentLength = -1L;
        int chunks = Environment.ProcessorCount;
        long minChunkSize = 10485760L;
        long maxChunkSize = 536870912L;
        _ = minChunkSize;
        if (chunks > 1)
        {
            try
            {
                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(_sourceUrl);
                httpWebRequest.Method = "HEAD";
                using HttpWebResponse httpWebResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
                if (httpWebResponse.Headers[HttpResponseHeader.AcceptRanges] != "bytes")
                {
                    chunks = 1;
                }

                _eTag = httpWebResponse.Headers[HttpResponseHeader.ETag];
                contentLength = httpWebResponse.ContentLength;
            }
            catch (WebException)
            {
                contentLength = -1L;
            }
        }

        long chunkSize;
        if (contentLength == -1)
        {
            chunks = 1;
            chunkSize = -1L;
        }
        else
        {
            chunkSize = PowerFloor(contentLength / chunks, 2);
            if (chunkSize < minChunkSize)
            {
                chunkSize = minChunkSize;
            }

            if (chunkSize > maxChunkSize)
            {
                chunkSize = maxChunkSize;
            }

            chunks = (int)(contentLength / chunkSize);
            if (contentLength > chunks * chunkSize)
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

            chunkTasks[i] = DownloadChunkAsync(targetPath, i, chunkSize, contentLength, progressReporters[i]);
        }

        await Task.WhenAll(chunkTasks);
        _downloading = false;
        if (_cancellationSource!.Token.IsCancellationRequested)
        {
            return;
        }

        string tempFile = GetTempFile("file");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        using (FileStream outFile = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            int i = 0;
            for (int j = chunks; i < j; i++)
            {
                using (FileStream inFile = new(chunkTasks[i].Result, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
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

                        if (_cancellationSource!.Token.IsCancellationRequested)
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

        if (!_cancellationSource!.Token.IsCancellationRequested)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_targetPath)!);
            if (File.Exists(_targetPath))
            {
                File.Delete(_targetPath);
            }

            File.Move(tempFile, _targetPath);
        }
    }

    private async Task<string> DownloadChunkAsync(string targetPath, int chunkID, long chunkSize, long contentLength, IProgress<double> progress)
    {
        int maxDelay = 300000;
        int retryDelay = 5000;
        int maxRetries = 6;
        int retries = 0;
        long totalBytesRead = 0L;
        long totalBytesToRead = chunkSize;
        string tempFile = GetTempFile($"{chunkID:0000}");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        while (retries++ < maxRetries)
        {
            try
            {
                if (_cancellationSource!.Token.IsCancellationRequested)
                {
                    return tempFile;
                }

                using FileStream outFile = new(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);
                long num = chunkID * chunkSize;
                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(_sourceUrl);
                if (contentLength != -1 && chunkSize != -1)
                {
                    if (outFile.Length >= chunkSize)
                    {
                        return tempFile;
                    }

                    if (num + outFile.Length >= contentLength)
                    {
                        return tempFile;
                    }

                    httpWebRequest.AddRange(num + outFile.Length, num + chunkSize);
                }

                outFile.Position = outFile.Length;
                using (HttpWebResponse response = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
                {
                    using Stream responseStream = response.GetResponseStream()!;
                    byte[] buffer = new byte[1048576];
                    while (true)
                    {
                        int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        if (_cancellationSource!.Token.IsCancellationRequested)
                        {
                            return tempFile;
                        }

                        totalBytesRead += bytesRead;
                        progress.Report((double)totalBytesRead / totalBytesToRead);
                        await outFile.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                return tempFile;
            }
            catch (Exception ex)
            {
                if (_cancellationSource!.Token.IsCancellationRequested)
                {
                    return tempFile;
                }

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                await Task.Delay(Math.Min(maxDelay, retryDelay * retries * retries));
                if (retries == maxRetries)
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
            return Path.Combine(Path.GetTempPath(), $"{typeof(CrpgChunkedRequest).FullName}", $"{(ulong)_targetPath.GetHashCode()}{arg}.{extension}");
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
