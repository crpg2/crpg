using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Scoreboard
{
    [OverrideView(typeof(MissionScoreboardUIHandler))]
    public class CrpgMissionScoreboardUIHandler : MissionView
    {
        [UsedImplicitly]
        public CrpgMissionScoreboardUIHandler(bool isSingleTeam)
        {
            _isSingleTeam = isSingleTeam;
            ViewOrderPriority = 25;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            InitializeLayer();
            Mission.IsFriendlyMission = false;
            GameKeyContext category = HotKeyManager.GetCategory("ScoreboardHotKeyCategory");
            if (!MissionScreen.SceneLayer.Input.IsCategoryRegistered(category))
            {
                MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(category);
            }

            _missionLobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
            _scoreboardStayDuration = MissionLobbyComponent.PostMatchWaitDuration / 2f;
            _teamSelectComponent = Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
            RegisterEvents();
            if (_dataSource != null)
            {
                _dataSource.IsActive = false;
            }
        }

        public override void OnRemoveBehavior()
        {
            UnregisterEvents();
            FinalizeLayer();
            base.OnRemoveBehavior();
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            UnregisterEvents();
            FinalizeLayer();
            base.OnMissionScreenFinalize();
        }

        private void RegisterEvents()
        {
            if (MissionScreen != null)
            {
                MissionScreen.OnSpectateAgentFocusIn += HandleSpectateAgentFocusIn;
                MissionScreen.OnSpectateAgentFocusOut += HandleSpectateAgentFocusOut;
            }

            _missionLobbyComponent.CurrentMultiplayerStateChanged += MissionLobbyComponentOnCurrentMultiplayerStateChanged;
            _missionLobbyComponent.OnCultureSelectionRequested += OnCultureSelectionRequested;
            if (_teamSelectComponent != null)
            {
                _teamSelectComponent.OnSelectingTeam += OnSelectingTeam;
            }

            MissionPeer.OnTeamChanged += OnTeamChanged;
        }

        private void UnregisterEvents()
        {
            if (MissionScreen != null)
            {
                MissionScreen.OnSpectateAgentFocusIn -= HandleSpectateAgentFocusIn;
                MissionScreen.OnSpectateAgentFocusOut -= HandleSpectateAgentFocusOut;
            }

            _missionLobbyComponent.CurrentMultiplayerStateChanged -= MissionLobbyComponentOnCurrentMultiplayerStateChanged;
            _missionLobbyComponent.OnCultureSelectionRequested -= OnCultureSelectionRequested;
            if (_teamSelectComponent != null)
            {
                _teamSelectComponent.OnSelectingTeam -= OnSelectingTeam;
            }

            MissionPeer.OnTeamChanged -= OnTeamChanged;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (_isMissionEnding)
            {
                if (_scoreboardStayTimeElapsed >= _scoreboardStayDuration)
                {
                    ToggleScoreboard(false);
                    return;
                }

                _scoreboardStayTimeElapsed += dt;
            }

            _dataSource?.Tick(dt);
            if (TaleWorlds.InputSystem.Input.IsGamepadActive)
            {
                bool flag = MissionScreen.SceneLayer.Input.IsGameKeyPressed(4) || (_gauntletLayer?.Input.IsGameKeyPressed(4) ?? false);
                if (_isMissionEnding)
                {
                    ToggleScoreboard(true);
                }
                else if (flag && !MissionScreen.IsRadialMenuActive && !Mission.IsOrderMenuOpen)
                {
                    ToggleScoreboard(!_dataSource?.IsActive ?? false);
                }
            }
            else
            {
                bool flag2 = MissionScreen.SceneLayer.Input.IsHotKeyDown("HoldShow") || (_gauntletLayer?.Input.IsHotKeyDown("HoldShow") ?? false);
                bool isActive = _isMissionEnding || (flag2 && !MissionScreen.IsRadialMenuActive && !Mission.IsOrderMenuOpen);
                ToggleScoreboard(isActive);
            }

            if (_isActive && (MissionScreen.SceneLayer.Input.IsGameKeyPressed(35) || (_gauntletLayer?.Input.IsGameKeyPressed(35) ?? false)))
            {
                _mouseRequstedWhileScoreboardActive = true;
            }

            bool mouseState = _isMissionEnding || (_isActive && _mouseRequstedWhileScoreboardActive);
            SetMouseState(mouseState);
        }

        private void ToggleScoreboard(bool isActive)
        {
            if (_isActive != isActive && _dataSource != null)
            {
                _isActive = isActive;
                _dataSource.IsActive = _isActive;
                MissionScreen.SetCameraLockState(_isActive);
                if (!_isActive)
                {
                    _mouseRequstedWhileScoreboardActive = false;
                }

                Action<bool> onScoreboardToggled = OnScoreboardToggled;
                if (onScoreboardToggled == null)
                {
                    return;
                }

                onScoreboardToggled(_isActive);
            }
        }

        private void SetMouseState(bool isMouseVisible)
        {
            if (_isMouseVisible != isMouseVisible)
            {
                _isMouseVisible = isMouseVisible;
                if (!_isMouseVisible)
                {
                    _gauntletLayer?.InputRestrictions.ResetInputRestrictions();
                }
                else
                {
                    _gauntletLayer?.InputRestrictions.SetInputRestrictions(_isMouseVisible, InputUsageMask.Mouse);
                }

                CrpgMissionScoreboardVM? dataSource = _dataSource;
                if (dataSource == null)
                {
                    return;
                }

                dataSource.SetMouseState(isMouseVisible);
            }
        }

        private void HandleSpectateAgentFocusOut(Agent followedAgent)
        {
            if (followedAgent.MissionPeer != null)
            {
                MissionPeer component = followedAgent.MissionPeer.GetComponent<MissionPeer>();
                _dataSource?.DecreaseSpectatorCount(component);
            }
        }

        private void HandleSpectateAgentFocusIn(Agent followedAgent)
        {
            if (followedAgent.MissionPeer != null)
            {
                MissionPeer component = followedAgent.MissionPeer.GetComponent<MissionPeer>();
                _dataSource?.IncreaseSpectatorCount(component);
            }
        }

        private void MissionLobbyComponentOnCurrentMultiplayerStateChanged(MissionLobbyComponent.MultiplayerGameState newState)
        {
            _isMissionEnding = newState == MissionLobbyComponent.MultiplayerGameState.Ending;
        }

        private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
        {
            if (peer.IsMine)
            {
                FinalizeLayer();
                InitializeLayer();
            }
        }

        private void FinalizeLayer()
        {
            if (_dataSource != null)
            {
                _dataSource.OnFinalize();
            }

            if (_gauntletLayer != null)
            {
                MissionScreen.RemoveLayer(_gauntletLayer);
            }

            _gauntletLayer = null;
            _dataSource = null;
            _isActive = false;
        }

        private void InitializeLayer()
        {
            _dataSource = new CrpgMissionScoreboardVM(_isSingleTeam, Mission);
            _gauntletLayer = new GauntletLayer("CrpgMultiplayerScoreboard", ViewOrderPriority);
            _gauntletLayer.LoadMovie("CrpgMultiplayerScoreboard", _dataSource);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
            MissionScreen.AddLayer(_gauntletLayer);
            _dataSource.IsActive = _isActive;
        }

        private void OnSelectingTeam(List<Team> disableTeams)
        {
            ToggleScoreboard(false);
        }

        private void OnCultureSelectionRequested()
        {
            ToggleScoreboard(false);
        }

        private GauntletLayer? _gauntletLayer = null!;

        private CrpgMissionScoreboardVM? _dataSource;

        private bool _isSingleTeam;

        private bool _isActive;

        private bool _isMissionEnding;

        private bool _mouseRequstedWhileScoreboardActive;

        private bool _isMouseVisible;

        private MissionLobbyComponent _missionLobbyComponent = null!;

        private MultiplayerTeamSelectComponent _teamSelectComponent = null!;

        public Action<bool> OnScoreboardToggled = null!;

        private float _scoreboardStayDuration;

        private float _scoreboardStayTimeElapsed;
    }
}
