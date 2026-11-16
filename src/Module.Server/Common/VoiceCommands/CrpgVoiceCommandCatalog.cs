using System;

namespace Crpg.Module.Common.VoiceCommands;

internal static class CrpgVoiceCommandCatalog
{
    private static readonly string[] VoiceNames = Enum.GetNames(typeof(CrpgVoiceCommand));

    public static int Count => VoiceNames.Length;

    public static bool IsValid(int commandId)
    {
        return commandId >= 0 && commandId < Count;
    }

    public static string GetVoiceName(int commandId)
    {
        return VoiceNames[commandId];
    }
}
