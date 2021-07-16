// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace HoloLab.Immersal
{
    public class EditorDebugCapture : MonoBehaviour, ICaptureForLocalization
    {
        [SerializeField]
        private Texture2D imageTexture = null;

        [SerializeField]
        private Vector2 principalPoint = Vector2.zero;

        [SerializeField]
        private Vector2 focalLength = Vector2.zero;

        [SerializeField]
        private Vector3 position = Vector3.zero;

        [SerializeField]
        private Vector3 rotation = Vector3.zero;
        
        public int IntervalMilliseconds { get; set; }

        private bool captureEnabled;

        public void StartCapture()
        {
            captureEnabled = true;
            _ = CaptureLoop();
        }

        public void StopCapture()
        {
            captureEnabled = false;
        }

        public event Action<CaptureImage> OnCaptured;
        public event Action<Exception> OnError;

        private async Task CaptureLoop()
        {
            try
            {
                var data = imageTexture.EncodeToPNG();
                var image = new CaptureImage()
                {
                    Data = data,
                    PrincipalPoint = principalPoint,
                    FocalLength = focalLength,
                    CameraPose = new Pose(position, Quaternion.Euler(rotation))
                };

                while (captureEnabled)
                {
                    OnCaptured?.Invoke(image);
                    await Task.Delay(IntervalMilliseconds);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                OnError?.Invoke(e);
            }
        }
    }
}