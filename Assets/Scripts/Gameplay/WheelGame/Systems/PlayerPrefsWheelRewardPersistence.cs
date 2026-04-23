using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelGame.Application
{
    //TODO: Create a DataManager class to handle save and load
    public sealed class PlayerPrefsWheelRewardPersistence
    {
        private const string LifetimeRewardsInitializedPlayerPrefsKey = "Vertigo.WheelGame.LifetimeRewards.Initialized";
        private const string LifetimeRewardsPlayerPrefsKey = "Vertigo.WheelGame.LifetimeRewards";

        public bool TryLoadLifetimeRewards(out IReadOnlyDictionary<string, int> rewards)
        {
            var loadedRewards = new Dictionary<string, int>(StringComparer.Ordinal);
            rewards = loadedRewards;

            if (!PlayerPrefs.HasKey(LifetimeRewardsInitializedPlayerPrefsKey))
            {
                return false;
            }

            var json = PlayerPrefs.GetString(LifetimeRewardsPlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var saveData = JsonUtility.FromJson<LifetimeRewardsSaveData>(json);
                var rewardEntries = saveData.Rewards;
                for (var i = 0; i < rewardEntries.Length; i++)
                {
                    AddReward(loadedRewards, rewardEntries[i].RewardId, rewardEntries[i].Amount);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("PlayerPrefsWheelRewardPersistence failed to load lifetime rewards. " + exception.Message);
                loadedRewards.Clear();
                return false;
            }
        }

        public void SaveLifetimeRewards(IReadOnlyDictionary<string, int> rewards)
        {
            var rewardCount = rewards != null ? rewards.Count : 0;
            var rewardEntries = new RewardSaveEntry[rewardCount];
            var index = 0;

            if (rewards != null)
            {
                foreach (var pair in rewards)
                {
                    rewardEntries[index++] = new RewardSaveEntry(pair.Key, pair.Value);
                }
            }

            var saveData = new LifetimeRewardsSaveData(rewardEntries);
            PlayerPrefs.SetString(LifetimeRewardsPlayerPrefsKey, JsonUtility.ToJson(saveData));
            PlayerPrefs.SetInt(LifetimeRewardsInitializedPlayerPrefsKey, 1);
            PlayerPrefs.Save();
        }

        private static void AddReward(Dictionary<string, int> rewards, string rewardId, int amount)
        {
            var key = string.IsNullOrWhiteSpace(rewardId) ? "unknown_reward" : rewardId.Trim();
            var value = Math.Max(0, amount);

            if (rewards.TryGetValue(key, out var existing))
            {
                rewards[key] = existing + value;
            }
            else
            {
                rewards[key] = value;
            }
        }

        [Serializable]
        private struct LifetimeRewardsSaveData
        {
            [SerializeField] private RewardSaveEntry[] rewards;

            public LifetimeRewardsSaveData(RewardSaveEntry[] rewards)
            {
                this.rewards = rewards ?? Array.Empty<RewardSaveEntry>();
            }

            public RewardSaveEntry[] Rewards => rewards ?? Array.Empty<RewardSaveEntry>();
        }

        [Serializable]
        private struct RewardSaveEntry
        {
            [SerializeField] private string rewardId;
            [SerializeField] private int amount;

            public RewardSaveEntry(string rewardId, int amount)
            {
                this.rewardId = string.IsNullOrWhiteSpace(rewardId) ? "unknown_reward" : rewardId.Trim();
                this.amount = Math.Max(0, amount);
            }

            public string RewardId => rewardId;
            public int Amount => amount;
        }
    }
}
