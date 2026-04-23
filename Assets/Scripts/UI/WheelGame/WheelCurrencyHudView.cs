using TMPro;
using UnityEngine;
using Vertigo.WheelGame.Application;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelCurrencyHudView : MonoBehaviour
    {
        [SerializeField] private RectTransform ui_group_currency_hud;
        [SerializeField] private TMP_Text ui_text_owned_cash_value;
        [SerializeField] private TMP_Text ui_text_owned_gold_value;

        private void Awake()
        {
            if (ui_group_currency_hud == null || ui_text_owned_cash_value == null || ui_text_owned_gold_value == null)
            {
                Debug.LogError("WheelCurrencyHudView is missing one or more required serialized references.", this);
                enabled = false;
            }
        }

        public void Render(WheelRunSnapshot snapshot)
        {
            if (ui_text_owned_cash_value != null)
            {
                ui_text_owned_cash_value.text = snapshot.OwnedCash.ToString("N0");
            }

            if (ui_text_owned_gold_value != null)
            {
                ui_text_owned_gold_value.text = snapshot.OwnedGold.ToString("N0");
            }
        }
    }
}
