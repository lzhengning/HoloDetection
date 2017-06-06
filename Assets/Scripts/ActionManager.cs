using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionManager : Singleton<ActionManager>
{
    public Dictionary<string, string> names { get; private set; }
    public Dictionary<string, string> trans { get; private set; }
    public Dictionary<string, string> wikis { get; private set; }

    public TextMeshPro boardText;
    private string itemNow;

    public string actionItem
    {
        get { return itemNow; }
        set
        {
            if (names.ContainsKey(value))
                boardText.SetText(wikis[value]);
            else
                boardText.SetText("Not Found");
            itemNow = value;
        }
    }

    public string ItemName
    {
        get { return names[itemNow]; }
    }

    public string Itemtranslation
    {
        get { return trans[itemNow]; }
    }

    public string ItemWiki
    {
        get { return wikis[itemNow]; }
    }

    void Start()
    {
        wikis = new Dictionary<string, string>();
        names = new Dictionary<string, string>();
        trans = new Dictionary<string, string>();

        TextAsset text = Resources.Load("wiki") as TextAsset;
        string[] lines = text.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i += 4)
        {
            names.Add(lines[i], lines[i + 1]);
            trans.Add(lines[i], lines[i + 2]);
            wikis.Add(lines[i], lines[i + 3]);
        }

        gameObject.SetActive(false);
    }
}
