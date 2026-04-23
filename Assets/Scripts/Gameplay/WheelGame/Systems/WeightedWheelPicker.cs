using System;
using System.Collections.Generic;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public static class WeightedWheelPicker
    {
        public static int PickIndex(IReadOnlyList<RuntimeWheelSlice> slices, IRandomProvider randomProvider)
        {
            var totalWeight = 0;
            for (var i = 0; i < slices.Count; i++)
            {
                totalWeight += Math.Max(1, slices[i].Weight);
            }

            var roll = randomProvider.Range(0, totalWeight);
            var accumulated = 0;

            for (var i = 0; i < slices.Count; i++)
            {
                accumulated += Math.Max(1, slices[i].Weight);
                if (roll < accumulated)
                {
                    return i;
                }
            }

            return slices.Count - 1;
        }
    }
}
