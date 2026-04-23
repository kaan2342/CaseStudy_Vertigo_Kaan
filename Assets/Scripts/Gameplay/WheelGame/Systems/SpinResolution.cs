using System.Collections.Generic;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public readonly struct SpinResolution
    {
        public bool HitBomb { get; }
        public RuntimeWheelSlice LandedSlice { get; }

        public SpinResolution(bool hitBomb, RuntimeWheelSlice landedSlice)
        {
            HitBomb = hitBomb;
            LandedSlice = landedSlice;
        }
    }
}
