using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Vertigo.WheelGame.Application;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelGameView : MonoBehaviour
    {
        [Header("Hero Text")]
        [SerializeField] private TMP_Text ui_text_spin_title_value;
        [SerializeField] private TMP_Text ui_text_spin_footer_value;

        [Header("Status Text")]
        [SerializeField] private TMP_Text ui_text_pending_rewards_value;
        [SerializeField] private TMP_Text ui_text_lifetime_rewards_value;
        [SerializeField] private TMP_Text ui_text_last_spin_value;

        [Header("Wheel")]
        [SerializeField] private WheelSpinner ui_wheel_spinner;
        [SerializeField] private WheelZoneMapView ui_zone_map_view;

        [Header("Subviews")]
        [SerializeField] private WheelSpinInputView ui_spin_input_view;
        [SerializeField] private WheelRewardPanelView ui_reward_panel_view;
        [SerializeField] private WheelCurrencyHudView ui_currency_hud_view;
        [SerializeField] private WheelBombModalView ui_bomb_modal_view;
        [SerializeField] private WheelRewardPopupView ui_reward_popup_view;

        private readonly StringBuilder rewardsTextBuilder = new StringBuilder(128);
        private bool viewEventsBound;

        public event Action SpinPressed;
        public event Action LeavePressed;
        public event Action RestartPressed;
        public event Action ReviveCurrencyPressed;
        public event Action ReviveAdPressed;

        private void OnEnable()
        {
            if (viewEventsBound) return;
            
            ui_spin_input_view.SpinPressed += HandleSpinPressed;
            ui_reward_panel_view.LeavePressed += HandleLeavePressed;
            
            ui_bomb_modal_view.RestartPressed += HandleRestartPressed;
            ui_bomb_modal_view.ReviveCurrencyPressed += HandleReviveCurrencyPressed;
            ui_bomb_modal_view.ReviveAdPressed += HandleReviveAdPressed;

            viewEventsBound = true;
        }

        private void OnDisable()
        {
            if (!viewEventsBound) return;
            
            ui_spin_input_view.SpinPressed -= HandleSpinPressed;
            ui_reward_panel_view.LeavePressed -= HandleLeavePressed;
                
            ui_bomb_modal_view.RestartPressed -= HandleRestartPressed;
            ui_bomb_modal_view.ReviveCurrencyPressed -= HandleReviveCurrencyPressed;
            ui_bomb_modal_view.ReviveAdPressed -= HandleReviveAdPressed;

            viewEventsBound = false;
        }

        public void ApplyConfig()
        {
            ui_wheel_spinner.ApplyConfig();
            ui_reward_popup_view.ApplyConfig();
        }

        public void RenderSnapshot(WheelRunSnapshot snapshot)
        {
            RenderWheelPresentation(snapshot);
            RenderSharedState(snapshot);
        }

        public void RenderTransientState(WheelRunSnapshot snapshot)
        {
            RenderSharedState(snapshot);
        }

        public IEnumerator PlaySpin(SpinPlan spinPlan, float spinDurationSeconds, int extraRevolutions)
        {
            yield return ui_wheel_spinner.SpinToSlice(
                spinPlan.LandedIndex,
                spinPlan.SliceCount,
                spinDurationSeconds,
                extraRevolutions);
        }

        public IEnumerator PlayRewardEarnedSequence(RuntimeWheelSlice landedSlice)
        {
            yield return ui_reward_popup_view.PlayRewardEarnedSequence(
                landedSlice,
                ui_wheel_spinner,
                ui_reward_panel_view);
        }

        private void RenderWheelPresentation(WheelRunSnapshot snapshot)
        {
            ui_text_spin_title_value.text = BuildSpinTitle(snapshot.CurrentZoneType);
            ui_text_spin_title_value.color = ResolveZoneAccent(snapshot.CurrentZoneType);
                
            ui_text_spin_footer_value.text = BuildSpinFooter(snapshot.CurrentWheel, snapshot.CurrentZoneType);
            ui_text_spin_footer_value.color = ResolveZoneAccent(snapshot.CurrentZoneType);
                
            ui_wheel_spinner.RenderWheel(snapshot.CurrentWheel, snapshot.CurrentZoneType, snapshot.CurrentZone);
                
            ui_zone_map_view.Render(snapshot.CurrentZone, snapshot.SafeZoneInterval, snapshot.SuperZoneInterval);
        }

        private void RenderSharedState(WheelRunSnapshot snapshot)
        {
            ui_text_pending_rewards_value.text = FormatRewards(snapshot.PendingRewards);
            ui_text_lifetime_rewards_value.text = FormatRewards(snapshot.LifetimeRewards);
            ui_text_last_spin_value.text = snapshot.HasLastResolvedSlice
                ? FormatSlice(snapshot.LastResolvedSlice)
                : "-";
                
            ui_spin_input_view.Render(snapshot.CanSpin);
            ui_currency_hud_view.Render(snapshot);
            ui_reward_panel_view.Render(snapshot, ui_wheel_spinner);
            ui_bomb_modal_view.Render(snapshot);
        }

        private static string BuildSpinTitle(ZoneType zoneType) //TODO: add values in a config
        {
            if (zoneType == ZoneType.Super)
            {
                return "GOLDEN SPIN";
            }

            if (zoneType == ZoneType.Safe)
            {
                return "SILVER SPIN";
            }

            return "BRONZE SPIN";
        }

        private static Color ResolveZoneAccent(ZoneType zoneType) //TODO: add values in a config
        {
            if (zoneType == ZoneType.Super)
            {
                return new Color(1f, 0.91f, 0.18f, 1f);
            }

            if (zoneType == ZoneType.Safe)
            {
                return new Color(0.9f, 0.94f, 1f, 1f);
            }

            return new Color(1f, 0.76f, 0.22f, 1f);
        }

        private static string BuildSpinFooter(IReadOnlyList<RuntimeWheelSlice> slices, ZoneType zoneType)//TODO: get these strings from a config SO
        {
            if (zoneType == ZoneType.Super)
            {
                return "UP TO X10 REWARDS";
            }

            if (zoneType == ZoneType.Safe)
            {
                return "SAFE SILVER REWARDS";
            }

            if (slices == null || slices.Count == 0)
            {
                return "BUILD YOUR WHEEL";
            }

            RuntimeWheelSlice? bestSlice = null;
            for (var i = 0; i < slices.Count; i++)
            {
                if (slices[i].IsBomb)
                {
                    continue;
                }

                if (!bestSlice.HasValue || slices[i].Amount > bestSlice.Value.Amount)
                {
                    bestSlice = slices[i];
                }
            }

            if (!bestSlice.HasValue)
            {
                return "BOMB RISK ACTIVE";
            }

            var bestLabel = string.IsNullOrWhiteSpace(bestSlice.Value.DisplayLabel)
                ? "X" + bestSlice.Value.Amount
                : bestSlice.Value.DisplayLabel.ToUpperInvariant();

            return "UP TO " + bestLabel + " REWARDS";
        }

        private static string FormatSlice(RuntimeWheelSlice slice)
        {
            if (slice.IsBomb)
            {
                return "BOMB"; //TODO: add value in a config
            }

            var rewardName = FormatRewardName(slice.RewardId);
            var amountLabel = !string.IsNullOrWhiteSpace(slice.DisplayLabel)
                ? slice.DisplayLabel.ToUpperInvariant()
                : "X" + slice.Amount;

            if (string.IsNullOrWhiteSpace(rewardName))
            {
                return amountLabel;
            }

            return rewardName + " " + amountLabel;
        }

        private static string FormatRewardName(string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return string.Empty;
            }

            return rewardId.Replace("_", " ").ToUpperInvariant();
        }

        private string FormatRewards(IReadOnlyDictionary<string, int> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return "-";
            }

            rewardsTextBuilder.Clear();
            var first = true;
            foreach (var pair in rewards)
            {
                if (!first)
                {
                    rewardsTextBuilder.Append('\n');
                }

                rewardsTextBuilder
                    .Append(pair.Key.ToUpperInvariant())
                    .Append(" x")
                    .Append(pair.Value);
                first = false;
            }

            return rewardsTextBuilder.ToString();
        }

        private void HandleSpinPressed()
        {
            SpinPressed?.Invoke();
        }

        private void HandleLeavePressed()
        {
            LeavePressed?.Invoke();
        }

        private void HandleRestartPressed()
        {
            RestartPressed?.Invoke();
        }

        private void HandleReviveCurrencyPressed()
        {
            ReviveCurrencyPressed?.Invoke();
        }

        private void HandleReviveAdPressed()
        {
            ReviveAdPressed?.Invoke();
        }
    }
}
