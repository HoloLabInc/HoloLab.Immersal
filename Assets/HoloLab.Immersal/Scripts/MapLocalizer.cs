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
            origin.position = Vector3.zero;
            origin.rotation = Quaternion.identity;

            captureCamera.localPosition = info.Pose.position;
            captureCamera.localRotation = info.Pose.rotation;

            // captureCamera -> mapOrigin が正しい値になる
            mapOrigin.position = origin.position;
            mapOrigin.rotation = origin.rotation;

            // origin -> captureCamera を実際の値に合わせる
            captureCamera.localPosition = info.CameraPose.position;
            captureCamera.localRotation = info.CameraPose.rotation;


            var position = mapOrigin.position;
            var rotation = mapOrigin.rotation;

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}