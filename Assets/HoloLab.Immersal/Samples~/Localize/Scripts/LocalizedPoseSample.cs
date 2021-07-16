// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using HoloLab.Immersal;
using UnityEngine;

public class LocalizedPoseSample : MonoBehaviour
{
    [SerializeField]
    private ImmersalLocalization immersalLocalization = null;

    private void Awake()
    {
        immersalLocalization.OnLocalized += ImmersalLocalization_OnLocalized;
    }

    private void ImmersalLocalization_OnLocalized(ImmersalLocalization.LocalizeInfo info)
    {
        transform.localPosition = info.Pose.position;
        transform.localRotation = info.Pose.rotation;
    }
}
