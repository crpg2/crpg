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
            this.ViewOrderPriority = 25;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this.InitializeLayer();
            base.Mission.IsFriendlyMission = false;
            GameKeyContext category = HotKeyManager.GetCategory("ScoreboardHotKeyCategory");
            if (!base.MissionScreen.SceneLayer.Input.IsCategoryRegistered(category))
            {
                base.MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(category);
            }
            this._missionLobbyComponent = base.Mission.GetMissionBehavior<MissionLobbyComponent>();
            this._scoreboardStayDuration = MissionLobbyComponent.PostMatchWaitDuration / 2f;
            this._teamSelectComponent = base.Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
            this.RegisterEvents();
            if (this._dataSource != null)
            {
                this._dataSource.IsActive = false;
            }
        }

        public override void OnRemoveBehavior()
        {
            this.UnregisterEvents();
            this.FinalizeLayer();
            base.OnRemoveBehavior();
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            this.UnregisterEvents();
            this.FinalizeLayer();
            base.OnMissionScreenFinalize();
        }

        private void RegisterEvents()
        {
            if (base.MissionScreen != null)
            {
                base.MissionScreen.OnSpectateAgentFocusIn += this.HandleSpectateAgentFocusIn;
                base.MissionScreen.OnSpectateAgentFocusOut += this.HandleSpectateAgentFocusOut;
            }
            this._missionLobbyComponent.CurrentMultiplayerStateChanged += this.MissionLobbyComponentOnCurrentMultiplayerStateChanged;
            this._missionLobbyComponent.OnCultureSelectionRequested += this.OnCultureSelectionRequested;
            if (this._teamSelectComponent != null)
            {
                this._teamSelectComponent.OnSelectingTeam += this.OnSelectingTeam;
            }
            MissionPeer.OnTeamChanged += this.OnTeamChanged;
        }

        private void UnregisterEvents()
        {
            if (base.MissionScreen != null)
            {
                base.MissionScreen.OnSpectateAgentFocusIn -= this.HandleSpectateAgentFocusIn;
                base.MissionScreen.OnSpectateAgentFocusOut -= this.HandleSpectateAgentFocusOut;
            }
            this._missionLobbyComponent.CurrentMultiplayerStateChanged -= this.MissionLobbyComponentOnCurrentMultiplayerStateChanged;
            this._missionLobbyComponent.OnCultureSelectionRequested -= this.OnCultureSelectionRequested;
            if (this._teamSelectComponent != null)
            {
                this._teamSelectComponent.OnSelectingTeam -= this.OnSelectingTeam;
            }
            MissionPeer.OnTeamChanged -= this.OnTeamChanged;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._isMissionEnding)
            {
                if (this._scoreboardStayTimeElapsed >= this._scoreboardStayDuration)
                {
                    this.ToggleScoreboard(false);
                    return;
                }
                this._scoreboardStayTimeElapsed += dt;
            }
            this._dataSource?.Tick(dt);
            if (TaleWorlds.InputSystem.Input.IsGamepadActive)
            {
                bool flag = base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(4) || (this._gauntletLayer?.Input.IsGameKeyPressed(4) ?? false);
                if (this._isMissionEnding)
                {
                    this.ToggleScoreboard(true);
                }
                else if (flag && !base.MissionScreen.IsRadialMenuActive && !base.Mission.IsOrderMenuOpen)
                {
                    this.ToggleScoreboard(!this._dataSource?.IsActive ?? false);
                }
            }
            else
            {
                bool flag2 = base.MissionScreen.SceneLayer.Input.IsHotKeyDown("HoldShow") || (this._gauntletLayer?.Input.IsHotKeyDown("HoldShow") ?? false);
                bool isActive = this._isMissionEnding || (flag2 && !base.MissionScreen.IsRadialMenuActive && !base.Mission.IsOrderMenuOpen);
                this.ToggleScoreboard(isActive);
            }
            if (this._isActive && (base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(35) || (this._gauntletLayer?.Input.IsGameKeyPressed(35) ?? false)))
            {
                this._mouseRequstedWhileScoreboardActive = true;
            }
            bool mouseState = this._isMissionEnding || (this._isActive && this._mouseRequstedWhileScoreboardActive);
            this.SetMouseState(mouseState);
        }

        private void ToggleScoreboard(bool isActive)
        {
            if (this._isActive != isActive && _dataSource != null)
            {
                this._isActive = isActive;
                this._dataSource.IsActive = this._isActive;
                base.MissionScreen.SetCameraLockState(this._isActive);
                if (!this._isActive)
                {
                    this._mouseRequstedWhileScoreboardActive = false;
                }
                Action<bool> onScoreboardToggled = this.OnScoreboardToggled;
                if (onScoreboardToggled == null)
                {
                    return;
                }
                onScoreboardToggled(this._isActive);
            }
        }

        private void SetMouseState(bool isMouseVisible)
        {
            if (this._isMouseVisible != isMouseVisible)
            {
                this._isMouseVisible = isMouseVisible;
                if (!this._isMouseVisible)
                {
                    this._gauntletLayer?.InputRestrictions.ResetInputRestrictions();
                }
                else
                {
                    this._gauntletLayer?.InputRestrictions.SetInputRestrictions(this._isMouseVisible, InputUsageMask.Mouse);
                }
                CrpgMissionScoreboardVM? dataSource = this._dataSource;
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
                this._dataSource?.DecreaseSpectatorCount(component);
            }
        }

        private void HandleSpectateAgentFocusIn(Agent followedAgent)
        {
            if (followedAgent.MissionPeer != null)
            {
                MissionPeer component = followedAgent.MissionPeer.GetComponent<MissionPeer>();
                this._dataSource?.IncreaseSpectatorCount(component);
            }
        }

        private void MissionLobbyComponentOnCurrentMultiplayerStateChanged(MissionLobbyComponent.MultiplayerGameState newState)
        {
            this._isMissionEnding = (newState == MissionLobbyComponent.MultiplayerGameState.Ending);
        }

        private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
        {
            if (peer.IsMine)
            {
                this.FinalizeLayer();
                this.InitializeLayer();
            }
        }

        private void FinalizeLayer()
        {
            if (this._dataSource != null)
            {
                this._dataSource.OnFinalize();
            }
            if (this._gauntletLayer != null)
            {
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
            }
            this._gauntletLayer = null;
            this._dataSource = null;
            this._isActive = false;
        }

        private void InitializeLayer()
        {
            this._dataSource = new CrpgMissionScoreboardVM(this._isSingleTeam, base.Mission);
            this._gauntletLayer = new GauntletLayer("CrpgMultiplayerScoreboard", this.ViewOrderPriority, false);
            this._gauntletLayer.LoadMovie("CrpgMultiplayerScoreboard", this._dataSource);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this._dataSource.IsActive = this._isActive;
        }

        private void OnSelectingTeam(List<Team> disableTeams)
        {
            this.ToggleScoreboard(false);
        }

        private void OnCultureSelectionRequested()
        {
            this.ToggleScoreboard(false);
        }

        private GauntletLayer? _gauntletLayer = default!;

        private CrpgMissionScoreboardVM? _dataSource;

        private bool _isSingleTeam;

        private bool _isActive;

        private bool _isMissionEnding;

        private bool _mouseRequstedWhileScoreboardActive;

        private bool _isMouseVisible;

        private MissionLobbyComponent _missionLobbyComponent = default!;

        private MultiplayerTeamSelectComponent _teamSelectComponent = default!;

        public Action<bool> OnScoreboardToggled = default!;

        private float _scoreboardStayDuration;

        private float _scoreboardStayTimeElapsed;
    }
}
