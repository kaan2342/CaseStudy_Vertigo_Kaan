using System.Collections;
using System.Collections.Generic;
using MRGameCore.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Config;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelRewardPopupView : MonoBehaviour
    {
        [SerializeField] private RectTransform ui_group_reward_popup_overlay;
        [SerializeField] private RectTransform ui_group_reward_popup_card;
        [SerializeField] private CanvasGroup ui_group_reward_popup_card_canvas_group;
        [SerializeField] private Image ui_image_reward_popup_card_background;
        [SerializeField] private Image ui_image_reward_popup_card_icon;
        [SerializeField] private TMP_Text ui_text_reward_popup_card_title_value;
        [SerializeField] private TMP_Text ui_text_reward_popup_card_amount_value;
        [SerializeField] private Image ui_prefab_reward_popup_burst_icon;

        private readonly List<Image> activeBurstIcons = new List<Image>(50);
        private Vector2[] burstSpreadTargets = System.Array.Empty<Vector2>();
        private ComponentPrefabPool<Image> rewardPopupBurstIconPool;
        private WheelGameConfigSO wheelConfig;

        private void Awake()
        {
            ui_group_reward_popup_card.gameObject.SetActive(false);
            EnsureBurstIconPool();
        }

        public void ApplyConfig()
        {
            wheelConfig = WheelGameConfigSO.Instance;

            if (rewardPopupBurstIconPool != null)
            {
                rewardPopupBurstIconPool.Prewarm(Mathf.Min(wheelConfig.RewardPopupMaxBurstIcons, 16));
            }
        }

        public IEnumerator PlayRewardEarnedSequence(
            RuntimeWheelSlice landedSlice,
            WheelSpinner spinner,
            WheelRewardPanelView rewardPanelView)
        {
            if (landedSlice.IsBomb || spinner == null)
            {
                yield break;
            }

            ApplyConfig();

            var overlay = ui_group_reward_popup_overlay;
            overlay.SetAsLastSibling();

            PrepareCard(landedSlice, spinner);

            var rewardSprite = spinner.ResolveRewardSprite(landedSlice.RewardId);
            var spinnerTarget = WheelViewUtility.ResolveOverlayPosition(overlay, spinner.transform as RectTransform, Vector2.zero);
            var rowTarget = rewardPanelView != null
                ? rewardPanelView.ResolveRewardTargetPosition(overlay, landedSlice.RewardId)
                : spinnerTarget;
            var activeWheelConfig = wheelConfig != null ? wheelConfig : WheelGameConfigSO.Instance;
            var burstIconCount = ResolveBurstIconCount(landedSlice.Amount, activeWheelConfig);
            EnsureBurstSpreadCapacity(burstIconCount);

            activeBurstIcons.Clear();

            //TODO: move hardcoded values into config or make them serialized here
            var cardStart = new Vector2(0f, 120f);
            ui_group_reward_popup_card.anchoredPosition = cardStart;
            ui_group_reward_popup_card.localScale = Vector3.one;
            ui_group_reward_popup_card_canvas_group.alpha = 0f;
            ui_group_reward_popup_card.gameObject.SetActive(true);

            var randomScaleRange = activeWheelConfig != null
                ? activeWheelConfig.RewardPopupIconScaleRange
                : new Vector2(0.85f, 1.15f);

            for (var i = 0; i < burstIconCount; i++)
            {
                var burstIcon = CreateBurstIcon(overlay, rewardSprite);
                burstIcon.rectTransform.anchoredPosition = cardStart;
                burstIcon.rectTransform.localScale = Vector3.one * Random.Range(randomScaleRange.x, randomScaleRange.y);
                burstSpreadTargets[i] = cardStart + ResolveBurstSpreadOffset(i, burstIconCount);

                var burstColor = burstIcon.color;
                burstColor.a = 0f;
                burstIcon.color = burstColor;
                activeBurstIcons.Add(burstIcon);
            }

            yield return AnimateRewardPopupAppear(ui_group_reward_popup_card, ui_group_reward_popup_card_canvas_group, cardStart);

            var holdDuration = activeWheelConfig != null ? activeWheelConfig.RewardPopupHoldSeconds : 1f;
            if (holdDuration > 0f)
            {
                yield return new WaitForSeconds(holdDuration);
            }

            yield return AnimateRewardPopupLaunch(
                ui_group_reward_popup_card,
                ui_group_reward_popup_card_canvas_group,
                activeBurstIcons,
                cardStart,
                spinnerTarget,
                burstSpreadTargets);
            yield return AnimateBurstIconsToTarget(activeBurstIcons, burstSpreadTargets, rowTarget);

            ui_group_reward_popup_card.gameObject.SetActive(false);
            ReleaseBurstIcons(activeBurstIcons);
            activeBurstIcons.Clear();
        }

        private void PrepareCard(RuntimeWheelSlice slice, WheelSpinner spinner)
        {
            ui_group_reward_popup_card.SetAsLastSibling();
            ui_image_reward_popup_card_icon.sprite = spinner.ResolveRewardSprite(slice.RewardId);
            ui_image_reward_popup_card_icon.preserveAspect = true;
            ui_image_reward_popup_card_icon.color = Color.white;
            ui_text_reward_popup_card_title_value.text = FormatRewardName(slice.RewardId);
            ui_text_reward_popup_card_amount_value.text = ResolveAmountLabel(slice);
            ui_group_reward_popup_card_canvas_group.alpha = 0f;
        }

        private Image CreateBurstIcon(RectTransform overlay, Sprite sprite)
        {
            var pool = EnsureBurstIconPool();
            var icon = pool.Get(overlay);
            var iconRect = icon.rectTransform;
            iconRect.SetAsLastSibling();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.color = Color.white;
            icon.raycastTarget = false;
            return icon;
        }

        private ComponentPrefabPool<Image> EnsureBurstIconPool()
        {
            if (rewardPopupBurstIconPool == null || rewardPopupBurstIconPool.Prefab != ui_prefab_reward_popup_burst_icon)
            {
                rewardPopupBurstIconPool = new ComponentPrefabPool<Image>(
                    ui_prefab_reward_popup_burst_icon,
                    ui_group_reward_popup_overlay);

                var activeWheelConfig = wheelConfig != null ? wheelConfig : WheelGameConfigSO.Instance;
                rewardPopupBurstIconPool.Prewarm(Mathf.Min(activeWheelConfig.RewardPopupMaxBurstIcons, 16));
            }

            return rewardPopupBurstIconPool;
        }

        private void ReleaseBurstIcons(IReadOnlyList<Image> burstIcons)
        {
            if (rewardPopupBurstIconPool == null || burstIcons == null)
            {
                return;
            }

            rewardPopupBurstIconPool.ReleaseAll(burstIcons);
        }

        private void EnsureBurstSpreadCapacity(int count)
        {
            if (burstSpreadTargets.Length >= count)
            {
                return;
            }

            burstSpreadTargets = new Vector2[count];
        }

        private static int ResolveBurstIconCount(int amount, WheelGameConfigSO wheelConfig)
        {
            var maxCount = wheelConfig != null ? wheelConfig.RewardPopupMaxBurstIcons : 50;
            return Mathf.Clamp(Mathf.Max(1, amount), 1, Mathf.Max(1, maxCount));
        }

        private static Vector2 ResolveBurstSpreadOffset(int index, int count)
        {
            var normalized = count <= 1 ? 0.5f : index / (float)(count - 1);
            var horizontal = Mathf.Lerp(-150f, 150f, normalized);
            var vertical = Random.Range(72f, 158f);
            return new Vector2(horizontal, vertical);
        }

        private static string FormatRewardName(string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return "REWARD";
            }

            return rewardId.Replace("_", " ").ToUpperInvariant();
        }

        private static string ResolveAmountLabel(RuntimeWheelSlice slice)
        {
            return !string.IsNullOrWhiteSpace(slice.DisplayLabel)
                ? slice.DisplayLabel.ToUpperInvariant()
                : "X" + Mathf.Max(0, slice.Amount);
        }

        private static IEnumerator AnimateRewardPopupAppear(
            RectTransform cardRoot,
            CanvasGroup cardGroup,
            Vector2 cardTarget)
        {
            const float duration = 0.2f;
            var elapsed = 0f;
            var startScale = 0.82f;
            var overshootScale = 1.06f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);

                cardRoot.anchoredPosition = cardTarget;
                cardRoot.localScale = Vector3.one * Mathf.Lerp(startScale, overshootScale, eased);
                cardGroup.alpha = eased;
                yield return null;
            }

            cardRoot.anchoredPosition = cardTarget;
            cardRoot.localScale = Vector3.one;
            cardGroup.alpha = 1f;
        }

        //TODO: could use polish && move hardcoded values into config or make them serialized here
        private static IEnumerator AnimateRewardPopupLaunch(
            RectTransform cardRoot,
            CanvasGroup cardGroup,
            List<Image> burstIcons,
            Vector2 cardStart,
            Vector2 spinnerTarget,
            IReadOnlyList<Vector2> spreadTargets)
        {
            const float duration = 0.34f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                var cardT = 1f - Mathf.Pow(1f - t, 3f);

                cardRoot.anchoredPosition = Vector2.Lerp(cardStart, spinnerTarget, cardT);
                cardRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.15f, cardT);
                cardGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t * 1.15f));

                for (var i = 0; i < burstIcons.Count; i++)
                {
                    if (burstIcons[i] == null)
                    {
                        continue;
                    }

                    burstIcons[i].rectTransform.anchoredPosition = Vector2.Lerp(cardStart, spreadTargets[i], eased);
                    var color = burstIcons[i].color;
                    color.a = eased;
                    burstIcons[i].color = color;
                }

                yield return null;
            }

            cardRoot.anchoredPosition = spinnerTarget;
            cardRoot.localScale = Vector3.one * 0.15f;
            cardGroup.alpha = 0f;
        }

        //TODO: could use polish && move hardcoded values into config or make them serialized here
        private static IEnumerator AnimateBurstIconsToTarget(List<Image> burstIcons, IReadOnlyList<Vector2> startPositions, Vector2 target)
        {
            const float duration = 0.52f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);

                for (var i = 0; i < burstIcons.Count; i++)
                {
                    var icon = burstIcons[i];
                    if (icon == null)
                    {
                        continue;
                    }

                    var start = startPositions[i];
                    var curveOffset = new Vector2(0f, Mathf.Sin(t * Mathf.PI) * 42f);
                    icon.rectTransform.anchoredPosition = Vector2.Lerp(start, target, eased) + curveOffset;
                    icon.rectTransform.localScale = Vector3.Lerp(icon.rectTransform.localScale, Vector3.one * 0.62f, eased);

                    var color = icon.color;
                    color.a = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.75f) / 0.25f));
                    icon.color = color;
                }

                yield return null;
            }
        }
    }
}
