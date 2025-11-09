using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Crpg.Module.Helpers;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;

namespace Crpg.Module.GUI;

[OverrideView(typeof(MissionMainAgentCheerBarkControllerView))]
public class CrpgMissionGauntletMainAgentCheerControllerView : MissionView
{
}
