// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLab.Immersal
{
    public struct LocalizeResult
    {
        public bool Success;
        public int MapId;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public static class ApiMessagesExtensions
    {
        internal static LocalizeResult ToLocalizeResult(this SDKLocalizeResult sdkResult)
        {
            var success = sdkResult.success;
            var mapId = sdkResult.map;
            var position = new Vector3(sdkResult.px, sdkResult.py, sdkResult.pz);

            var matrix = new Matrix4x4
            {
                m00 = sdkResult.r00,
                m01 = sdkResult.r01,
                m02 = sdkResult.r02,
                m10 = sdkResult.r10,
                m11 = sdkResult.r11,
                m12 = sdkResult.r12,
                m20 = sdkResult.r20,
                m21 = sdkResult.r21,
                m22 = sdkResult.r22,
                m33 = 1f
            };

            var rotation = matrix.rotation * Quaternion.AngleAxis(-90, Vector3.forward);

            return new LocalizeResult()
            {
                Success = success,
                MapId = mapId,
                Position = position,
                Rotation = rotation
            };
        }
    }
}