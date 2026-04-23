using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Presentation
{
    [DisallowMultipleComponent]
    public sealed class WheelSpinnerSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform ui_group_slice_slot;
        [SerializeField] private Image ui_image_slice_icon;
        [SerializeField] private TextMeshProUGUI ui_text_slice_value;

        public RectTransform Root => ui_group_slice_slot != null ? ui_group_slice_slot : transform as RectTransform;

        public void Render(
            RuntimeWheelSlice slice,
            Sprite iconSprite,
            Color textColor)
        {

            if (Root != null)
            {
                Root.gameObject.SetActive(true);
            }

            if (ui_image_slice_icon != null)
            {
                ui_image_slice_icon.sprite = iconSprite;
                ui_image_slice_icon.color = Color.white;
                ui_image_slice_icon.preserveAspect = true;
            }

            if (ui_text_slice_value != null)
            {
                ui_text_slice_value.text = BuildSliceText(slice);
                ui_text_slice_value.color = textColor;
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (Root != null)
            {
                Root.gameObject.SetActive(isVisible);
            }
        }

        private static string BuildSliceText(RuntimeWheelSlice slice)
        {
            if (slice.IsBomb)
            {
                return "BOMB";
            }

            if (!string.IsNullOrWhiteSpace(slice.DisplayLabel))
            {
                return slice.DisplayLabel.ToUpperInvariant();
            }

            return "X" + Mathf.Max(0, slice.Amount);
        }
    }
}
