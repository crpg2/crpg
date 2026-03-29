using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.User;

internal class FireWeaponSettingsCommand : ChatCommand
{
    public FireWeaponSettingsCommand(ChatCommandsComponent chatComponent)
    : base(chatComponent)
    {
        Name = "fws";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} <particle|color|radius|intensity> <value(s)>' to change your FireWeapon settings.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.Float32, ChatCommandParameterType.Float32, ChatCommandParameterType.Float32 }, ExecuteFireWeaponSettings), // /fws color <r> <g> <b>
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.Float32 }, ExecuteFireWeaponSettings),                                                     // /fws radius/intensity <value>
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.String }, ExecuteFireWeaponSettings),                                                         // /fws particle <id>
            new(new[] { ChatCommandParameterType.String }, ExecuteFireWeaponSettings),                                                                                          // /fws color (random)
        };
    }

    private static readonly Random Rng = new();
    private void ExecuteFireWeaponSettings(NetworkCommunicator fromPeer, object[] arguments)
    {
        var fwBehavior = Mission.Current.GetMissionBehavior<FireWeaponsBehaviorServer>();
        if (fwBehavior == null)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, "FireWeaponsBehaviorServer not found!");
            return;
        }

        if (arguments.Length == 0 || arguments[0] is not string settingName)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, "Usage: /fws <particle|color|radius|intensity> <value(s)>");
            return;
        }

        var data = fwBehavior.GetData(fromPeer);
        string oldValue;
        string newValue;

        switch (settingName.ToLower())
        {
            case "particle":
                string selectedParticleId;
                if (arguments.Length >= 2 && arguments[1] is string particleId)
                {
                    selectedParticleId = particleId;
                }
                else
                {
                    selectedParticleId = FireWeaponsBehaviorServer.ParticleSystemIds[Rng.Next(FireWeaponsBehaviorServer.ParticleSystemIds.Length)];
                }

                oldValue = data.ParticleSystemId;
                data.ParticleSystemId = selectedParticleId;
                newValue = data.ParticleSystemId;
                break;

            case "color":
                ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo,
                    $"args: {arguments.Length} | {string.Join(", ", arguments)}");

                Vec3 newColor;
                if (arguments.Length >= 4
                    && arguments[1] is float r
                    && arguments[2] is float g
                    && arguments[3] is float b)
                {
                    ChatComponent.ServerSendMessageToPlayer(fromPeer, "Parsed color args successfully.");
                    newColor = new Vec3(r, g, b);
                }
                else
                {
                    ChatComponent.ServerSendMessageToPlayer(fromPeer, "Random colors.");
                    var rng = Rng;
                    newColor = new Vec3(
                        (float)Rng.NextDouble(),
                        (float)Rng.NextDouble(),
                        (float)Rng.NextDouble());
                }

                oldValue = data.LightColor.ToString();
                data.LightColor = newColor;
                newValue = data.LightColor.ToString();
                break;

            case "radius":
                if (arguments.Length < 2 || arguments[1] is not float radius)
                {
                    ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, "Usage: /fws radius <value>");
                    return;
                }

                oldValue = data.LightRadius.ToString();
                data.LightRadius = radius;
                newValue = data.LightRadius.ToString();
                break;

            case "intensity":
                if (arguments.Length < 2 || arguments[1] is not float intensity)
                {
                    ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, "Usage: /fws intensity <value>");
                    return;
                }

                oldValue = data.LightIntensity.ToString();
                data.LightIntensity = intensity;
                newValue = data.LightIntensity.ToString();
                break;

            default:
                ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"Unknown setting '{settingName}'. Valid: particle, color, radius, intensity");
                return;
        }

        fwBehavior.SetData(fromPeer, data);
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"[FireWeapon] {settingName} updated. ({oldValue} → {newValue})");
    }
}
