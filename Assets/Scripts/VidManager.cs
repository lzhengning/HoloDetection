using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

class ContextWindow
{
    const int kWindowSize = 5;

    List<float> frames;

    public ContextWindow(float x)
    {
        frames = new List<float>();
        frames.Add(x);
    }

    public void Update()
    {
        frames.Add(0);
        if (frames.Count > kWindowSize)
        {
            frames.RemoveAt(0);
        }
    }

    public void Add(float x)
    {
        frames[frames.Count - 1] += x;
    }

    public float mean()
    {
        float result = 0;
        foreach (var f in frames)
            result += f;
        return result / kWindowSize;
    }

    public bool full()
    {
        return frames.Count == kWindowSize ? true : false;
    }
}


public class VidManager : Singleton<VidManager> {
    const float kUpperBound = 0.5f;
    const float kLowerBound = 0.10f;

    public List<BoundingBox> Boxes { get { return boxes; } }
    private List<BoundingBox> boxes;
    private Dictionary<string, ContextWindow> ctx;


    void Start()
    {
        boxes = new List<BoundingBox>();
        ctx = new Dictionary<string, ContextWindow>();
    }

    int cnt = 0;

    private List<BoundingBox> ContextSuppression(BoundingBox[] newBoxes)
    {
        cnt += 1;
        // Update Context
        foreach (var cw in ctx.Keys)
        {
            ctx[cw].Update();
        }
        foreach (var box in newBoxes)
        {
            var name = box.name;
            if (ctx.ContainsKey(name))
            {
                ctx[name].Add(box.prob);
            }
            else
            {
                ctx[name] = new ContextWindow(box.prob);
            }
        }
        var result = new List<BoundingBox>();

        Debug.LogFormat("In frame {0}, detect {1} box", cnt, newBoxes.Length); ;
        foreach(var box in newBoxes)
            if (box.prob > kUpperBound)
            {
                result.Add(box);
                Debug.Log(box.name + " " + box.prob);
            }
            else if (ctx[box.name].full() == true && ctx[box.name].mean() > kLowerBound)
            {
                result.Add(box);
                Debug.Log(box.name + " " + box.prob);
            }
            else
            {
                Debug.Log("Suppressed : " + box.name + " , " + box.prob);
            }

        return result;
    }

    public void UpdateDections(BoundingBox[] newBoxes)
    {
        boxes = ContextSuppression(newBoxes);
    }
}
