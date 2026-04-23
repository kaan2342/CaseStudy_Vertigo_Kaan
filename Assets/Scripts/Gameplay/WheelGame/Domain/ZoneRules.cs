using System;

namespace Vertigo.WheelGame.Domain
{
    public static class ZoneRules
    {
        public static ZoneType GetZoneType(int zoneIndex, int safeZoneInterval, int superZoneInterval)
        {
            if (zoneIndex % superZoneInterval == 0)
            {
                return ZoneType.Super;
            }

            if (zoneIndex % safeZoneInterval == 0)
            {
                return ZoneType.Safe;
            }

            return ZoneType.Normal;
        }

        public static bool CanLeave(bool isSpinning, bool hasClaimableRewards)
        {
            if (isSpinning)
            {
                return false;
            }

            return hasClaimableRewards;
        }
    }
}
