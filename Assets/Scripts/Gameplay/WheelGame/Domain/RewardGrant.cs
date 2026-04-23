using System;
using UnityEngine;

namespace Vertigo.WheelGame.Domain
{
    [Serializable]
    public struct RewardGrant
    {
        [SerializeField] private string rewardId;
        [SerializeField] private int amount;

        public string RewardId => rewardId;
        public int Amount => amount;

        public RewardGrant(string rewardId, int amount)
        {
            this.rewardId = string.IsNullOrWhiteSpace(rewardId) ? "unknown_reward" : rewardId.Trim();
            this.amount = Math.Max(0, amount);
        }

        public override string ToString()
        {
            return RewardId + " x" + Amount;
        }
    }
}
