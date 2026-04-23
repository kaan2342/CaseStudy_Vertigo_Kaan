using UnityEngine;
using Vertigo.WheelGame.Application;
using Vertigo.WheelGame.Config;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelGameController : MonoBehaviour
    {
        [SerializeField] private WheelGameView wheelGameView;

        [Header("Spin Animation")]
        [Min(0.05f)]
        [SerializeField] private float spinDurationSeconds = 2.2f;
        [Min(1)]
        [SerializeField] private int extraSpinRevolutions = 5;

        [Header("Restart Behavior")]
        [SerializeField] private bool clearLifetimeRewardsOnManualRestart;

        private WheelGameService service;
        private WheelGamePresentationCoordinator presentationCoordinator;

        private void Awake()
        {
            service = new WheelGameService(new UnityRandomProvider());
            presentationCoordinator = new WheelGamePresentationCoordinator(wheelGameView);
            wheelGameView.ApplyConfig();
            presentationCoordinator.Render(service.Snapshot);
        }

        private void OnEnable()
        {
            wheelGameView.SpinPressed += HandleSpinPressed;
            wheelGameView.LeavePressed += HandleLeavePressed;
            wheelGameView.RestartPressed += HandleRestartPressed;
            wheelGameView.ReviveCurrencyPressed += HandleReviveCurrencyPressed;
            wheelGameView.ReviveAdPressed += HandleReviveAdPressed;

            service.StateChanged += HandleServiceStateChanged;
        }

        private void OnDisable()
        {
            wheelGameView.SpinPressed -= HandleSpinPressed;
            wheelGameView.LeavePressed -= HandleLeavePressed;
            wheelGameView.RestartPressed -= HandleRestartPressed;
            wheelGameView.ReviveCurrencyPressed -= HandleReviveCurrencyPressed;
            wheelGameView.ReviveAdPressed -= HandleReviveAdPressed;

            service.StateChanged -= HandleServiceStateChanged;
        }

        private void HandleSpinPressed()
        {
            if (presentationCoordinator.IsBusy || service == null || !service.CanSpin)
            {
                return;
            }

            var spinPlan = service.PlanSpin();
            presentationCoordinator.PlaySpinSequence(
                this,
                service,
                spinPlan,
                spinDurationSeconds,
                extraSpinRevolutions);
        }

        private void HandleLeavePressed()
        {
            if (presentationCoordinator.IsBusy || service == null || !service.CanLeave)
            {
                return;
            }

            service.CashOut();
        }

        private void HandleRestartPressed()
        {
            if ( presentationCoordinator.IsBusy || service == null)
            {
                return;
            }

            if (service.CanGiveUp)
            {
                service.GiveUp();
                return;
            }

            service.RestartRun(clearLifetimeRewardsOnManualRestart);
        }

        private void HandleReviveCurrencyPressed()
        {
            if (presentationCoordinator.IsBusy || service == null || !service.CanRevive)
            {
                return;
            }

            service.ReviveRun();
        }

        private void HandleReviveAdPressed()
        {
            if (presentationCoordinator.IsBusy || service == null || !service.CanRevive)
            {
                return;
            }

            service.ReviveRun();
        }

        private void HandleServiceStateChanged()
        {
            presentationCoordinator.Render(service.Snapshot);
        }

        private void OnValidate()
        {
            spinDurationSeconds = Mathf.Max(0.05f, spinDurationSeconds);
            extraSpinRevolutions = Mathf.Max(1, extraSpinRevolutions);
        }
    }
}
