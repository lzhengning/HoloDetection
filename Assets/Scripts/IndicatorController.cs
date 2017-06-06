using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

public class IndicatorController : MonoBehaviour {
    public GameObject LU;
    public GameObject RU;
    public GameObject LD;
    public GameObject RD;

    const float kR = 2f;
    float[] rw = null;
    float[] rh = null;
    GameObject[] indicators = null;

    void Start()
    {
        indicators = new GameObject[4] { LU, RU, LD, RD };
        rw = new float[4] { -kR, +kR, -kR, +kR };
        rh = new float[4] { -kR, -kR, +kR, +kR };
    }

    void Update()
    {
        BoundingBox box = CursorManager.Instance.cursorBox;
        if (box == null || CursorManager.Instance.gazeUiObject != null)
        {
            for (int i = 0; i < indicators.Length; ++i)
                indicators[i].SetActive(false);
            return;
        }
        for (int i = 0; i < indicators.Length; ++i)
            indicators[i].SetActive(true);
        Vector3 rayPos = SceneUnderstanding.Instance.cameraPosition;
        Vector3 rayVec = SceneUnderstanding.Instance.PixelToAppCoordinateSystem(new Vector2(box.x, box.y));
        Plane indicatorPlane = new Plane(CursorManager.Instance.cursorTransform.position, rayVec);

        // Place the indicators
        float distance = SignManager.Instance.GetSignDepth(box);
        if (distance < 0)
            distance = 1.0f;
        Vector3 vecO = SceneUnderstanding.Instance.cameraPosition;
        for (int i = 0; i < indicators.Length; ++i)
        {
            Vector2 pos2d = new Vector2(box.x + box.w / rw[i], box.y + box.h / rh[i]);
            Vector3 vec3d = SceneUnderstanding.Instance.PixelToAppCoordinateSystem(pos2d);
            Vector3 vecRay = (vec3d - vecO).normalized;
            indicators[i].transform.position = vecO + vecRay * distance;
            indicators[i].transform.rotation = Quaternion.LookRotation(vecRay, Vector3.up);
        }
    }
}
