using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class SignManager : Singleton<SignManager> {
    public GameObject signText;
    private List<GameObject> signs;

    private void Start()
    {
        signs = new List<GameObject>();
    }

    public void UpdateSigns () {
        if (signs != null)
        {
            foreach (var sign in signs)
                Destroy(sign);
        }
        signs.Clear();

        foreach(var box in VidManager.Instance.Boxes)
        {
            GameObject sign = Instantiate(signText);
            sign.GetComponent<SignController>().box = box;
            signs.Add(sign);
        }
    }

    public float GetSignDepth(BoundingBox box)
    {
        float depth = -1;
        int ind = VidManager.Instance.Boxes.IndexOf(box);
        if (ind >= 0)
        {
            return signs[ind].GetComponent<SignController>().distance;
        }
        return depth;
    }
}
