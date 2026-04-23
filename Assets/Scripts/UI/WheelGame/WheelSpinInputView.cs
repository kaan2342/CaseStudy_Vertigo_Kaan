using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelSpinInputView : MonoBehaviour
    {
        [SerializeField] private Image ui_image_background_fill;
        [SerializeField] private Button ui_button_background_spin;

        private bool isBound;

        public event Action SpinPressed;

        private void Awake()
        {
            if (ui_image_background_fill == null || ui_button_background_spin == null)
            {
                Debug.LogError("WheelSpinInputView is missing one or more required serialized references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!enabled || isBound || ui_button_background_spin == null)
            {
                return;
            }

            ui_button_background_spin.onClick.AddListener(HandleSpinPressed);
            isBound = true;
        }

        private void OnDisable()
        {
            if (!isBound || ui_button_background_spin == null)
            {
                return;
            }

            ui_button_background_spin.onClick.RemoveListener(HandleSpinPressed);
            isBound = false;
        }

        public void Render(bool canSpin)
        {
            if (ui_button_background_spin != null)
            {
                ui_button_background_spin.interactable = canSpin;
            }
        }

        private void HandleSpinPressed()
        {
            SpinPressed?.Invoke();
        }
    }
}
