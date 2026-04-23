using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public readonly struct SpinPlan
    {
        public int LandedIndex { get; }
        public int SliceCount { get; }
        public RuntimeWheelSlice LandedSlice { get; }

        public SpinPlan(int landedIndex, int sliceCount, RuntimeWheelSlice landedSlice)
        {
            LandedIndex = landedIndex;
            SliceCount = sliceCount;
            LandedSlice = landedSlice;
        }
    }
}
