using System;

namespace Vertigo.WheelGame.Application
{
    //An example class to show we can use different random logics if needed
    public sealed class DeterministicRandomProvider : IRandomProvider
    {
        private readonly Random random;

        public DeterministicRandomProvider(int seed)
        {
            random = new Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }

        public float Value()
        {
            return (float)random.NextDouble();
        }
    }
}
