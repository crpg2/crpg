using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.GUI.EndOfRound;

public class CrpgEndOfBattleVM : ViewModel
{
    private readonly MissionMultiplayerGameModeBaseClient _gameMode;

    private readonly float _activeDelay;

    private bool _isBattleEnded;

    private float _activateTimeElapsed;

    private bool _isEnabled;

    private bool _hasFirstPlace;

    private bool _hasSecondPlace;

    private bool _hasThirdPlace;

    private string _titleText = string.Empty;

    private string _descriptionText = string.Empty;

    private CrpgEndOfBattlePlayerVM _firstPlacePlayer = null!;

    private CrpgEndOfBattlePlayerVM _secondPlacePlayer = null!;

    private CrpgEndOfBattlePlayerVM _thirdPlacePlayer = null!;

    public CrpgEndOfBattleVM()
    {
        _activeDelay = MissionLobbyComponent.PostMatchWaitDuration / 2f;
        _gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        TitleText = new TextObject("{=GPfkMajw}Battle Ended").ToString();
        DescriptionText = new TextObject("{=ADPaaX8R}Best Players of This Battle").ToString();
    }

    public void OnTick(float dt)
    {
        if (_isBattleEnded)
        {
            _activateTimeElapsed += dt;
            if (_activateTimeElapsed >= _activeDelay)
            {
                _isBattleEnded = false;
                OnEnabled();
            }
        }
    }

    public void OnBattleEnded()
    {
        _isBattleEnded = true;
    }

    private int GetPeerScore(MissionPeer peer)
    {
        if (peer == null)
        {
            return 0;
        }

        if (_gameMode.GameType != MultiplayerGameType.Duel)
        {
            return peer.Score;
        }

        DuelMissionRepresentative component = peer.GetComponent<DuelMissionRepresentative>();
        if (component == null)
        {
            return 0;
        }

        return component.Score;
    }

    private void OnEnabled()
    {
        MissionScoreboardComponent missionBehavior = Mission.Current.GetMissionBehavior<MissionScoreboardComponent>();
        List<MissionPeer> list = new();
        foreach (MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide in missionBehavior.Sides.Where(s => s != null && s.Side != BattleSideEnum.None))
        {
            foreach (MissionPeer item in missionScoreboardSide.Players)
            {
                list.Add(item);
            }
        }

        list.Sort((p1, p2) => GetPeerScore(p2).CompareTo(GetPeerScore(p1)));
        if (list.Count > 0)
        {
            HasFirstPlace = true;
            MissionPeer peer = list[0];
            FirstPlacePlayer = new CrpgEndOfBattlePlayerVM(peer, GetPeerScore(peer), 1);
        }

        if (list.Count > 1)
        {
            HasSecondPlace = true;
            MissionPeer peer2 = list[1];
            SecondPlacePlayer = new CrpgEndOfBattlePlayerVM(peer2, GetPeerScore(peer2), 2);
        }

        if (list.Count > 2)
        {
            HasThirdPlace = true;
            MissionPeer peer3 = list[2];
            ThirdPlacePlayer = new CrpgEndOfBattlePlayerVM(peer3, GetPeerScore(peer3), 3);
        }

        IsEnabled = true;
    }

    [DataSourceProperty]
    public bool IsEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (value != _isEnabled)
            {
                _isEnabled = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool HasFirstPlace
    {
        get
        {
            return _hasFirstPlace;
        }
        set
        {
            if (value != _hasFirstPlace)
            {
                _hasFirstPlace = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool HasSecondPlace
    {
        get
        {
            return _hasSecondPlace;
        }
        set
        {
            if (value != _hasSecondPlace)
            {
                _hasSecondPlace = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool HasThirdPlace
    {
        get
        {
            return _hasThirdPlace;
        }
        set
        {
            if (value != _hasThirdPlace)
            {
                _hasThirdPlace = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string TitleText
    {
        get
        {
            return _titleText;
        }
        set
        {
            if (value != _titleText)
            {
                _titleText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string DescriptionText
    {
        get
        {
            return _descriptionText;
        }
        set
        {
            if (value != _descriptionText)
            {
                _descriptionText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public CrpgEndOfBattlePlayerVM FirstPlacePlayer
    {
        get
        {
            return _firstPlacePlayer;
        }
        set
        {
            if (value != _firstPlacePlayer)
            {
                _firstPlacePlayer = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public CrpgEndOfBattlePlayerVM SecondPlacePlayer
    {
        get
        {
            return _secondPlacePlayer;
        }
        set
        {
            if (value != _secondPlacePlayer)
            {
                _secondPlacePlayer = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public CrpgEndOfBattlePlayerVM ThirdPlacePlayer
    {
        get
        {
            return _thirdPlacePlayer;
        }
        set
        {
            if (value != _thirdPlacePlayer)
            {
                _thirdPlacePlayer = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}
