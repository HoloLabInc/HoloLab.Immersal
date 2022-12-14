// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Runtime.InteropServices;

#if WINDOWS_UWP
using Windows.Perception.Spatial;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Media.Capture;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

#if MIXEDREALITY_OPENXR_PRESENT
using Microsoft.MixedReality.OpenXR;
#endif
#endif

namespace HoloLab.Immersal
{
    public class CaptureForHoloLens : MonoBehaviour, ICaptureForLocalization
    {
        [StructLayout(LayoutKind.Sequential)]
        struct HolographicFrameNativeData
        {
            public uint VersionNumber;
            public uint MaxNumberOfCameras;
            public IntPtr ISpatialCoordinateSystemPtr; // Windows::Perception::Spatial::ISpatialCoordinateSystem
            public IntPtr IHolographicFramePtr; // Windows::Graphics::Holographic::IHolographicFrame 
            public IntPtr IHolographicCameraPtr; // // Windows::Graphics::Holographic::IHolographicCamera
        }

        public int IntervalMilliseconds { get; set; }

        private SynchronizationContext context;

#if WINDOWS_UWP
        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan latestTime = TimeSpan.MinValue;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private bool isCapturing = false;

        private MediaCapture mediaCapture;
        private MediaFrameReader mediaFrameReader;

        private IntPtr currentWorldOriginCoordinateSystemPtr = IntPtr.Zero;
        private SpatialCoordinateSystem worldOriginCoordinateSystem = null;

        private SpatialCoordinateSystem WorldOriginCoordinateSystem
        {
            get
            {
#if MIXEDREALITY_OPENXR_PRESENT
               return PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
#else
                // https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/unity-xrdevice-advanced?tabs=xr
                var worldOriginCoordinateSystemPtr = IntPtr.Zero;
#if WMR_ENABLED
                worldOriginCoorinateSystemPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
#else
                var nativePtr = UnityEngine.XR.XRDevice.GetNativePtr();
                HolographicFrameNativeData hfd = Marshal.PtrToStructure<HolographicFrameNativeData>(nativePtr);
                worldOriginCoordinateSystemPtr = hfd.ISpatialCoordinateSystemPtr;

#endif

                if (currentWorldOriginCoordinateSystemPtr != worldOriginCoordinateSystemPtr && worldOriginCoordinateSystemPtr != IntPtr.Zero)
                {
                    worldOriginCoordinateSystem = (SpatialCoordinateSystem)Marshal.GetObjectForIUnknown(worldOriginCoordinateSystemPtr);
                }

                return worldOriginCoordinateSystem;
#endif
            }
        }
#endif

#pragma warning disable 0067
        public event Action<CaptureImage> OnCaptured;

        public event Action<Exception> OnError;
#pragma warning restore 0067

        private void Awake()
        {
            context = SynchronizationContext.Current;
        }

        public void StartCapture()
        {
#if WINDOWS_UWP
            _ = StartCaptureAsync();
#else
            throw new NotImplementedException();
#endif
        }

        public void StopCapture()
        {
#if WINDOWS_UWP
            _ = StopCaptureAsync();
#else
            throw new NotImplementedException();
#endif
        }


#if WINDOWS_UWP
        private async Task StopCaptureAsync()
        {
            await semaphore.WaitAsync();

            try
            {
                if (isCapturing)
                {
                    mediaFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
                    await mediaFrameReader.StopAsync();
                    mediaCapture.Dispose();
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
            finally
            {
                mediaCapture = null;
                isCapturing = false;
                semaphore.Release();
            }
        }

        private async Task StartCaptureAsync()
        {
            await semaphore.WaitAsync();

            try
            {
                if (isCapturing)
                {
                    return;
                }

                var (selectedGroup, colorSourceInfo) = await FindVideoPreviewSource();
                if (selectedGroup == null || colorSourceInfo == null)
                {
                    return;
                }

                mediaCapture = await InitializeMediaCapture(selectedGroup);

                if (mediaCapture == null)
                {
                    return;
                }
                var colorFrameSource = mediaCapture.FrameSources[colorSourceInfo.Id];
                await SetFormat(colorFrameSource);

                mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(colorFrameSource, MediaEncodingSubtypes.Argb32);
                mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;

                stopwatch.Start();
                await mediaFrameReader.StartAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }


        private async Task<(MediaFrameSourceGroup sourceGroup, MediaFrameSourceInfo sourceInfo)> FindVideoPreviewSource()
        {
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

            foreach (var sourceGroup in frameSourceGroups)
            {
                foreach (var sourceInfo in sourceGroup.SourceInfos)
                {
                    if (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview
                        && sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                    {

                        return (sourceGroup, sourceInfo);
                    }
                }
            }
            return (null, null);
        }

        private async Task<MediaCapture> InitializeMediaCapture(MediaFrameSourceGroup selectedGroup)
        {
            var mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = selectedGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };
            try
            {
                await mediaCapture.InitializeAsync(settings);
                return mediaCapture;
            }
            catch (Exception ex)
            {
                mediaCapture.Dispose();
                OnError?.Invoke(ex);
                return null;
            }
        }

        private async Task<bool> SetFormat(MediaFrameSource frameSource)
        {
            var supportedFormats = frameSource.SupportedFormats;
            var preferredFormat = supportedFormats.OrderBy(x => x.VideoFormat.Width).FirstOrDefault();

            if (preferredFormat == null)
            {
                return false;
            }

            await frameSource.SetFormatAsync(preferredFormat);
            return true;
        }

        private async void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var currentTime = stopwatch.Elapsed;
            if (currentTime < latestTime + TimeSpan.FromMilliseconds(IntervalMilliseconds))
            {
                return;
            }

            latestTime = currentTime;

            var mediaFrameReference = sender.TryAcquireLatestFrame();

            var coordinateSystem = mediaFrameReference?.CoordinateSystem;
            var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;
            var cameraIntrinsics = videoMediaFrame?.CameraIntrinsics;

            if (softwareBitmap == null)
            {
                return;
            }

            var pngData = await EncodeToPng(softwareBitmap);
            softwareBitmap.Dispose();


            context.Post(_ =>
            {
                // WorldOriginCoordinateSystem should be accessed in main thread
                var worldOrigin = WorldOriginCoordinateSystem;
                if (worldOrigin == null)
                {
                    return;
                }

                var matrix = coordinateSystem?.TryGetTransformTo(worldOrigin);

                Pose pose;
                if (matrix.HasValue)
                {
                    pose = ToUnityPose(matrix.Value);
                }
                else
                {
                    pose = new Pose();
                }
                var image = new CaptureImage()
                {
                    Data = pngData,
                    PrincipalPoint = ToUnityVector(cameraIntrinsics.PrincipalPoint),
                    FocalLength = ToUnityVector(cameraIntrinsics.FocalLength),
                    CameraPose = pose,
                    CoordinateSystem = coordinateSystem,
                };

                OnCaptured?.Invoke(image);
            }, null);
        }

        private async Task<byte[]> EncodeToPng(SoftwareBitmap bitmap)
        {
            byte[] array = null;
            var encoderId = BitmapEncoder.PngEncoderId;
            using (var ms = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
                encoder.SetSoftwareBitmap(bitmap);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception)
                {
                    return array;
                }

                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }

        private static Vector2 ToUnityVector(System.Numerics.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        private static Pose ToUnityPose(System.Numerics.Matrix4x4 matrix)
        {
            System.Numerics.Matrix4x4 newMatrix = matrix;

            // Platform coordinates are all right handed and unity uses left handed matrices. so we convert the matrix
            // from rhs-rhs to lhs-lhs 
            // Convert from right to left coordinate system
            newMatrix.M13 = -newMatrix.M13;
            newMatrix.M23 = -newMatrix.M23;
            newMatrix.M43 = -newMatrix.M43;

            newMatrix.M31 = -newMatrix.M31;
            newMatrix.M32 = -newMatrix.M32;
            newMatrix.M34 = -newMatrix.M34;

            System.Numerics.Matrix4x4.Decompose(newMatrix, out _, out var numericsRotation, out var numericsTranslation);
            var translation = new Vector3(numericsTranslation.X, numericsTranslation.Y, numericsTranslation.Z);
            var rotation = new Quaternion(numericsRotation.X, numericsRotation.Y, numericsRotation.Z, numericsRotation.W);

            return new Pose(translation, rotation);
        }
#endif
    }
}