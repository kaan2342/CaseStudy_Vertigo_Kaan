using UnityEngine;

namespace Vertigo.WheelGame.Application
{
    //Default used class for randomization logic
    public sealed class UnityRandomProvider : IRandomProvider
    {
        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }

        public float Value()
        {
            return Random.value;
        }
    }
}
