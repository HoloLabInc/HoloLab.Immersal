using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

#if ARFOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace HoloLab.Immersal
{
    class CaptureForARFoundation : MonoBehaviour, ICaptureForLocalization
    {
#if ARFOUNDATION_PRESENT
        [SerializeField]
        private ARCameraManager arCameraManager;

        private Texture2D _conversionTexture2D;

        private bool _isCapturing;
#endif

        public int IntervalMilliseconds { get; set; }
        public event Action<CaptureImage> OnCaptured;
        public event Action<Exception> OnError;


        public void StartCapture()
        {
#if ARFOUNDATION_PRESENT
            if (_isCapturing)
            {
                return;
            }

            _isCapturing = true;
            _ = CaptureLoop();
#endif
        }

        public void StopCapture()
        {
#if ARFOUNDATION_PRESENT
            _isCapturing = false;
#endif
        }

#if ARFOUNDATION_PRESENT
        private async Task CaptureLoop()
        {
            if (arCameraManager == null)
            {
                OnError?.Invoke(new NullReferenceException("ARCameraManager is not set"));
                return;
            }

            while (_isCapturing)
            {
                var processTask = ProcessCameraImageAsync();
                var delayTask = Task.Delay(IntervalMilliseconds);
                await Task.WhenAll(processTask, delayTask);
            }
        }

        /// <summary>
        /// Get and process capture camera data.
        /// </summary>
        private async Task ProcessCameraImageAsync()
        {
            if (!arCameraManager.TryGetIntrinsics(out var intrinsics))
            {
                return;
            }

            if (!arCameraManager.TryAcquireLatestCpuImage(out var image))
            {
                return;
            }

            var isConversionSuccess = await TryConvertCpuImageToTextureAsync(image);
            image.Dispose();

            if (!isConversionSuccess)
            {
                return;
            }

            var encodedImageData = _conversionTexture2D.EncodeToPNG();

            var cameraTransform = arCameraManager.transform;
            var captureImage = new CaptureImage
            {
                Data = encodedImageData,
                FocalLength = new Vector2(intrinsics.focalLength.y, intrinsics.focalLength.x),
                PrincipalPoint = new Vector2(intrinsics.principalPoint.y, intrinsics.principalPoint.x),
                CameraPose = new Pose(cameraTransform.localPosition, cameraTransform.localRotation)
            };

            OnCaptured?.Invoke(captureImage);
        }

        /// <summary>
        /// Convert XRCpuImage to Texture asynchronously.
        /// </summary>
        /// <param name="cpuImage">image data acquire from ARCamera</param>
        /// <returns>return true if conversion succeeded</returns>
        private async Task<bool> TryConvertCpuImageToTextureAsync(XRCpuImage cpuImage)
        {
            using (var conversionRequest = cpuImage.ConvertAsync(new XRCpuImage.ConversionParams
                   {
                       transformation = XRCpuImage.Transformation.None,
                       inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                       outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                       outputFormat = TextureFormat.RGBA32
                   }))
            {
                while (!conversionRequest.status.IsDone())
                {
                    if (!_isCapturing)
                    {
                        return false;
                    }

                    await Task.Yield();
                }

                if (conversionRequest.status != XRCpuImage.AsyncConversionStatus.Ready)
                {
                    OnError?.Invoke(new InvalidOperationException("cannot convert XRCpuImage to Texture"));
                    return false;
                }

                var originalWidth = conversionRequest.conversionParams.outputDimensions.x;
                var originalHeight = conversionRequest.conversionParams.outputDimensions.y;

                using (var rawData = conversionRequest.GetData<byte>())
                {
                    using (var swappedRawData = new NativeArray<byte>(rawData, Allocator.Temp))
                    {
                        if (!SwapXYPixelIndexOfImageData(rawData, originalWidth, originalHeight, swappedRawData))
                        {
                            return false;
                        }

                        if (_conversionTexture2D == null)
                        {
                            _conversionTexture2D = new Texture2D(
                                originalHeight,
                                originalWidth,
                                conversionRequest.conversionParams.outputFormat,
                                false
                            );
                        }

                        _conversionTexture2D.LoadRawTextureData(swappedRawData);
                        _conversionTexture2D.Apply();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Transpose image pixel index for Localization.
        /// </summary>
        /// <param name="sourceImageBuffer">Color pixel readonly native array(it can be acquired from Texture.GetRawTextureData)</param>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceHeight"></param>
        /// <param name="distImageBuffer"></param>
        /// <returns>Whether this process succeeded</returns>
        private static bool SwapXYPixelIndexOfImageData(
            NativeArray<byte> sourceImageBuffer, int sourceWidth, int sourceHeight,
            NativeArray<byte> distImageBuffer
        )
        {
            if (sourceImageBuffer.Length != sourceWidth * sourceHeight * 4)
            {
                return false;
            }

            if (distImageBuffer.Length != sourceWidth * sourceHeight * 4)
            {
                return false;
            }

            for (var y = 0; y < sourceHeight; y++)
            {
                for (var x = 0; x < sourceWidth; x++)
                {
                    var sourcePixelIndex = x + y * sourceWidth;
                    var distPixelIndex = (sourceHeight - 1 - y) + (sourceWidth - 1 - x) * sourceHeight;

                    distImageBuffer[distPixelIndex * 4 + 0] = sourceImageBuffer[sourcePixelIndex * 4 + 0];
                    distImageBuffer[distPixelIndex * 4 + 1] = sourceImageBuffer[sourcePixelIndex * 4 + 1];
                    distImageBuffer[distPixelIndex * 4 + 2] = sourceImageBuffer[sourcePixelIndex * 4 + 2];
                    distImageBuffer[distPixelIndex * 4 + 3] = sourceImageBuffer[sourcePixelIndex * 4 + 3];
                }
            }

            return true;
        }

#endif
    }
}