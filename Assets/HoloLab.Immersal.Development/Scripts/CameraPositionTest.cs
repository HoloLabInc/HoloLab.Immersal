// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using HoloLab.Immersal;
using UnityEngine;

namespace HoloLab.Immersal.Development
{
    public class CameraPositionTest : MonoBehaviour
    {
        [SerializeField]
        private ImmersalLocalization immersalLocalization = null;

        void Start()
        {
            immersalLocalization.OnLocalized += ImmersalLocalization_OnLocalized;
        }

        private void ImmersalLocalization_OnLocalized(ImmersalLocalization.LocalizeInfo info)
        {
            transform.position = info.CameraPose.position;
            transform.rotation = info.CameraPose.rotation;
        }
    }
}