using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class ActionButton : MonoBehaviour {
    public string buttonSelected;
    public string buttonUnselected;

    Renderer render;
    Texture txtSelected;
    Texture txtUnselected;
    Texture txtNow;

	// Use this for initialization
	void Start () {
        render = GetComponent<Renderer>();
        txtSelected = Resources.Load(buttonSelected) as Texture;
        txtUnselected = Resources.Load(buttonUnselected) as Texture;
        txtNow = txtUnselected;
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("Update in Button");
		if (GazeManager.Instance.HitObject == gameObject)
        {
            if (txtNow != txtSelected)
            {
                render.material.mainTexture = txtSelected;
                txtNow = txtSelected;
            }
        }
        else
        {
            if (txtNow != txtUnselected)
            {
                render.material.mainTexture = txtUnselected;
                txtNow = txtUnselected;
            }
        }
	}
}
