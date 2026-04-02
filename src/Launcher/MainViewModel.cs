using System.Collections.Concurrent;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LauncherV3.LauncherHelper;
using Microsoft.Win32;
using static LauncherV3.LauncherHelper.GameInstallationFolderResolver;

namespace LauncherV3;

public partial class MainViewModel : ObservableObject
{
    public enum Platform
    {
        Steam,
        Epic,
        Xbox,
    }

    public static readonly string ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Crpg Launcher");

    private static readonly string HashFileName = "CrpgHash.xml";
    private static readonly string ConfigFileName = "config.json";

    private readonly ConcurrentQueue<string> _messageQueue = new();

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isBeta;

    [ObservableProperty]
    private bool _ipMappingEnabled;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private bool _isVerifying;

    [ObservableProperty]
    private bool _isGameUpToDate;

    [ObservableProperty]
    private Platform _selectedPlatform;

    [ObservableProperty]
    private GameInstallationInfo? _gameLocation;

    [ObservableProperty]
    private string _version = "1.0.0";

    [ObservableProperty]
    private bool _isCrpgInstalled;

    public MainViewModel()
    {
        PlatformOptions = Enum.GetValues(typeof(Platform)).Cast<Platform>().ToList();
    }

    public event EventHandler? RequestClose;

    public List<Platform> PlatformOptions { get; }

    public ICommand StartUpdateCrpgCommand => IsGameUpToDate ? StartCrpgCommand : UpdateGameFilesCommand;

    public void ApplySettings()
    {
        if (ReadConfig())
        {
            SelectedPlatform = Config.LastPlatform;
            GameLocation = Config.GameLocations.TryGetValue(SelectedPlatform, out var gameLocation) ? gameLocation : null;
            IpMappingEnabled = Config.IpMappingEnabled;
            IsGameUpToDate = false;
        }

        NotifyUI();
    }

    [RelayCommand(CanExecute = nameof(CanDetect))]
    public void Detect()
    {
        UpdateGameLocation(SelectedPlatform, force: true);
    }

    public string FlushText()
    {
        StringBuilder sb = new();
        while (_messageQueue.TryDequeue(out string? text))
        {
            sb.AppendLine(text);
        }

        return sb.ToString();
    }

    public bool HasWritePermissionOnConfigDir()
    {
        try
        {
            if (!Directory.Exists(ProgramDataPath))
            {
                Directory.CreateDirectory(ProgramDataPath);
            }

            List<string> lines = new()
            {
                "test",
            };
            File.WriteAllLines(Path.Combine(ProgramDataPath, "test.ini"), lines);
            File.Delete(Path.Combine(ProgramDataPath, "test.ini"));
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // If an unauthorized access exception occurred, we don't have write permissions
            return false;
        }
        catch (IOException)
        {
            // Handle other IO exceptions if necessary
            return false;
        }
    }

    public bool ReadConfig()
    {
        return Config.ReadConfig(ProgramDataPath, ConfigFileName);
    }

    [RelayCommand(CanExecute = nameof(CanDetect))]
    public void ResetConfig()
    {
        Config.ClearLocations();
        WriteConfig();
        ApplySettings();
        WriteToConsole("Config Reset");
    }

    public void StartRoutine()
    {
        IsVerifying = false;
        IsUpdating = false;
        ApplySettings();
        Version = ReadTextFromResource("pack://application:,,,/launcherversion.txt");
        _ = CheckNewVersion();
        NotifyUI();
    }

    public string UpdateButtoneText(bool value) => DetermineUpdateButtonText(value);

    public void UpdateGameLocation(Platform platform, bool force = false)
    {
        if (Config.GameLocations.ContainsKey(platform) && !force)
        {
            GameLocation = Config.GameLocations[platform];
            Config.LastPlatform = platform;
        }

        if (GameLocation == null || force)
        {
            WriteToConsole("Trying to Auto Resolve Bannerlord Location");
            if (platform == Platform.Epic)
            {
                GameLocation = ResolveBannerlordEpicGamesInstallation();
                _ = HandleGameLocationChange(platform);
            }

            if (platform == Platform.Steam)
            {
                GameLocation = ResolveBannerlordSteamInstallation();
                _ = HandleGameLocationChange(platform);
            }

            if (platform == Platform.Xbox)
            {
                GameLocation = ResolveBannerlordXboxInstallation();
                _ = HandleGameLocationChange(platform);
            }
        }
        else
        {
            Config.LastPlatform = platform;
            WriteConfig();
        }
    }

    [RelayCommand(CanExecute = nameof(CanVerify))]
    public async Task VerifyGameFilesActionAsync()
    {
        IsVerifying = true;
        if (GameLocation == null)
        {
            WriteToConsole("Game Location is not properly set");
            IsVerifying = false;
            return;
        }

        await VerifyGameFilesAsync();
    }

    public void WriteToConsole(string text)
    {
        _messageQueue.Enqueue(text);
    }

    private bool CanOpenFolder()
    {
        bool cantOpenFolder = (SelectedPlatform == Platform.Epic) || IsUpdating || IsVerifying;
        return !cantOpenFolder;
    }

    private bool CanVerify()
    {
        bool canVerify = !IsUpdating && !IsVerifying && GameLocation != null;
        return canVerify;
    }

    private bool CanStartCrpg()
    {
        bool canStartCrpg = !IsUpdating && !IsVerifying && GameLocation != null && IsGameUpToDate;
        return canStartCrpg;
    }

    private bool CanDetect()
    {
        bool canDetect = !IsUpdating && !IsVerifying;
        return canDetect;
    }

    private bool CanUpdate()
    {
        bool canUpdate = !IsUpdating && !IsVerifying && GameLocation != null;
        return canUpdate;
    }

    private string GetBaseUrl()
    {
        if (IsBeta)
        {
            return "https://namidaka.fr/";
        }

        return IpMappingEnabled ? "https://c-rpg.site/" : "https://c-rpg.eu/";
    }

    private async Task CheckNewVersion()
    {
        string onlineVersion = await OnlineLauncherVersion(GetBaseUrl() + "LauncherVersion.txt");
        if (onlineVersion == "failed")
        {
            return;
        }

        if (Version.Replace("\r", "").Replace("\n", "") != onlineVersion)
        {
            WriteToConsole($"This version is outdated please download version {onlineVersion}");
            WriteToConsole($"at https://c-rpg.eu/LauncherV3.exe");
        }
        else
        {
            WriteToConsole("Your Launcher is up to date");
        }
    }

    private void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private string DetermineUpdateButtonText(bool value)
    {
        if (value)
        {
            return "Update cRPG";
        }
        else
        {
            return "Install cRPG";
        }
    }

    private async Task Extract(HttpResponseMessage response, string path)
    {
        using (var stream = await response.Content.ReadAsStreamAsync())
        {
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                try
                {
                    TarFile.ExtractToDirectory(gzipStream, path, overwriteFiles: true);
                }
                catch (Exception e)
                {
                    WriteToConsole("Error while trying to extract download file");
                    WriteToConsole(e.Message);
                }
            }
        }
    }

    private void ExtractAndDeleteFile(string inputPath, string outputPath)
    {
        using (var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                try
                {
                    TarFile.ExtractToDirectory(gzipStream, outputPath, overwriteFiles: true);
                }
                catch (Exception e)
                {
                    WriteToConsole("Error while trying to extract download file");
                    WriteToConsole(e.Message);
                }
            }
        }

        try
        {
            File.Delete(inputPath);
        }
        catch
        {
        }
    }

    private async Task HandleGameLocationChange(Platform platform)
    {
        if (GameLocation == null)
        {
            WriteToConsole("Bannerlord was not found in the location");
            Config.GameLocations[platform] = null;
            Config.LastPlatform = platform;
            WriteConfig();
        }
        else
        {
            WriteToConsole("Bannerlord was detected in the location");
            Config.GameLocations[platform] = GameLocation;
            Config.LastPlatform = platform;
            WriteConfig();
            if (!HashExist())
            {
                if (!Directory.Exists(Path.Combine(GameLocation.InstallationPath, "Modules/cRPG")))
                {
                    WriteToConsole("cRPG is not Installed, Click on Install Mod to Install");
                    IsCrpgInstalled = false;
                    IsGameUpToDate = false;
                }
                else
                {
                    WriteToConsole("Discovering Potential cRPG Installation");
                    await VerifyGameFilesAsync(download: true);
                }
            }
        }

        NotifyUI();
    }

    private bool HashExist()
    {
        return File.Exists(Path.Combine(ProgramDataPath, ConfigFileName));
    }

    private void NotifyUI()
    {
        VerifyGameFilesActionCommand.NotifyCanExecuteChanged();
        UpdateGameFilesCommand.NotifyCanExecuteChanged();
        OpenFolderCommand.NotifyCanExecuteChanged();
        StartCrpgCommand.NotifyCanExecuteChanged();
        DetectCommand.NotifyCanExecuteChanged();
        ResetConfigCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(StartUpdateCrpgCommand));
    }

    private async Task<string> OnlineLauncherVersion(string url)
    {
        using (HttpClient client = new())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                string firstLine = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                return firstLine;
            }
            catch (Exception)
            {
                return "failed";
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenFolder))]
    private void OpenFolder()
    {
        var folderDialog = new OpenFolderDialog();

        if (folderDialog.ShowDialog() == true)
        {
            GameLocation = GameInstallationFolderResolver.CreateGameInstallationInfo(folderDialog.FolderName, SelectedPlatform);
        }

        if (GameLocation != null)
        {
            SelectedPlatform = GameLocation.Platform;
            Config.GameLocations[SelectedPlatform] = GameLocation;
            Config.LastPlatform = SelectedPlatform;
            WriteConfig();
            IsCrpgInstalled = true;
        }
        else
        {
            WriteToConsole("Bannerlord was not found at current Location");
            IsCrpgInstalled = false;
        }
    }

    private string ReadTextFromResource(string packUri)
    {
        Uri uri = new(packUri, UriKind.RelativeOrAbsolute);
        StreamResourceInfo? resourceInfo = Application.GetResourceStream(uri);

        if (resourceInfo != null)
        {
            using (StreamReader reader = new(resourceInfo.Stream))
            {
                return reader.ReadToEnd();
            }
        }

        return "Resource not found.";
    }

    [RelayCommand(CanExecute = nameof(CanStartCrpg))]
    private void StartCrpg()
    {
        ApplyIpMappingSetting(GameLocation?.ProgramWorkingDirectory ?? string.Empty);

        if (GameLocation == null)
        {
            WriteToConsole("Game Location is not set!");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = GameLocation?.ProgramWorkingDirectory ?? string.Empty,
            FileName = GameLocation!.Program,
            Arguments = GameLocation.ProgramArguments ?? string.Empty,
            UseShellExecute = true,
        });
        Close();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateGameFilesAsync()
    {
        IsUpdating = true;
        if (!HashExist())
        {
            await VerifyGameFilesAsync();
        }

        XmlDocument doc = new();
        try
        {
            string url = GetBaseUrl() + "hash.xml";
            using (var client = new HttpClient())
            {
                string xmlContent = await client.GetStringAsync(url);
                doc.LoadXml(xmlContent);
            }
        }
        catch (Exception ex)
        {
            WriteToConsole($"Website may be updating, Use modmail to report it if issue persists in 15 minutes");
            WriteToConsole($"Error: {ex.Message}");
            IsUpdating = false;
            return;
        }

        if (doc?.DocumentElement == null)
        {
            IsUpdating = false;
            return;
        }

        Dictionary<string, string> distantAssets = new();
        Dictionary<string, string> distantMaps = new();
        string distantRestHash = CrpgHashMethods.ReadHash(doc, distantAssets, distantMaps);
        XmlDocument doc2 = new();
        try
        {
            doc2.Load(Path.Combine(ProgramDataPath, HashFileName));
        }
        catch (Exception ex)
        {
            WriteToConsole(ex.Message);
            WriteToConsole("Please Verify your game files first");
            IsUpdating = false;
            return;
        }

        Dictionary<string, string> localAssets = new();
        Dictionary<string, string> localMaps = new();

        string localRestHash = CrpgHashMethods.ReadHash(doc2, localAssets, localMaps);
        bool downloadRest = localRestHash != distantRestHash;

        var assetsToDownload = distantAssets.Where(a => !localAssets.Contains(a)).ToList();
        var assetsToDelete = localAssets.Where(a => !distantAssets.Contains(a)).ToList();
        if (Config.DevMode)
        {
            assetsToDelete = localAssets.Where(a => distantAssets.ContainsKey(a.Key) && !distantAssets.ContainsValue(a.Value)).ToList();
        }

        var mapsToDelete = localMaps.Where(a => !distantMaps.Contains(a)).ToList();
        if (Config.DevMode)
        {
            mapsToDelete = localMaps.Where(a => distantMaps.ContainsKey(a.Key) && !distantMaps.ContainsValue(a.Value)).ToList();
        }

        var mapsToDownload = distantMaps.Where(a => !localMaps.Contains(a)).ToList();

        if (assetsToDelete.Count == 0 && assetsToDownload.Count == 0 && mapsToDownload.Count == 0 && mapsToDelete.Count == 0 && !downloadRest)
        {
            WriteToConsole("Your game is Up To Date");
            IsGameUpToDate = true;
            IsUpdating = false;
            return;
        }

        if (GameLocation == null)
        {
            WriteToConsole("Cannot Download update as Bannerlord Location is not known");
            IsUpdating = false;
            return;
        }

        foreach (var assetToDelete in assetsToDelete)
        {
            string pathToDelete = Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/AssetPackages/", assetToDelete.Key);
            WriteToConsole(pathToDelete);
            try
            {
                File.Delete(pathToDelete);
            }
            catch
            {
            }
        }

        foreach (var mapToDelete in mapsToDelete)
        {
            string pathToDelete = Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/SceneObj/", mapToDelete.Key);
            WriteToConsole($"deleting {pathToDelete}");
            try
            {
                Directory.Delete(pathToDelete, recursive: true);
            }
            catch
            {
            }
        }

        string cRPGFolder = Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/");
        if (Config.DevMode)
        {
            WriteToConsole("You're in Dev Mode. Only Assets and Maps will update. Other files remain untouched");
            WriteToConsole("If you want to update the other files, Uncheck Dev Mode , then put back your files");
        }
        else
        {
            if (downloadRest && Directory.Exists(cRPGFolder))
            {
                foreach (string dir in Directory.GetDirectories(cRPGFolder))
                {
                    if (Path.GetFileName(dir) == "SceneObj" || Path.GetFileName(dir) == "AssetPackages")
                    {
                        continue;
                    }

                    WriteToConsole($"deleting {dir}");
                    Directory.Delete(dir, recursive: true);
                }

                foreach (string file in Directory.GetFiles(cRPGFolder))
                {
                    WriteToConsole($"deleting {file}");
                    File.Delete(file);
                }

                try
                {
                    string subModulePath = Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/SubModule.xml");
                    if (File.Exists(subModulePath))
                    {
                        WriteToConsole($"deleting {subModulePath}");
                        File.Delete(subModulePath);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        List<Task> allTasks = new();
        bool updateSuccessful = true;
        using (var client = new HttpClient())
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            foreach (var assetToDownload in assetsToDownload)
            {
                try
                {
                    // Download the file
                    WriteToConsole($"Downloading and extracting {assetToDownload.Key + ".tar.gz"} ");
                    string fileToDownload = assetToDownload.Key + ".tar.gz";
                    var chunkedRequest = CrpgChunkedRequest.Create(GetBaseUrl() + "AssetPackages/" + assetToDownload.Key + ".tar.gz");
                    string tempPath = Path.Combine(Path.GetTempPath(), fileToDownload);
                    IProgress<double> currentProgress = new Progress<double>(p =>
                    {
                       Progress = p * 100;
                    });
                    await chunkedRequest.DownloadAsync(tempPath, currentProgress);

                    var extractionTask1 = Task.Run(() => ExtractAndDeleteFile(tempPath, Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/AssetPackages/")));
                    allTasks.Add(extractionTask1);
                }
                catch (Exception ex)
                {
                    updateSuccessful = false;
                    WriteToConsole(ex.Message);
                }
            }

            Progress = 100;

            var semaphore = new SemaphoreSlim(20); // Allows 20 concurrent tasks
            var downloadTasks = new List<Task>();
            var progresses = new ConcurrentDictionary<int, double>();
            IProgress<double> overallProgress = new Progress<double>(p =>
            {
                // Calculate the overall progress based on the individual progresses.
                double totalProgress = progresses.Values.Sum();
                double averageProgress = totalProgress / progresses.Count;
                Progress = averageProgress;
            });

            int mapIndex = 0; // To track the index of the map being downloaded

            foreach (var mapToDownload in mapsToDownload)
            {
                await semaphore.WaitAsync();
                int localIndex = mapIndex; // Local copy for the closure in the lambda expression
                mapIndex++; // Increment for the next iteration

                downloadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        WriteToConsole($"Downloading and extracting {mapToDownload.Key + ".tar.gz"} ");
                        string fileToDownload = mapToDownload.Key + ".tar.gz";
                        var chunkedRequest = CrpgChunkedRequest.Create(GetBaseUrl() + "SceneObj/" + fileToDownload);
                        string tempPath = Path.Combine(Path.GetTempPath(), fileToDownload);

                        progresses.TryAdd(localIndex, 0); // Initialize progress for this download

                        IProgress<double> progressReporter = new Progress<double>(p =>
                        {
                            progresses[localIndex] = p; // Update progress safely
                            overallProgress.Report(p); // Report individual progress
                        });

                        await chunkedRequest.DownloadAsync(tempPath, progressReporter);
                        await Task.Run(() => ExtractAndDeleteFile(tempPath, Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/SceneObj/")));
                    }
                    catch (Exception ex)
                    {
                        updateSuccessful = false; // This should be thread-safe if accessed concurrently
                        WriteToConsole(ex.Message);
                    }
                    finally
                    {
                        progresses.TryRemove(localIndex, out _); // Remove progress when done
                        semaphore.Release(); // Release the slot
                    }
                }));
            }

            // Wait for all downloads to complete
            await Task.WhenAll(downloadTasks);

            Progress = 100;

            if (downloadRest)
            {
                try
                {
                    // Download the file
                    WriteToConsole($"Downloading and extracting the xmls files : rest.tar.gz");
                    string fileToDownload = "rest" + ".tar.gz";
                    var chunkedRequest = CrpgChunkedRequest.Create(GetBaseUrl() + fileToDownload);
                    string tempPath = Path.Combine(Path.GetTempPath(), fileToDownload);
                    IProgress<double> currentProgress = new Progress<double>(p =>
                    {
                        Progress = p * 100;
                    });
                    await chunkedRequest.DownloadAsync(tempPath, currentProgress);

                    var extractionTask3 = Task.Run(() =>
                    {
                        ExtractAndDeleteFile(tempPath, Path.Combine(GameLocation.InstallationPath, "Modules/cRPG/"));
                    });
                    allTasks.Add(extractionTask3);
                }
                catch (Exception ex)
                {
                    updateSuccessful = false;
                    WriteToConsole(ex.Message);
                }
            }

            Progress = 100;
        }

        Progress = 100;

        await Task.WhenAll(allTasks);
        if (updateSuccessful)
        {
            doc.Save(Path.Combine(ProgramDataPath, HashFileName));
            WriteToConsole("Update Finished");
            IsGameUpToDate = true;
            WriteConfig();
        }
        else
        {
            WriteToConsole("There were issues during the update");
            WriteToConsole("Verifying Game Files to validate Download");
            IsUpdating = false;
            IsVerifying = true;
            await VerifyGameFilesAsync(false);
            WriteToConsole("It is possible that we are currently updating cRPG");
            WriteToConsole("If problem persist in an hour, please contact and moderator on discord");
            IsGameUpToDate = false;
        }

        IsUpdating = false;
    }

    private async Task VerifyGameFilesAsync(bool download = true)
    {
        WriteToConsole("Verifying Game Files, Launcher May become unresponsive for the next 60 secs");
        if (GameLocation != null)
        {
            await CrpgHashMethods.VerifyGameFiles(GameLocation.InstallationPath, ProgramDataPath, HashFileName);
            await Task.Delay(500);
            if (download)
            {
                WriteToConsole("Updating Crpg Now");
                await UpdateGameFilesAsync();
            }
        }
        else
        {
            WriteToConsole("Bannerlord was not found at current location");
        }

        IsVerifying = false;
    }

    private bool WriteConfig()
    {
        return Config.WriteConfig(ProgramDataPath, ConfigFileName);
    }

    private void ApplyIpMappingSetting(string cRPGFolderPath)
    {
        try
        {
            string ipMappingPath = Path.Combine(cRPGFolderPath, "ModuleData", "crpg-ip-mapping.json");
            if (!File.Exists(ipMappingPath))
            {
                return;
            }

            string json = File.ReadAllText(ipMappingPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check if the JSON has the expected structure
            if (root.TryGetProperty("enabled", out _))
            {
                // Create a new JSON with updated enabled field
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

                writer.WriteStartObject();

                // Copy all properties, updating the enabled field
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Name == "enabled")
                    {
                        writer.WriteBoolean("enabled", Config.IpMappingEnabled);
                    }
                    else
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
                writer.Flush();

                File.WriteAllBytes(ipMappingPath, stream.ToArray());
                WriteToConsole($"Proxy setting updated: {Config.IpMappingEnabled}");
            }
        }
        catch (Exception ex)
        {
            WriteToConsole($"Failed to update Proxy setting: {ex.Message}");
        }
    }

    partial void OnGameLocationChanged(GameInstallationInfo? value)
    {
        NotifyUI();
    }

    partial void OnIsBetaChanged(bool oldValue, bool newValue)
    {
        IsGameUpToDate = false;
        NotifyUI();
    }

    partial void OnIpMappingEnabledChanged(bool oldValue, bool newValue)
    {
        Config.IpMappingEnabled = newValue;
        WriteConfig();
        NotifyUI();
    }

    partial void OnIsCrpgInstalledChanged(bool value)
    {
        OnPropertyChanged(nameof(UpdateButtoneText));
    }

    partial void OnIsGameUpToDateChanged(bool oldValue, bool newValue)
    {
        NotifyUI();
    }

    partial void OnIsUpdatingChanged(bool value)
    {
        NotifyUI();
    }

    partial void OnIsVerifyingChanged(bool value)
    {
        NotifyUI();
    }

    partial void OnSelectedPlatformChanged(Platform value)
    {
        UpdateGameLocation(value);
    }
}
