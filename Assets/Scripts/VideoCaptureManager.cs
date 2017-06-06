using HoloLensCameraStream;
using System;
using UnityEngine;
using UnityEngine.VR.WSA;

#if WINDOWS_UWP
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

public class VideoCaptureManager : MonoBehaviour {

    //"Injected" objects.
    VideoCapture _videoCapture;
    IntPtr _spatialCoordinateSystemPtr;

    Matrix4x4 cameraToWorld;
    Matrix4x4 projection;

    void Start ()
    {
        return;
        var task = SocketManager.Instance.CreateSocketClient();

        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = WorldManager.GetNativeISpatialCoordinateSystemPtr();

        //Call this in Start() to ensure that the CameraStreamHelper is already "Awake".
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        //You could also do this "shortcut":
        //CameraStreamManager.Instance.GetVideoCaptureAsync(v => videoCapture = v);
    }

    private void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }

    void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Debug.LogError("Did not find a video capture object. You may not be using the HoloLens.");
            return;
        }

        this._videoCapture = videoCapture;

        //Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystemPtr);

        var resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float frameRate = CameraStreamHelper.Instance.GetLowestFrameRate(resolution);
        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        //You don't need to set all of these params.
        //I'm just adding them to show you that they exist.
        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = resolution.height;
        cameraParams.cameraResolutionWidth = resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParams.rotateImage180Degrees = false; //If your image is upside down, remove this line.
        cameraParams.enableHolograms = false;

        Debug.Log("Camera Resolution: " + cameraParams.cameraResolutionHeight + " " + cameraParams.cameraResolutionWidth + "\n Camera Frame Rate " + cameraParams.frameRate);

        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

#if WINDOWS_UWP
    // Everything above here is boilerplate from the VideoPanelApp.cs project
    bool frameProccessed = true;
    int cnt_in = 0;
    int cnt_out = 0;
    async void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        if (frameProccessed == false)
        {
            cnt_out += 1;
            return;
        }
        cnt_in += 1;
        Debug.Log("cnt : in = " + cnt_in.ToString() + ", out = " + cnt_out);
        frameProccessed = false;
        Debug.Log("Frame sample acquired");
        bool mappable = true;
        float[] cameraToWorldMatrixAsFloat;
        float[] projectionMatrixAsFloat;
        mappable &= sample.TryGetCameraToWorldMatrix(out cameraToWorldMatrixAsFloat);
        mappable &= sample.TryGetProjectionMatrix(out projectionMatrixAsFloat);

        //when copying the bytes out of the buffer, you must supply a byte[] that is appropriately sized.
        //you can reuse this byte[] until you need to resize it(for whatever reason).
        byte[] latestImageBytes = null;

        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        st.Start();
        using (var ms = new InMemoryRandomAccessStream())
        {
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms);
            encoder.SetSoftwareBitmap(sample.Bitmap);
            try
            {
                await encoder.FlushAsync();
            }
            catch (Exception err)
            {
                Debug.LogError(err.Message);
                return;
            }
            latestImageBytes = new byte[ms.Size];
            await ms.ReadAsync(latestImageBytes.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
        }
        st.Stop();
        Debug.Log("encoding time " + st.ElapsedMilliseconds.ToString());

        // Right now we pass things across the pipe as a float array then convert them back into UnityEngine.Matrix using a utility method
        if (mappable)
        {
            st.Restart();
            cameraToWorld = CameraStreamHelper.ConvertFloatArrayToMatrix4x4(cameraToWorldMatrixAsFloat);
            projection = CameraStreamHelper.ConvertFloatArrayToMatrix4x4(projectionMatrixAsFloat);
            await SocketManager.Instance.SendPhoto(latestImageBytes);
            st.Stop();
            Debug.Log("network time " + st.ElapsedMilliseconds.ToString());
            BoundingBox[] boxes = await SocketManager.Instance.RecvDetections();
            SceneUnderstanding.Instance.RecvDetections(cameraToWorld, projection, boxes, mappable);
        }
        frameProccessed = true;
    }
#else
    private void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
    }
#endif
}
