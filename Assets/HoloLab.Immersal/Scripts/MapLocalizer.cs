using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Immersal
{
    public class MapLocalizer : MonoBehaviour
    {
        [SerializeField]
        private ImmersalLocalization immersalLocalization;

        private Transform origin;
        private Transform captureCamera;
        private Transform mapOrigin;

        private void Awake()
        {
            origin = new GameObject("Origin").transform;
            origin.SetParent(transform);

            captureCamera = new GameObject("CaptureCamera").transform;
            captureCamera.SetParent(origin);

            mapOrigin = new GameObject("MapOrigin").transform;
            mapOrigin.SetParent(captureCamera);

            immersalLocalization.OnLocalized += ImmersalLocalization_OnLocalized;
        }

        private void ImmersalLocalization_OnLocalized(ImmersalLocalization.LocalizeInfo info)
        {
            var mapOriginPose = info.Pose.Inverse().GetTransformedBy(info.CameraPose);

            transform.position = mapOriginPose.position;
            transform.rotation = mapOriginPose.rotation;
        }
    }
}