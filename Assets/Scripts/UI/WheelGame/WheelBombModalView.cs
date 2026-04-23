using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Application;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelBombModalView : MonoBehaviour
    {
        [SerializeField] private GameObject ui_group_bomb_modal;
        [SerializeField] private TMP_Text ui_text_bomb_title_value;
        [SerializeField] private TMP_Text ui_text_bomb_subtitle_value;
        [SerializeField] private Button ui_button_restart;
        [SerializeField] private Button ui_button_revive_currency;
        // [SerializeField] private Button ui_button_revive_ad;

        private bool isBound;

        public event Action RestartPressed;
        public event Action ReviveCurrencyPressed;
        public event Action ReviveAdPressed;
        
        private void OnEnable()
        {
            if (!enabled || isBound)
            {
                return;
            }
            
            ui_button_restart.onClick.AddListener(HandleRestartPressed);
            ui_button_revive_currency.onClick.AddListener(HandleReviveCurrencyPressed);
            // ui_button_revive_ad.onClick.AddListener(HandleReviveAdPressed);

            isBound = true;
        }

        private void OnDisable()
        {
            if (!isBound)
            {
                return;
            }
            
            ui_button_restart.onClick.RemoveListener(HandleRestartPressed);
            ui_button_revive_currency.onClick.RemoveListener(HandleReviveCurrencyPressed);
            // ui_button_revive_ad.onClick.RemoveListener(HandleReviveAdPressed);

            isBound = false;
        }

        public void Render(WheelRunSnapshot snapshot)
        {
            var bombVisible = snapshot.IsAwaitingReviveDecision;
            ui_group_bomb_modal.SetActive(bombVisible);
            ui_text_bomb_title_value.gameObject.SetActive(true);
            ui_text_bomb_subtitle_value.text = BuildBombSubtitle(snapshot);
            
            ui_button_restart.gameObject.SetActive(bombVisible);
            ui_button_restart.interactable = !snapshot.IsSpinning && snapshot.IsAwaitingReviveDecision;
            
            ui_button_revive_currency.gameObject.SetActive(snapshot.IsAwaitingReviveDecision);
            ui_button_revive_currency.interactable = snapshot.CanRevive;
            WheelViewUtility.UpdateButtonLabel(ui_button_revive_currency, "REVIVE " + snapshot.CurrentReviveGoldCost.ToString("N0") + " GOLD");
            
            // ui_button_revive_ad.gameObject.SetActive(false);
        }

        private static string BuildBombSubtitle(WheelRunSnapshot snapshot)//TODO: move these texts to a config SO
        {
            if (snapshot.IsAwaitingReviveDecision)
            {
                if (snapshot.CanRevive)
                {
                    return "Your acquired rewards stay on the left. Spend " + snapshot.CurrentReviveGoldCost.ToString("N0") + " gold to continue.";
                }

                return "Your acquired rewards stay on the left, but you do not have enough gold to revive.";
            }

            return "Press EXIT whenever you want to bank your earned rewards.";
        }

        private void HandleRestartPressed()
        {
            RestartPressed?.Invoke();
        }

        private void HandleReviveCurrencyPressed()
        {
            ReviveCurrencyPressed?.Invoke();
        }

        private void HandleReviveAdPressed()
        {
            ReviveAdPressed?.Invoke();
        }
    }
}
