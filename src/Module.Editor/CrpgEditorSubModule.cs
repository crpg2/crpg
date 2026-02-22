using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module;

internal class CrpgEditorSubModule : MBSubModuleBase
{
    static CrpgEditorSubModule()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Debug.Print(args.ExceptionObject.ToString(), color: Debug.DebugColor.Red);
    }

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
    }

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);
    }

    protected override void OnApplicationTick(float delta)
    {
        base.OnApplicationTick(delta);
    }
}
