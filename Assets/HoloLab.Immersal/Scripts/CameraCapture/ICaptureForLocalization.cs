// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Perception.Spatial;
#endif

namespace HoloLab.Immersal
{
    public class CaptureImage
    {
        public byte[] Data;
        public Vector2 PrincipalPoint;
        public Vector2 FocalLength;

        public Pose CameraPose;
#if WINDOWS_UWP
        public SpatialCoordinateSystem CoordinateSystem;
#endif
    }

    public interface ICaptureForLocalization
    {
        int IntervalMilliseconds { set; get; }
        void StartCapture();
        void StopCapture();

        event Action<CaptureImage> OnCaptured;
        event Action<Exception> OnError;
    }
}