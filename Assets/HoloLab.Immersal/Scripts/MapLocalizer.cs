// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Immersal
{
    public class MapLocalizer : MonoBehaviour
    {
        [SerializeField]
        private ImmersalLocalization immersalLocalization = null;

        private void Awake()
        {
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