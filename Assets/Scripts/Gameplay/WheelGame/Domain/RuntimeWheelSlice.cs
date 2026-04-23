using System;

namespace Vertigo.WheelGame.Domain
{
    public readonly struct RuntimeWheelSlice
    {
        public string RewardId { get; }
        public int Amount { get; }
        public string DisplayLabel { get; }
        public int Weight { get; }
        public bool IsBomb => SliceType == WheelSliceType.Bomb;

        private WheelSliceType SliceType { get; }
        
        public RuntimeWheelSlice(
            WheelSliceType sliceType,
            string rewardId,
            int amount,
            string displayLabel,
            int weight)
        {
            SliceType = sliceType;
            RewardId = rewardId;
            Amount = Math.Max(0, amount);
            DisplayLabel = displayLabel;
            Weight = Math.Max(1, weight);
        }
    }
}
