using HoloLensCameraStream;
using System;
using System.Linq;
using UnityEngine;

public class CameraStreamHelper : MonoBehaviour
{
    event OnVideoCaptureResourceCreatedCallback VideoCaptureCreated;

    static VideoCapture videoCapture;

    static CameraStreamHelper instance;
    public static CameraStreamHelper Instance
    {
        get
        {
            return instance;
        }
    }

    public void SetNativeISpatialCoordinateSystemPtr(IntPtr ptr)
    {
        videoCapture.WorldOriginPtr = ptr;
    }

    public void GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback onVideoCaptureAvailable)
    {
        if (onVideoCaptureAvailable == null)
        {
            Debug.LogError("You must supply the onVideoCaptureAvailable delegate.");
        }

        if (videoCapture == null)
        {
            VideoCaptureCreated += onVideoCaptureAvailable;
        }
        else
        {
            onVideoCaptureAvailable(videoCapture);
        }
    }

    public HoloLensCameraStream.Resolution GetHighestResolution()
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedResolutions().OrderByDescending((r) => r.width * r.height).FirstOrDefault();
    }

    public HoloLensCameraStream.Resolution GetLowestResolution()
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedResolutions().OrderBy((r) => r.width * r.height).FirstOrDefault();
    }

    public float GetHighestFrameRate(HoloLensCameraStream.Resolution forResolution)
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        var frameRates = videoCapture.GetSupportedFrameRatesForResolution(forResolution);
        Debug.Log("support frames");
        foreach (var rate in frameRates)
        {
            Debug.Log(rate);
        }
        return videoCapture.GetSupportedFrameRatesForResolution(forResolution).OrderByDescending(r => r).FirstOrDefault();
    }

    public float GetLowestFrameRate(HoloLensCameraStream.Resolution forResolution)
    {
        if (videoCapture == null)
        {
            throw new Exception("Please call this method after a VideoCapture instance has been created.");
        }
        return videoCapture.GetSupportedFrameRatesForResolution(forResolution).OrderBy(r => r).FirstOrDefault();
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Cannot create two instances of CamStreamManager.");
            return;
        }

        instance = this;
        VideoCapture.CreateAync(OnVideoCaptureInstanceCreated);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnVideoCaptureInstanceCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Debug.LogError("Creating the VideoCapture object failed.");
            return;
        }

        CameraStreamHelper.videoCapture = videoCapture;
        if (VideoCaptureCreated != null)
        {
            VideoCaptureCreated(videoCapture);
        }
    }

    /// <summary>
    /// Helper method for converting into UnityEngine.Matrix4x4
    /// </summary>
    /// <param name="matrixAsArray"></param>
    /// <returns></returns>
    public static Matrix4x4 ConvertFloatArrayToMatrix4x4(float[] matrixAsArray)
    {
        //There is probably a better way to be doing this but System.Numerics.Matrix4x4 is not available 
        //in Unity and we do not include UnityEngine in the plugin.
        Matrix4x4 m = new Matrix4x4();
        m.m00 = matrixAsArray[0];
        m.m01 = matrixAsArray[1];
        m.m02 = matrixAsArray[2];
        m.m03 = matrixAsArray[3];
        m.m10 = matrixAsArray[4];
        m.m11 = matrixAsArray[5];
        m.m12 = matrixAsArray[6];
        m.m13 = matrixAsArray[7];
        m.m20 = matrixAsArray[8];
        m.m21 = matrixAsArray[9];
        m.m22 = matrixAsArray[10];
        m.m23 = matrixAsArray[11];
        m.m30 = matrixAsArray[12];
        m.m31 = matrixAsArray[13];
        m.m32 = matrixAsArray[14];
        m.m33 = matrixAsArray[15];

        return m;
    }
}
