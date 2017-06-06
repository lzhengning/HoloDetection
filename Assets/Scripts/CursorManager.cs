using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    public BoundingBox cursorBox { get; private set; }

    private RaycastHit hitInfo;
    public GameObject gazeUiObject { get; private set; }

    public Transform cursorTransform
    {
        get { return gameObject.transform; }
    }

    void Start()
    {
        cursorBox = null;
    }

    void Update()
    {
        cursorBox = SceneUnderstanding.Instance.GetBoundingBox(transform.position);
        Vector3 gazeOrigin = GazeManager.Instance.GazeOrigin;
        Vector3 gazeNormal = GazeManager.Instance.GazeNormal;
        bool hit = Physics.Raycast(gazeOrigin, gazeNormal, out hitInfo, 10.0f, LayerMask.GetMask("UI"));
        gazeUiObject = hit ? hitInfo.collider.gameObject : null;
    }
}
