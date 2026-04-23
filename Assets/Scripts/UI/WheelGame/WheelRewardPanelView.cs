using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Application;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelRewardPanelView : MonoBehaviour
    {
        [SerializeField] private Button ui_button_leave;
        [SerializeField] private WheelRewardRowContainerView ui_reward_row_container_view;

        private bool isBound;

        public event Action LeavePressed;

        private void Awake()
        {
            if (ui_button_leave == null || ui_reward_row_container_view == null)
            {
                Debug.LogError("WheelRewardPanelView is missing one or more required serialized references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!enabled || isBound || ui_button_leave == null)
            {
                return;
            }

            ui_button_leave.onClick.AddListener(HandleLeavePressed);
            isBound = true;
        }

        private void OnDisable()
        {
            if (!isBound || ui_button_leave == null)
            {
                return;
            }

            ui_button_leave.onClick.RemoveListener(HandleLeavePressed);
            isBound = false;
        }

        public void Render(WheelRunSnapshot snapshot, WheelSpinner spinner)
        {
            if (ui_button_leave != null)
            {
                ui_button_leave.gameObject.SetActive(true);
                ui_button_leave.interactable = snapshot.CanLeave;
            }

            if (ui_reward_row_container_view != null)
            {
                ui_reward_row_container_view.RenderRewards(snapshot.PendingRewards, spinner);
            }
        }

        public Vector2 ResolveRewardTargetPosition(RectTransform overlay, string rewardId)
        {
            if (ui_reward_row_container_view != null)
            {
                return ui_reward_row_container_view.ResolveRewardTargetPosition(overlay, rewardId);
            }

            return Vector2.zero;
        }

        private void HandleLeavePressed()
        {
            LeavePressed?.Invoke();
        }
    }
}
