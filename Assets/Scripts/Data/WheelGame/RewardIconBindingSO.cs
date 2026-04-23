using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelGame.Config
{
    [Serializable]
    public struct RewardIconEntry
    {
        [SerializeField] private string rewardId;
        [SerializeField] private Sprite sprite;

        public string RewardId => rewardId;
        public Sprite Sprite => sprite;
    }
    
    [CreateAssetMenu(fileName = "so_reward_icon_bindings", menuName = "Vertigo/Wheel Game/Reward Icon Bindings")]
    public class RewardIconBindingSO : ScriptableObject
    {
        [SerializeField] private RewardIconEntry[] rewardIconBindings = Array.Empty<RewardIconEntry>();
        [NonSerialized] private Dictionary<string, RewardIconEntry> lookupByRewardId;

        public bool TryGetBinding(string rewardId, out RewardIconEntry binding)
        {
            EnsureLookup();

            if (!string.IsNullOrWhiteSpace(rewardId) &&
                lookupByRewardId != null &&
                lookupByRewardId.TryGetValue(rewardId.Trim(), out binding))
            {
                return true;
            }

            binding = default;
            return false;
        }
        
        private void EnsureLookup()
        {
            if (lookupByRewardId != null)
            {
                return;
            }

            lookupByRewardId = new Dictionary<string, RewardIconEntry>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rewardIconBindings.Length; i++)
            {
                var rewardId = rewardIconBindings[i].RewardId;
                if (string.IsNullOrWhiteSpace(rewardId))
                {
                    continue;
                }

                lookupByRewardId[rewardId.Trim()] = rewardIconBindings[i];
            }
        }

        private void OnValidate()
        {
            lookupByRewardId = null;
        }
    }
}
