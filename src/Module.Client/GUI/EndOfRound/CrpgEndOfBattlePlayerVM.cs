using System;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.EndOfRound;

public class CrpgEndOfBattlePlayerVM : MPPlayerVM
{
    public CrpgEndOfBattlePlayerVM(MissionPeer peer, int displayedScore, int placement)
        : base(peer)
    {
        _placement = placement;
        _displayedScore = displayedScore;
        BasicCharacterObject @object = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        @object.UpdatePlayerCharacterBodyProperties(peer.Peer.BodyProperties, peer.Peer.Race, peer.Peer.IsFemale);
        @object.Age = peer.Peer.BodyProperties.Age;

        var crpgUser = peer.Peer.GetComponent<CrpgPeer>()?.User;

        if (crpgUser != null)
        {
            var equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
            MBEquipmentRoster equipmentRoster = new();
            ReflectionHelper.SetField(equipmentRoster, "_equipments", new MBList<Equipment> { equipment });
            ReflectionHelper.SetField(@object, "_equipmentRoster", equipmentRoster);
        }

        RefreshPreview(@object, peer.Peer.BodyProperties.DynamicProperties, peer.Peer.IsFemale);
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        _scoreTextObj.SetTextVariable("SCORE", _displayedScore);
        ScoreText = _scoreTextObj.ToString();
        PlacementText = TaleWorlds.Library.Common.ToRoman(_placement);
    }

    [DataSourceProperty]
    public string PlacementText
    {
        get
        {
            return _placementText;
        }
        set
        {
            if (value != _placementText)
            {
                _placementText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string ScoreText
    {
        get
        {
            return _scoreText;
        }
        set
        {
            if (value != _scoreText)
            {
                _scoreText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    private readonly int _placement;

    private readonly int _displayedScore;

    private readonly TextObject _scoreTextObj = new("{=Kvqb1lQR}{SCORE} Score");

    private string _placementText = string.Empty;

    private string _scoreText = string.Empty;
}
