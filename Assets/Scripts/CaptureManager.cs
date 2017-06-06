using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA.WebCam;
using HoloToolkit.Unity;

public class CaptureManager : Singleton<CaptureManager> {

    PhotoCapture photoCaptureObject = null;
    bool onPhotoMode = false;
    bool isCapturing = false;
    bool stopCapturing = false;

    protected CaptureManager() { }

    void Start ()
    {
        var task = SocketManager.Instance.CreateSocketClient();
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void Update ()
    {
        if (onPhotoMode == true && isCapturing == false)
        {
            isCapturing = true;
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        if (stopCapturing == true)
        {
            onPhotoMode = false;
            stopCapturing = false;
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        Debug.LogWarning("Photo Capture created");
        photoCaptureObject = captureObject;

        foreach(var res in PhotoCapture.SupportedResolutions)
        {
            Debug.LogWarning(String.Format("resolution : {0} * {1}", res.width, res.height));
        }

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => -res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.PNG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            onPhotoMode = true;
            Debug.LogWarning("Start photo mode");
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

#if WINDOWS_UWP

    int cnt = 0;
    private async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    { 
        if (result.success)
        {          
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            // Get the transform matrix
            Matrix4x4 cameraToWorld = new Matrix4x4();
            Matrix4x4 projection = new Matrix4x4();

            bool mappable = true;
            mappable &= photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorld);
            mappable &= photoCaptureFrame.TryGetProjectionMatrix(out projection);

            // Upload the locatable photo & Download the detection results
            await SocketManager.Instance.SendPhoto(imageBufferList.ToArray());
            BoundingBox[] boxes = await SocketManager.Instance.RecvDetections();
            SceneUnderstanding.Instance.RecvDetections(cameraToWorld, projection, boxes, mappable);

            isCapturing = false;
            stopCapturing = false;
            cnt += 1;
            if (cnt == 50)
            {
                stopCapturing = true;
            }
        }
    }
#else
    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    { }
#endif

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}
