using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class CrpgMainGuiVM : ViewModel
{
    private bool _isVisible;

    public event Action<CrpgMainGuiMissionView.CharacterEquipOpenedFromSource>? OpenCharacterEquipRequested;
    public CrpgMainGuiVM()
    {
    }

    public void OpenCharacterEquipFromButton()
    {
        OpenCharacterEquipRequested?.Invoke(CrpgMainGuiMissionView.CharacterEquipOpenedFromSource.MainGuiButton);
    }

    // MainGui visibility managed by MissionView
    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
    }
}
