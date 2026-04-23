using System;
using System.Collections.Generic;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public sealed class WheelComposer
    {
        private readonly WheelGameConfigSO config;

        public WheelComposer()
        {
            config = WheelGameConfigSO.Instance ?? throw new InvalidOperationException(
                "WheelComposer requires WheelGameConfigSO.Instance to be available.");
        }

        public List<RuntimeWheelSlice> ComposeForZone(ZoneType zoneType)
        {
            var sourceWheel = config.GetWheelForZone(zoneType);
            if (sourceWheel == null || sourceWheel.Slices == null || sourceWheel.Slices.Count == 0)
            {
                throw new InvalidOperationException("Wheel definition is missing slices for zone type " + zoneType + ".");
            }

            var slices = new List<RuntimeWheelSlice>(sourceWheel.Slices.Count);
            var bombCount = 0;

            for (var i = 0; i < sourceWheel.Slices.Count; i++)
            {
                var source = sourceWheel.Slices[i];
                if (source == null)
                {
                    continue;
                }

                if (source.SliceType == WheelSliceType.Bomb)
                {
                    if (zoneType != ZoneType.Normal)
                    {
                        continue;
                    }

                    bombCount++;
                    slices.Add(new RuntimeWheelSlice(
                        WheelSliceType.Bomb,
                        string.Empty,
                        0,
                        source.DisplayLabel,
                        source.Weight));
                    continue;
                }

                var rewardId = string.IsNullOrWhiteSpace(source.RewardId) ? "unknown_reward" : source.RewardId.Trim();
                var displayLabel = string.IsNullOrWhiteSpace(source.DisplayLabel) ? rewardId : source.DisplayLabel;

                slices.Add(new RuntimeWheelSlice(
                    WheelSliceType.Reward,
                    rewardId,
                    source.BaseAmount,
                    displayLabel,
                    source.Weight));
            }

            if (zoneType == ZoneType.Normal && bombCount != 1)
            {
                throw new InvalidOperationException("Normal wheel must contain exactly one bomb slice.");
            }

            if (slices.Count == 0)
            {
                throw new InvalidOperationException("Composed wheel is empty. Check wheel configuration.");
            }

            return slices;
        }
    }
}
