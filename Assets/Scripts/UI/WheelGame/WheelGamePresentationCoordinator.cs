using System;
using System.Collections;
using UnityEngine;
using Vertigo.WheelGame.Application;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelGamePresentationCoordinator
    {
        private readonly WheelGameView wheelGameView;

        private Coroutine spinSequence;
        private bool suppressWheelPresentation;

        public WheelGamePresentationCoordinator(WheelGameView wheelGameView)
        {
            this.wheelGameView = wheelGameView;
        }

        public bool IsBusy => spinSequence != null;

        public void Render(WheelRunSnapshot snapshot)
        {
            if (suppressWheelPresentation)
            {
                wheelGameView.RenderTransientState(snapshot);
                return;
            }

            wheelGameView.RenderSnapshot(snapshot);
        }

        public void PlaySpinSequence(
            MonoBehaviour host,
            WheelGameService service,
            SpinPlan spinPlan,
            float spinDurationSeconds,
            int extraSpinRevolutions)
        {
            suppressWheelPresentation = true;
            spinSequence = host.StartCoroutine(SpinAndResolve(host, service, spinPlan, spinDurationSeconds, extraSpinRevolutions));
        }

        private IEnumerator SpinAndResolve(
            MonoBehaviour host,
            WheelGameService service,
            SpinPlan spinPlan,
            float spinDurationSeconds,
            int extraSpinRevolutions)
        {
            var didResolveSpin = false;

            try
            {
                yield return wheelGameView.PlaySpin(spinPlan, spinDurationSeconds, extraSpinRevolutions);

                var resolution = service.ResolvePlannedSpin();
                didResolveSpin = true;

                if (!resolution.HitBomb)
                {
                    yield return wheelGameView.PlayRewardEarnedSequence(resolution.LandedSlice);
                }
            }
            finally
            {
                suppressWheelPresentation = false;

                if (!didResolveSpin)
                {
                    try
                    {
                        service.CancelPlannedSpin();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, host);
                    }
                }

                spinSequence = null;
                RenderLatestSnapshotSafely(service, host);
            }
        }

        private void RenderLatestSnapshotSafely(WheelGameService service, UnityEngine.Object context)
        {
            try
            {
                wheelGameView.RenderSnapshot(service.Snapshot);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, context);
            }
        }
    }
}
