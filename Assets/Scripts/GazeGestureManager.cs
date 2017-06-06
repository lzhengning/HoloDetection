using HoloToolkit.Unity.InputModule;
using TMPro;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class GazeGestureManager : MonoBehaviour {

    private GestureRecognizer recognizer;

    public GameObject cursor;
    public GameObject actions;
    public GameObject searchButton;
    public GameObject translateButton;
    public GameObject cancelButton;
    public GameObject boardQuad;
    public TextMesh translation;
    public GameObject boardText;

    void Start ()
    {
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.TappedEvent += TappedEventHandler;
        recognizer.StartCapturingGestures();

        translation.text = "";
	}

    void TappedEventHandler(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        GameObject hitObject = CursorManager.Instance.gazeUiObject;
        if (actions.activeSelf == false && CursorManager.Instance.cursorBox != null)
        {
            actions.SetActive(true);
            boardQuad.SetActive(false);
            boardText.SetActive(false);
            translation.text = "";
            actions.transform.position = cursor.transform.position + Camera.main.transform.right * 0.15f;
            actions.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
            ActionManager.Instance.actionItem = CursorManager.Instance.cursorBox.name;
        }
        else if (actions.activeSelf == true)
        {
            if (hitObject != null)
            {
                if (hitObject == searchButton)
                {
                    searchButton.SetActive(false);
                    translateButton.SetActive(false);
                    cancelButton.SetActive(false);
                    boardQuad.SetActive(true);
                    boardText.SetActive(true);
                    boardText.GetComponent<TextMeshPro>().SetText(ActionManager.Instance.ItemWiki);
                    translation.text = "";
                }
                if (hitObject == translateButton)
                {
                    if (translation.text.Equals(""))
                    {
                        translation.text = ActionManager.Instance.Itemtranslation;
                    }
                    else
                    {
                        translation.text = "";
                    }
                }
                if (hitObject == cancelButton)
                {
                    actions.SetActive(false);
                    translation.text = "";
                }
                if (hitObject == boardQuad)
                {
                    searchButton.SetActive(true);
                    translateButton.SetActive(true);
                    cancelButton.SetActive(true);
                    boardQuad.SetActive(false);
                    boardText.SetActive(false);
                    translation.text = "";
                }
            }
        }
    }
}
