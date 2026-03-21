using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.HotConstants;

internal class HotConstantsClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UpdateHotConstant>(HandleUpdateHotConstant);
    }

    private void HandleUpdateHotConstant(UpdateHotConstant message)
    {
        if (HotConstant.TryGet(message.Id, out var constant))
        {
            constant!.Update(message.NewValue);
        }

        InformationManager.DisplayMessage(new InformationMessage($"Changed constant with id '{message.Id}' from {message.OldValue} to {message.NewValue}"));
    }
}
