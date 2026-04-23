using System;
using UnityEngine;

namespace Vertigo.WheelGame.Domain
{
    [Serializable]
    public sealed class WheelSliceDefinition
    {
        [SerializeField] private WheelSliceType sliceType = WheelSliceType.Reward;
        [SerializeField] private string rewardId = "coin";
        [Min(1)]
        [SerializeField] private int baseAmount = 100;
        [Min(1)]
        [SerializeField] private int weight = 1;
        [SerializeField] private string displayLabel = "Coin";

        public WheelSliceType SliceType => sliceType;
        public string RewardId => rewardId;
        public int BaseAmount => Math.Max(1, baseAmount);
        public int Weight => Math.Max(1, weight);
        public string DisplayLabel => displayLabel;
    }
}
