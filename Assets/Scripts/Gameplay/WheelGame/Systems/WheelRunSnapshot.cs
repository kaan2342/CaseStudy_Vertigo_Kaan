using System.Collections.Generic;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public readonly struct WheelRunSnapshot
    {
        public int CurrentZone { get; }
        public ZoneType CurrentZoneType { get; }
        public int SafeZoneInterval { get; }
        public int SuperZoneInterval { get; }
        public bool IsSpinning { get; }
        public bool IsAwaitingReviveDecision { get; }
        public bool CanSpin { get; }
        public bool CanLeave { get; }
        public bool CanRevive { get; }
        public int CurrentReviveGoldCost { get; }
        public int OwnedCash { get; }
        public int OwnedGold { get; }
        public IReadOnlyList<RuntimeWheelSlice> CurrentWheel { get; }
        public IReadOnlyDictionary<string, int> PendingRewards { get; }
        public IReadOnlyDictionary<string, int> LifetimeRewards { get; }
        public bool HasLastResolvedSlice { get; }
        public RuntimeWheelSlice LastResolvedSlice { get; }

        public WheelRunSnapshot(
            int currentZone,
            ZoneType currentZoneType,
            int safeZoneInterval,
            int superZoneInterval,
            bool isSpinning,
            bool isAwaitingReviveDecision,
            bool canSpin,
            bool canLeave,
            bool canRevive,
            int currentReviveGoldCost,
            int ownedCash,
            int ownedGold,
            IReadOnlyList<RuntimeWheelSlice> currentWheel,
            IReadOnlyDictionary<string, int> pendingRewards,
            IReadOnlyDictionary<string, int> lifetimeRewards,
            bool hasLastResolvedSlice,
            RuntimeWheelSlice lastResolvedSlice)
        {
            CurrentZone = currentZone;
            CurrentZoneType = currentZoneType;
            SafeZoneInterval = safeZoneInterval;
            SuperZoneInterval = superZoneInterval;
            IsSpinning = isSpinning;
            IsAwaitingReviveDecision = isAwaitingReviveDecision;
            CanSpin = canSpin;
            CanLeave = canLeave;
            CanRevive = canRevive;
            CurrentReviveGoldCost = currentReviveGoldCost;
            OwnedCash = ownedCash;
            OwnedGold = ownedGold;
            CurrentWheel = WheelReadOnlyData.CloneWheel(currentWheel);
            PendingRewards = WheelReadOnlyData.CloneRewards(pendingRewards);
            LifetimeRewards = WheelReadOnlyData.CloneRewards(lifetimeRewards);
            HasLastResolvedSlice = hasLastResolvedSlice;
            LastResolvedSlice = lastResolvedSlice;
        }
    }
}
