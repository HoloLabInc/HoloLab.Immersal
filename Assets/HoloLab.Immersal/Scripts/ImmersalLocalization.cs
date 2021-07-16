using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace HoloLab.Immersal
{
    public class ImmersalLocalization : MonoBehaviour
    {
        public struct LocalizeInfo
        {
            public int MapId;

            public Pose Pose;

            public Pose CameraPose;
        }

        [SerializeField]
        private int intervalMilliseconds = 3000;

        [SerializeField]
        private LocalizationSettings localizationSettings = null;

        private ICaptureForLocalization captureForLocalization;
        private ImmersalClient immersalClient;

        private SynchronizationContext context;

        public event Action<LocalizeInfo> OnLocalized;
        public event Action OnLocalizationFailed;

        private void Start()
        {
            context = SynchronizationContext.Current;

            Initialize();
            captureForLocalization.StartCapture();
        }

        private void OnDestroy()
        {
            captureForLocalization.StopCapture();
        }


        private void Initialize()
        {
            immersalClient = new ImmersalClient()
            {
                Token = localizationSettings.Token
            };

            captureForLocalization = GetComponent<ICaptureForLocalization>();
            captureForLocalization.IntervalMilliseconds = intervalMilliseconds;

            captureForLocalization.OnCaptured += CaptureForLocalization_OnCaptured;
            captureForLocalization.OnError += CaptureForLocalization_OnError;
        }

        private async void CaptureForLocalization_OnCaptured(CaptureImage image)
        {
            var focalLength = image.FocalLength;
            var fx = focalLength.x;
            var fy = focalLength.y;

            var principalPoint = image.PrincipalPoint;
            var ox = principalPoint.x;
            var oy = principalPoint.y;

            var result = await immersalClient.Localize(localizationSettings.MapIds, image.Data, fx, fy, ox, oy);

            if (result.Success)
            {
                var localizeInfo = new LocalizeInfo()
                {
                    MapId = result.MapId,
                    Pose = new Pose(result.Position, result.Rotation),
                    CameraPose = image.CameraPose
                };

                context.Post(_ => { OnLocalized?.Invoke(localizeInfo); }, null);
            }
            else
            {
                context.Post(_ => { OnLocalizationFailed?.Invoke(); }, null);
            }
        }

        private void CaptureForLocalization_OnError(Exception exception)
        {
            Debug.LogWarning(exception);
        }
    }
}