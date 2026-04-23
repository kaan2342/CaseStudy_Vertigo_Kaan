using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelSpinner : MonoBehaviour
    {
        //TODO: move these values into a config SO
        private static readonly Color BronzeAccentColor = new Color(1f, 0.76f, 0.22f, 1f);
        private static readonly Color SilverAccentColor = new Color(0.9f, 0.94f, 1f, 1f);
        private static readonly Color GoldenAccentColor = new Color(1f, 0.91f, 0.18f, 1f);

        [Header("Scene References")]
        [SerializeField] private RectTransform ui_wheel_root;
        [SerializeField] private RectTransform ui_group_slice_slots;
        [SerializeField] private WheelSpinnerSlotView[] ui_slice_slot_views = Array.Empty<WheelSpinnerSlotView>();
        [SerializeField] private Image ui_image_spin_base;
        [SerializeField] private Image ui_image_indicator;
        [SerializeField] private Image ui_image_center_glow;
        [SerializeField] private TMP_Text ui_text_center_value;

        [Header("Theme Sprites")]
        [SerializeField] private Sprite ui_sprite_bronze_base;
        [SerializeField] private Sprite ui_sprite_silver_base;
        [SerializeField] private Sprite ui_sprite_golden_base;
        [SerializeField] private Sprite ui_sprite_bronze_indicator;
        [SerializeField] private Sprite ui_sprite_silver_indicator;
        [SerializeField] private Sprite ui_sprite_golden_indicator;

        [Header("Slice Presentation")]
        [SerializeField] private Sprite ui_sprite_bomb_icon;
        [SerializeField] private Sprite ui_sprite_fallback_reward_icon;
        [SerializeField] private Color rewardTextColor = Color.white;
        [SerializeField] private Color bombTextColor = new Color(1f, 0.42f, 0.31f, 1f);

        [Header("Spin Animation")]
        [SerializeField] private AnimationCurve spinEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private WheelGameConfigSO wheelGameConfig;
        private float currentWheelAngle;
        private bool hasInitializedWheelAngle;

        public void ApplyConfig()
        {
            wheelGameConfig = WheelGameConfigSO.Instance;
        }

        public void RenderWheel(IReadOnlyList<RuntimeWheelSlice> slices, ZoneType zoneType, int currentZone)
        {
            if (!enabled)
            {
                return;
            }

            ApplyZoneTheme(zoneType);
            UpdateCenterValue(currentZone, zoneType);
            RenderSlots(slices);
        }

        //NOTE: Used AI for this algorithm
        public IEnumerator SpinToSlice(int sliceIndex, int sliceCount, float spinDurationSeconds, int extraRevolutions)
        {
            if (sliceCount <= 0)
            {
                yield break;
            }

            var targetAngle = ResolveTargetWheelAngle(sliceIndex, sliceCount);
            if (spinDurationSeconds <= 0f)
            {
                SetWheelAngle(targetAngle);
                yield break;
            }

            var startAngle = currentWheelAngle;
            var startNormalizedAngle = NormalizeAngle(startAngle);
            var targetNormalizedAngle = NormalizeAngle(targetAngle);
            var clockwiseDelta = Mathf.Repeat(startNormalizedAngle - targetNormalizedAngle, 360f);
            var totalDelta = -(clockwiseDelta + (Mathf.Max(0, extraRevolutions) * 360f));

            var elapsed = 0f;
            while (elapsed < spinDurationSeconds)
            {
                elapsed += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsed / spinDurationSeconds);
                var easedTime = spinEase != null ? spinEase.Evaluate(normalizedTime) : normalizedTime;
                SetWheelAngle(startAngle + (totalDelta * easedTime));
                yield return null;
            }

            SetWheelAngle(targetAngle);
        }

        public Sprite ResolveRewardSprite(string rewardId)
        {
            RewardIconEntry binding;
            if (TryGetRewardBinding(rewardId, out binding) && binding.Sprite != null)
            {
                return binding.Sprite;
            }

            return ui_sprite_fallback_reward_icon;
        }

        private void Awake()
        {
            InitializeWheelAngle();
        }

        private void RenderSlots(IReadOnlyList<RuntimeWheelSlice> slices)
        {
            var renderedCount = slices != null ? Mathf.Min(slices.Count, ui_slice_slot_views.Length) : 0;
            for (var i = 0; i < ui_slice_slot_views.Length; i++)
            {
                var slotView = ui_slice_slot_views[i];
                if (slotView == null)
                {
                    continue;
                }

                var shouldRender = i < renderedCount;
                slotView.SetVisible(shouldRender);
                if (!shouldRender)
                {
                    continue;
                }

                var slice = slices[i];
                slotView.Render(
                    slice,
                    ResolveSliceSprite(slice),
                    slice.IsBomb ? bombTextColor : rewardTextColor);
            }
        }

        private void ApplyZoneTheme(ZoneType zoneType)
        {
            ui_image_spin_base.sprite = ResolveBaseSprite(zoneType);
            ui_image_indicator.sprite = ResolveIndicatorSprite(zoneType);

            var accentColor = ResolveZoneAccent(zoneType);
            ui_image_center_glow.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.8f);
            ui_text_center_value.color = accentColor;
        }

        private void UpdateCenterValue(int currentZone, ZoneType zoneType)
        {
            ui_text_center_value.text = Mathf.Max(0, currentZone).ToString("00");
            ui_text_center_value.color = ResolveZoneAccent(zoneType);
        }

        private float ResolveTargetWheelAngle(int sliceIndex, int sliceCount)
        {
            var clampedIndex = Mathf.Clamp(sliceIndex, 0, Mathf.Max(0, sliceCount - 1));
            if (ui_slice_slot_views != null && clampedIndex < ui_slice_slot_views.Length)
            {
                var slotView = ui_slice_slot_views[clampedIndex];
                var root = slotView != null ? slotView.Root : null;
                if (root != null)
                {
                    var radialPosition = root.anchoredPosition;
                    if (radialPosition.sqrMagnitude > 0.001f)
                    {
                        var slotAngle = Mathf.Atan2(radialPosition.y, radialPosition.x) * Mathf.Rad2Deg;
                        return NormalizeAngle(90f - slotAngle);
                    }
                }
            }

            var sliceAngle = 360f / Mathf.Max(1, sliceCount);
            return NormalizeAngle(clampedIndex * sliceAngle);
        }

        private void SetWheelAngle(float angle)
        {
            currentWheelAngle = NormalizeAngle(angle);
            ui_image_spin_base.rectTransform.localEulerAngles = new Vector3(0f, 0f, currentWheelAngle);
        }

        private bool TryGetRewardBinding(string rewardId, out RewardIconEntry binding)
        {
            var resolvedConfig = wheelGameConfig != null ? wheelGameConfig : WheelGameConfigSO.Instance;
            if (resolvedConfig != null && resolvedConfig.RewardIconBinding != null)
            {
                return resolvedConfig.RewardIconBinding.TryGetBinding(rewardId, out binding);
            }

            binding = default;
            return false;
        }

        private Sprite ResolveSliceSprite(RuntimeWheelSlice slice)
        {
            if (slice.IsBomb)
            {
                return ui_sprite_bomb_icon;
            }

            return ResolveRewardSprite(slice.RewardId);
        }
        
        private void InitializeWheelAngle()
        {
            if (hasInitializedWheelAngle || ui_image_spin_base == null)
            {
                return;
            }

            currentWheelAngle = NormalizeAngle(ui_image_spin_base.rectTransform.localEulerAngles.z);
            hasInitializedWheelAngle = true;
        }

        private Sprite ResolveBaseSprite(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Super => ui_sprite_golden_base != null ? ui_sprite_golden_base : ui_image_spin_base.sprite,
                ZoneType.Safe => ui_sprite_silver_base != null ? ui_sprite_silver_base : ui_image_spin_base.sprite,
                _ => ui_sprite_bronze_base != null ? ui_sprite_bronze_base : ui_image_spin_base.sprite
            };
        }

        private Sprite ResolveIndicatorSprite(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Super => ui_sprite_golden_indicator != null ? ui_sprite_golden_indicator : ui_image_indicator.sprite,
                ZoneType.Safe => ui_sprite_silver_indicator != null ? ui_sprite_silver_indicator : ui_image_indicator.sprite,
                _ => ui_sprite_bronze_indicator != null ? ui_sprite_bronze_indicator : ui_image_indicator.sprite
            };
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle, 360f);
        }

        private static Color ResolveZoneAccent(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Super => GoldenAccentColor,
                ZoneType.Safe => SilverAccentColor,
                _ => BronzeAccentColor
            };
        }
    }
}
