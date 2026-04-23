using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    internal static class WheelReadOnlyData
    {
        private static readonly IReadOnlyDictionary<string, int> EmptyRewards =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(0, StringComparer.Ordinal));

        public static IReadOnlyList<RuntimeWheelSlice> CloneWheel(IReadOnlyList<RuntimeWheelSlice> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<RuntimeWheelSlice>();
            }

            var clone = new RuntimeWheelSlice[source.Count];
            for (var i = 0; i < source.Count; i++)
            {
                clone[i] = source[i];
            }

            return clone;
        }

        public static IReadOnlyDictionary<string, int> CloneRewards(IReadOnlyDictionary<string, int> source)
        {
            if (source == null || source.Count == 0)
            {
                return EmptyRewards;
            }

            var clone = new Dictionary<string, int>(source.Count, StringComparer.Ordinal);
            foreach (var pair in source)
            {
                var rewardId = string.IsNullOrWhiteSpace(pair.Key) ? "unknown_reward" : pair.Key;
                clone[rewardId] = Math.Max(0, pair.Value);
            }

            return new ReadOnlyDictionary<string, int>(clone);
        }
    }
}
