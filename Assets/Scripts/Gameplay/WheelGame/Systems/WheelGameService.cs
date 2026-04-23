using System;
using System.Collections.Generic;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Application
{
    public sealed class WheelGameService
    {
        private readonly WheelGameConfigSO config;
        private readonly WheelComposer wheelComposer;
        private readonly IRandomProvider randomProvider;
        private readonly PlayerPrefsWheelRewardPersistence rewardPersistence = new PlayerPrefsWheelRewardPersistence();

        private readonly Dictionary<string, int> pendingRewards = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> lifetimeRewards = new Dictionary<string, int>(StringComparer.Ordinal);

        private List<RuntimeWheelSlice> currentWheel = new List<RuntimeWheelSlice>();
        private int currentZone = 1;
        private ZoneType currentZoneType = ZoneType.Normal;
        private int currentReviveGoldCost;
        private bool isSpinning;
        private bool isAwaitingReviveDecision;
        private bool hasPlannedSpin;
        private SpinPlan plannedSpin;
        private bool hasLastResolvedSlice;
        private RuntimeWheelSlice lastResolvedSlice;

        public event Action StateChanged;

        public WheelGameService(IRandomProvider randomProvider = null)
        {
            config = WheelGameConfigSO.Instance;
            this.randomProvider = randomProvider ?? new UnityRandomProvider();
            wheelComposer = new WheelComposer();

            LoadOrSeedLifetimeRewards();
            RestartRunInternal(clearLifetimeRewards: false, invokeEvent: false);
        }

        public bool CanSpin => !isSpinning && !isAwaitingReviveDecision;

        public bool CanLeave => !isAwaitingReviveDecision &&
                                ZoneRules.CanLeave(isSpinning, HasClaimableRewards);

        public bool CanRevive => isAwaitingReviveDecision && GetRewardCount(lifetimeRewards, config.GoldRewardId) >= currentReviveGoldCost;

        public bool CanGiveUp => isAwaitingReviveDecision;

        public WheelRunSnapshot Snapshot => BuildSnapshot();

        private bool HasClaimableRewards => pendingRewards.Count > 0;

        public SpinPlan PlanSpin()
        {
            if (!CanSpin)
            {
                throw new InvalidOperationException("Spin is not allowed in current state.");
            }

            var landedIndex = WeightedWheelPicker.PickIndex(currentWheel, randomProvider);
            var landedSlice = currentWheel[landedIndex];

            plannedSpin = new SpinPlan(landedIndex, currentWheel.Count, landedSlice);
            hasPlannedSpin = true;
            isSpinning = true;
            RaiseStateChanged();

            return plannedSpin;
        }

        public SpinResolution ResolvePlannedSpin()
        {
            if (!hasPlannedSpin || !isSpinning)
            {
                throw new InvalidOperationException("There is no planned spin to resolve.");
            }

            var resolvedSlice = plannedSpin.LandedSlice;
            var landedZone = currentZone;
            var landedZoneType = currentZoneType;

            isSpinning = false;
            hasPlannedSpin = false;
            plannedSpin = default;
            hasLastResolvedSlice = true;
            lastResolvedSlice = resolvedSlice;

            var hitBomb = resolvedSlice.IsBomb;

            if (hitBomb)
            {
                isAwaitingReviveDecision = true;
            }
            else
            {
                AddReward(pendingRewards, resolvedSlice.RewardId, resolvedSlice.Amount);
                AdvanceZone();
            }

            var resolution = new SpinResolution(hitBomb, resolvedSlice);

            RaiseStateChanged();
            return resolution;
        }

        public void CancelPlannedSpin()
        {
            if (!hasPlannedSpin && !isSpinning)
            {
                return;
            }

            isSpinning = false;
            hasPlannedSpin = false;
            plannedSpin = default;
            RaiseStateChanged();
        }

        public void ReviveRun()
        {
            if (!isAwaitingReviveDecision)
            {
                throw new InvalidOperationException("Revive is not allowed in current state.");
            }

            if (!TrySpendReward(lifetimeRewards, config.GoldRewardId, currentReviveGoldCost))
            {
                throw new InvalidOperationException("Not enough gold to revive.");
            }

            isAwaitingReviveDecision = false;
            currentReviveGoldCost = ComputeNextReviveGoldCost(currentReviveGoldCost, config.ReviveGoldCostGrowthMultiplier);
            PersistLifetimeRewards();
            RaiseStateChanged();
        }

        public void GiveUp()
        {
            if (!CanGiveUp)
            {
                throw new InvalidOperationException("Give up is only allowed after a failed run state.");
            }

            RestartRunInternal(clearLifetimeRewards: false, invokeEvent: false);
            RaiseStateChanged();
        }

        public void CashOut()
        {
            if (!CanLeave)
            {
                throw new InvalidOperationException("Cash out is only allowed when you have claimable rewards and the wheel is not spinning.");
            }

            AddRewards(lifetimeRewards, pendingRewards);
            PersistLifetimeRewards();
            RestartRunInternal(clearLifetimeRewards: false, invokeEvent: false);
            RaiseStateChanged();
        }

        public void RestartRun(bool clearLifetimeRewards = false)
        {
            RestartRunInternal(clearLifetimeRewards, invokeEvent: true);
        }

        private void RestartRunInternal(bool clearLifetimeRewards, bool invokeEvent)
        {
            if (clearLifetimeRewards)
            {
                lifetimeRewards.Clear();
                SeedStartingInventory(lifetimeRewards);
                PersistLifetimeRewards();
            }

            pendingRewards.Clear();
            currentZone = 1;
            currentReviveGoldCost = config.ReviveGoldCost;
            isSpinning = false;
            isAwaitingReviveDecision = false;
            hasPlannedSpin = false;
            plannedSpin = default;
            hasLastResolvedSlice = false;
            lastResolvedSlice = default;
            RebuildWheelForCurrentZone();

            if (invokeEvent)
            {
                RaiseStateChanged();
            }
        }

        private void AdvanceZone()
        {
            currentZone += 1;
            RebuildWheelForCurrentZone();
        }

        private void RebuildWheelForCurrentZone()
        {
            currentZoneType = ZoneRules.GetZoneType(currentZone, config.SafeZoneInterval, config.SuperZoneInterval);
            currentWheel = wheelComposer.ComposeForZone(currentZoneType);
        }

        private WheelRunSnapshot BuildSnapshot()
        {
            return new WheelRunSnapshot(
                currentZone,
                currentZoneType,
                config.SafeZoneInterval,
                config.SuperZoneInterval,
                isSpinning,
                isAwaitingReviveDecision,
                CanSpin,
                CanLeave,
                CanRevive,
                currentReviveGoldCost,
                GetRewardCount(lifetimeRewards, config.CashRewardId),
                GetRewardCount(lifetimeRewards, config.GoldRewardId),
                currentWheel,
                pendingRewards,
                lifetimeRewards,
                hasLastResolvedSlice,
                lastResolvedSlice);
        }

        private static void AddReward(Dictionary<string, int> rewards, string rewardId, int amount)
        {
            var key = string.IsNullOrWhiteSpace(rewardId) ? "unknown_reward" : rewardId;
            var value = Math.Max(0, amount);

            if (rewards.TryGetValue(key, out var existing))
            {
                rewards[key] = existing + value;
            }
            else
            {
                rewards[key] = value;
            }
        }

        private static void AddRewards(
            Dictionary<string, int> target,
            IReadOnlyDictionary<string, int> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var pair in source)
            {
                AddReward(target, pair.Key, pair.Value);
            }
        }

        private void SeedStartingInventory(Dictionary<string, int> rewards)
        {
            AddReward(rewards, config.CashRewardId, config.StartingCash);
            AddReward(rewards, config.GoldRewardId, config.StartingGold);
        }

        private void LoadOrSeedLifetimeRewards()
        {
            lifetimeRewards.Clear();

            if (!rewardPersistence.TryLoadLifetimeRewards(out var loadedRewards))
            {
                SeedStartingInventory(lifetimeRewards);
                PersistLifetimeRewards();
                return;
            }

            AddRewards(lifetimeRewards, loadedRewards);
        }

        private static int GetRewardCount(Dictionary<string, int> rewards, string rewardId)
        {
            if (rewards == null || string.IsNullOrWhiteSpace(rewardId))
            {
                return 0;
            }

            return rewards.TryGetValue(rewardId, out var value) ? Math.Max(0, value) : 0;
        }

        private static bool TrySpendReward(Dictionary<string, int> rewards, string rewardId, int amount)
        {
            if (rewards == null || string.IsNullOrWhiteSpace(rewardId))
            {
                return false;
            }

            var clampedAmount = Math.Max(0, amount);
            var key = rewardId;
            var currentAmount = GetRewardCount(rewards, key);
            if (currentAmount < clampedAmount)
            {
                return false;
            }

            rewards[key] = currentAmount - clampedAmount;
            return true;
        }

        private static int ComputeNextReviveGoldCost(int currentCost, float multiplier)
        {
            return Math.Max(1, (int)Math.Ceiling(Math.Max(1, currentCost) * Math.Max(1f, multiplier)));
        }

        private void PersistLifetimeRewards()
        {
            rewardPersistence.SaveLifetimeRewards(lifetimeRewards);
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
