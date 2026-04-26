using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TaleWorlds.Library;

namespace Crpg.Module.Client.HarmonyPatches;

/// <summary>
/// Configuration for IP address mapping.
/// </summary>
public class IpMappingConfig
{
    [JsonProperty("mappings")]
    public List<IpMapping> Mappings { get; set; } = new();

    [JsonProperty("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Single IP mapping entry.
/// </summary>
public class IpMapping
{
    [JsonProperty("originalAddress")]
    public string OriginalAddress { get; set; } = string.Empty;

    [JsonProperty("originalPort")]
    public int OriginalPort { get; set; }

    [JsonProperty("newAddress")]
    public string NewAddress { get; set; } = string.Empty;

    [JsonProperty("newPort")]
    public int NewPort { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Manages IP address mappings for cRPG servers.
/// </summary>
public static class CrpgIpMapping
{
    private static readonly object _lock = new();
    private static IpMappingConfig? _config;

    /// <summary>
    /// Tries to get a mapping for the given address and port.
    /// </summary>
    public static bool TryGetMapping(string address, int port, out string newAddress, out int newPort)
    {
        newAddress = address;
        newPort = port;

        var config = GetConfig();
        if (config == null || !config.Enabled || config.Mappings.Count == 0)
        {
            return false;
        }

        // Normalize address for comparison
        string normalizedAddress = address.ToLowerInvariant().Trim();

        var mapping = config.Mappings.FirstOrDefault(m =>
            m.OriginalAddress.ToLowerInvariant() == normalizedAddress &&
            m.OriginalPort == port);

        if (mapping == null)
        {
            return false;
        }

        newAddress = mapping.NewAddress;
        newPort = mapping.NewPort;

        return true;
    }

    private static IpMappingConfig? GetConfig()
    {
        lock (_lock)
        {
            if (_config != null)
            {
                return _config;
            }

            try
            {
                string modulePath = TaleWorlds.ModuleManager.ModuleHelper.GetModuleFullPath("cRPG");
                string configPath = Path.Combine(modulePath, "ModuleData", "crpg-ip-mapping.json");

                if (!File.Exists(configPath))
                {
                    Debug.Print("[cRPG Proxy] Configuration file not found: " + configPath);
                    _config = new IpMappingConfig();
                    return _config;
                }

                string json = File.ReadAllText(configPath);
                _config = JsonConvert.DeserializeObject<IpMappingConfig>(json);

                if (_config == null)
                {
                    Debug.Print("[cRPG Proxy] Failed to deserialize configuration");
                    _config = new IpMappingConfig();
                }

                Debug.Print($"[cRPG Proxy] Loaded {_config.Mappings.Count} mapping(s), enabled: {_config.Enabled}");
                return _config;
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[cRPG Proxy] Error loading configuration: {ex.Message}");
                _config = new IpMappingConfig();
                return _config;
            }
        }
    }
}
