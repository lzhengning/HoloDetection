using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignController : MonoBehaviour {
    public BoundingBox box { private get; set; }
    public float distance { get; private set; }


    private TextMesh text = null;
    private float kDefaultDistance = 2.0f;
    private float kMaxDistance = 10.0f;

	// Use this for initialization
	void Start () {
        text = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
        if (box != null)
        {
            text.text = box.name;
            Vector2 vec2d = new Vector2(box.x, box.y);
            Vector3 vec3d = SceneUnderstanding.Instance.PixelToAppCoordinateSystem(vec2d);
            Vector3 vecRay = (vec3d - box.cameraPos).normalized;

            RaycastHit hitInfo;
            if (Physics.Raycast(box.cameraPos, vecRay, out hitInfo, kMaxDistance))
            {
                Vector3 vecUp = Vector3.up;
                Vector3 planeNormal = Vector3.Cross(vecRay, vecUp).normalized;
                Vector3 normal = hitInfo.normal;
                if (Vector3.Dot(normal, vecRay) > 0)
                {
                    normal = -normal;
                }
                Vector3 vecRes = Vector3.Dot(planeNormal, normal) * planeNormal;
                Vector3 projection = normal - vecRes;
                transform.position = box.cameraPos + vecRay * hitInfo.distance;
                transform.rotation = Quaternion.LookRotation(-projection, Vector3.up);
                distance = hitInfo.distance;
                box.pos = transform.position;
                box.mapped = true;
            }
            else
            {
                transform.position = box.cameraPos + vecRay * kDefaultDistance;
                transform.rotation = Quaternion.LookRotation(vecRay, Vector3.up);
                distance = kDefaultDistance;
            }
        }
    }
}
