using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CursorTextController : MonoBehaviour {
    TextMesh textMesh;

	void Start ()
    {
        textMesh = gameObject.GetComponent<TextMesh>();
	}

    void Update()
    {
        if (CursorManager.Instance.cursorBox != null && CursorManager.Instance.gazeUiObject == null)
        {
            textMesh.text = ActionManager.Instance.names[CursorManager.Instance.cursorBox.name];
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            transform.position = CursorManager.Instance.cursorTransform.position + Camera.main.transform.right * 0.05f;
        }
        else
        {
            textMesh.text = "";
        }
        textMesh.text = "";
    }
}
