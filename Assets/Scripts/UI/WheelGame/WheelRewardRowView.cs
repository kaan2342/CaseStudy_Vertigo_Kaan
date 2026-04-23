using MRGameCore.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelRewardRowView : MonoBehaviour, IPoolCallbacks
    {
        [SerializeField] private RectTransform ui_group_reward_row;
        [SerializeField] private RectTransform ui_group_reward_icon;
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TMP_Text ui_text_reward_amount_value;

        private string rewardId = string.Empty;

        public RectTransform Root => ui_group_reward_row;

        public RectTransform IconRect => ui_group_reward_icon;

        public string RewardId => rewardId;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        public void Apply(string nextRewardId, int value, WheelSpinner spinner)
        {
            rewardId = nextRewardId ?? string.Empty;

            if (ui_image_reward_icon != null && spinner != null)
            {
                ui_image_reward_icon.sprite = spinner.ResolveRewardSprite(rewardId);
                ui_image_reward_icon.preserveAspect = true;
                ui_image_reward_icon.color = Color.white;
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = value.ToString("N0");
            }
        }

        public void OnTakenFromPool()
        {
            if (ui_group_reward_row != null)
            {
                ui_group_reward_row.localScale = Vector3.one;
            }
        }

        public void OnReturnedToPool()
        {
            rewardId = string.Empty;

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = null;
                ui_image_reward_icon.color = Color.white;
            }

            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = string.Empty;
            }
        }

        private bool ValidateReferences()
        {
            var hasMissingReference =
                ui_group_reward_row == null ||
                ui_group_reward_icon == null ||
                ui_image_reward_icon == null ||
                ui_text_reward_amount_value == null;

            if (!hasMissingReference)
            {
                return true;
            }

            Debug.LogError("WheelRewardRowView is missing one or more required serialized references.", this);
            return false;
        }
    }
}
