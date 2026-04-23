using System;
using System.Collections.Generic;
using MRGameCore.Utils;
using TMPro;
using UnityEngine;
using Vertigo.WheelGame.Application;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelRewardRowContainerView : MonoBehaviour
    {
        [SerializeField] private RectTransform ui_group_reward_entries;
        [SerializeField] private TMP_Text ui_text_rewards_empty_value;
        [SerializeField] private WheelRewardRowView ui_prefab_reward_row;
        [Min(0)]
        [SerializeField] private int prewarmRowCount = 6;

        private readonly List<WheelRewardRowView> activeRows = new List<WheelRewardRowView>(8);
        private readonly List<KeyValuePair<string, int>> orderedRewardsBuffer = new List<KeyValuePair<string, int>>(8);
        private readonly Dictionary<string, WheelRewardRowView> visibleRewardRowsByRewardId = new Dictionary<string, WheelRewardRowView>(StringComparer.OrdinalIgnoreCase);

        private ComponentPrefabPool<WheelRewardRowView> rewardRowPool;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            EnsureRewardRowPool();
        }

        public void RenderRewards(IReadOnlyDictionary<string, int> rewards, WheelSpinner spinner)
        {
            if (ui_group_reward_entries == null)
            {
                return;
            }

            visibleRewardRowsByRewardId.Clear();
            BuildOrderedRewardsBuffer(rewards);

            if (ui_text_rewards_empty_value != null)
            {
                ui_text_rewards_empty_value.gameObject.SetActive(orderedRewardsBuffer.Count == 0);
            }

            EnsureActiveRowCount(orderedRewardsBuffer.Count);

            for (var i = 0; i < orderedRewardsBuffer.Count; i++)
            {
                var row = activeRows[i];
                var pair = orderedRewardsBuffer[i];

                if (row == null)
                {
                    continue;
                }

                row.transform.SetAsLastSibling();
                row.Apply(pair.Key, pair.Value, spinner);
                visibleRewardRowsByRewardId[pair.Key] = row;
            }
        }

        public Vector2 ResolveRewardTargetPosition(RectTransform overlay, string rewardId)
        {
            if (!string.IsNullOrWhiteSpace(rewardId) &&
                visibleRewardRowsByRewardId.TryGetValue(rewardId, out var row) &&
                row != null &&
                row.IconRect != null)
            {
                return WheelViewUtility.ResolveOverlayPosition(overlay, row.IconRect, Vector2.zero);
            }

            return WheelViewUtility.ResolveOverlayPosition(overlay, ui_group_reward_entries, new Vector2(40f, -40f));
        }

        private void BuildOrderedRewardsBuffer(IReadOnlyDictionary<string, int> rewards)
        {
            orderedRewardsBuffer.Clear();
            if (rewards != null)
            {
                foreach (var pair in rewards)
                {
                    if (pair.Value > 0)
                    {
                        orderedRewardsBuffer.Add(pair);
                    }
                }
            }

            orderedRewardsBuffer.Sort((left, right) =>
            {
                var byAmount = right.Value.CompareTo(left.Value);
                return byAmount != 0
                    ? byAmount
                    : string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
            });
        }

        private void EnsureActiveRowCount(int targetCount)
        {
            var pool = EnsureRewardRowPool();
            if (pool == null)
            {
                return;
            }

            while (activeRows.Count < targetCount)
            {
                var row = pool.Get(ui_group_reward_entries);
                row.transform.SetAsLastSibling();
                activeRows.Add(row);
            }

            while (activeRows.Count > targetCount)
            {
                var lastIndex = activeRows.Count - 1;
                var row = activeRows[lastIndex];
                activeRows.RemoveAt(lastIndex);
                pool.Release(row);
            }
        }

        private ComponentPrefabPool<WheelRewardRowView> EnsureRewardRowPool()
        {
            if (ui_group_reward_entries == null || ui_prefab_reward_row == null)
            {
                return null;
            }

            if (rewardRowPool == null || rewardRowPool.Prefab != ui_prefab_reward_row)
            {
                rewardRowPool = new ComponentPrefabPool<WheelRewardRowView>(
                    ui_prefab_reward_row,
                    ui_group_reward_entries);
                rewardRowPool.Prewarm(Mathf.Max(0, prewarmRowCount));
            }

            return rewardRowPool;
        }

        private bool ValidateReferences()
        {
            var hasMissingReference =
                ui_group_reward_entries == null ||
                ui_text_rewards_empty_value == null ||
                ui_prefab_reward_row == null;

            if (!hasMissingReference)
            {
                return true;
            }

            Debug.LogError("WheelRewardRowContainerView is missing one or more required serialized references.", this);
            return false;
        }
    }
}
