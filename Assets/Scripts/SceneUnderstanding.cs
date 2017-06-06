using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox
{
    public float x;
    public float y;
    public float w;
    public float h;
    public int imageHeight;
    public int imageWidth;
    public float prob;
    public string name;
    public Vector3 cameraPos;
    public bool mapped;
    public Vector3 pos;
    public Vector3 posLU;
    public Vector3 posRU;
    public Vector3 posLD;
    public Vector3 posRD;
}

public class SceneUnderstanding : Singleton<SceneUnderstanding> {
    public List<BoundingBox> Boxes { get { return VidManager.Instance.Boxes; } }
    public Vector3 cameraPosition { get; private set; }
    public Vector3 cameraRay { get; private set; }
    public bool mappable { get; private set; }

    private Matrix4x4 cameraToWorld;
    private Matrix4x4 worldToCamera;
    private Matrix4x4 projection;
    private BoundingBox[] tempBox;
    private bool getNewDetections = false;
    
    public Vector2 AppCoordinateSystemToPixel(Vector3 position)
    {
        Vector4 worldSpacePos = new Vector4(position.x, position.y, position.z, 1);
        Vector4 cameraSpacePos = worldToCamera * worldSpacePos;
        Vector4 imagePosUnnormalized = projection * cameraSpacePos;
        Vector2 imagePosProjected = new Vector2(imagePosUnnormalized.x / imagePosUnnormalized.w,
                                                imagePosUnnormalized.y / imagePosUnnormalized.w);
        Vector2 imagePosZeroToOne = imagePosProjected * 0.5f + new Vector2(0.5f, 0.5f);
        Vector2 pixelPos = new Vector2(imagePosZeroToOne.x, 1 - imagePosZeroToOne.y);
        return pixelPos;
    }

    public Vector3 PixelToAppCoordinateSystem(Vector2 pixelPos)
    {
        Vector2 imagePosZeroToOne = new Vector2(pixelPos.x, 1 - pixelPos.y);
        Vector2 imagePosProjected = imagePosZeroToOne * 2 - new Vector2(1, 1);
        Vector3 cameraSpacePos = UnProjectVector(projection, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));
        return cameraToWorld.MultiplyPoint(cameraSpacePos);
    }

    public Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = new Vector3(0, 0, 0);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }

    void TestPixelToAppCoordinateSystem()
    {
        Vector3 vec3 = PixelToAppCoordinateSystem(new Vector2(0, 0));
        Vector2 vec2 = AppCoordinateSystemToPixel(vec3);
        Debug.LogError("vec2 = " + vec2.ToString());
        if (vec2.SqrMagnitude() > 1e-5)
        {
            Debug.LogError("Error : in test coordinate mapping.");
        }
    }

    public BoundingBox GetBoundingBox(Vector3 point)
    {
        if (Boxes == null || Boxes.Count == 0 || mappable == false)
            return null;
        Vector2 pixel = AppCoordinateSystemToPixel(point);
        BoundingBox result = null;
        foreach (var box in Boxes)
            if (box.x - box.w / 2 < pixel.x && box.x + box.w / 2 > pixel.x &&
                box.y - box.h / 2 < pixel.y && box.y + box.h / 2 > pixel.y &&
                (result == null || box.w * box.h < result.w * result.h))
                result = box;
        return result;
    }

    public void RecvDetections(Matrix4x4 cameraToWorld, Matrix4x4 projection, BoundingBox[] boxes, bool mappable)
    {
        this.mappable = mappable;
        if (mappable == false)
            Debug.LogError("Cannot get transform matrix.");

        this.cameraToWorld = cameraToWorld;
        this.projection = projection;

        worldToCamera = cameraToWorld.inverse;
        cameraPosition = cameraToWorld * new Vector4(0, 0, 0, 1);

        cameraRay = (PixelToAppCoordinateSystem(new Vector2(0, 0)) - cameraPosition).normalized;

        foreach (var box in boxes)
        {
            box.cameraPos = cameraPosition;
            box.mapped = false;
        }

        // Events
        tempBox = boxes;
        getNewDetections = true;
    }

    void Update()
    {
        if (getNewDetections)
        {
            VidManager.Instance.UpdateDections(tempBox);
            SignManager.Instance.UpdateSigns();
            getNewDetections = false;
        }
    }
}
