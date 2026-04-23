namespace Vertigo.WheelGame.Application
{
    public interface IRandomProvider
    {
        int Range(int minInclusive, int maxExclusive);
        float Value();
    }
}
