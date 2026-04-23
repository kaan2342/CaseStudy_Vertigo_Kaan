using System;
using MRGameCore.Utils;
using UnityEngine;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;


[CreateAssetMenu(fileName = "so_wheel_game_config", menuName = "Vertigo/Wheel Game/Config")]
  public sealed class WheelGameConfigSO : GenericConfig<WheelGameConfigSO>
  {
      private const string ResourceAssetName = "so_wheel_game_config";

      [Header("Zone Rules")]
      [Min(1)]
      [SerializeField] private int safeZoneInterval = 5;
      [Min(1)]
      [SerializeField] private int superZoneInterval = 30;

      [Header("Player Economy")]
      [SerializeField] private string cashRewardId = "cash";
      [SerializeField] private string goldRewardId = "gold";
      [Min(0)]
      [SerializeField] private int startingCash;
      [Min(0)]
      [SerializeField] private int startingGold = 50;

      [Header("Revive Economy")]
      [Min(1)]
      [SerializeField] private int reviveGoldCost = 25;
      [Min(1f)]
      [SerializeField] private float reviveGoldCostGrowthMultiplier = 1.07f;

      [Header("Reward Popup")]
      [Min(0f)]
      [SerializeField] private float rewardPopupHoldSeconds = 1f;
      [Min(1)]
      [SerializeField] private int rewardPopupMaxBurstIcons = 50;
      [SerializeField] private Vector2 rewardPopupIconScaleRange = new Vector2(0.85f, 1.15f);

      [Header("Wheels")]
      [SerializeField] private WheelDefinition normalWheel = new WheelDefinition();
      [SerializeField] private WheelDefinition safeWheel = new WheelDefinition();
      [SerializeField] private WheelDefinition superWheel = new WheelDefinition();

      [Header("Reward Presentation")]
      [SerializeField] private RewardIconBindingSO rewardIconBinding;
      
      public int SafeZoneInterval => Mathf.Max(1, safeZoneInterval);
      public int SuperZoneInterval => Mathf.Max(1, superZoneInterval);
      public string CashRewardId => cashRewardId;
      public string GoldRewardId => goldRewardId;
      public int StartingCash => Mathf.Max(0, startingCash);
      public int StartingGold => Mathf.Max(0, startingGold);
      public int ReviveGoldCost => Mathf.Max(1, reviveGoldCost);
      public float ReviveGoldCostGrowthMultiplier => Mathf.Max(1f, reviveGoldCostGrowthMultiplier);
      public float RewardPopupHoldSeconds => Mathf.Max(0f, rewardPopupHoldSeconds);
      public int RewardPopupMaxBurstIcons => Mathf.Max(1, rewardPopupMaxBurstIcons);
      public Vector2 RewardPopupIconScaleRange => rewardPopupIconScaleRange;
      public RewardIconBindingSO RewardIconBinding => rewardIconBinding;

      public WheelDefinition GetWheelForZone(ZoneType zoneType)
      {
          switch (zoneType)
          {
              case ZoneType.Safe:
                  return safeWheel;
              case ZoneType.Super:
                  return superWheel;
              default:
                  return normalWheel;
          }
      }
      
      private void OnValidate()
      {
          safeZoneInterval = Mathf.Max(1, safeZoneInterval);
          superZoneInterval = Mathf.Max(1, superZoneInterval);
          startingCash = Mathf.Max(0, startingCash);
          startingGold = Mathf.Max(0, startingGold);
          reviveGoldCost = Mathf.Max(1, reviveGoldCost);
          reviveGoldCostGrowthMultiplier = Mathf.Max(1f, reviveGoldCostGrowthMultiplier);
          rewardPopupHoldSeconds = Mathf.Max(0f, rewardPopupHoldSeconds);
          rewardPopupMaxBurstIcons = Mathf.Max(1, rewardPopupMaxBurstIcons);

          if (rewardPopupIconScaleRange.x <= 0f)
          {
              rewardPopupIconScaleRange.x = 0.01f;
          }

          if (rewardPopupIconScaleRange.y < rewardPopupIconScaleRange.x)
          {
              rewardPopupIconScaleRange.y = rewardPopupIconScaleRange.x;
          }
      }
  }

