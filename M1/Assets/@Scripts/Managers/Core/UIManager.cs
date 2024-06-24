using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager
{
    private int _order = 10;

    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

    private UI_Scene _sceneUI = null;
    public UI_Scene SceneUI
    {
        set { _sceneUI = value; }
        get { return _sceneUI; }
    }

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject { name = "@UI_Root" };
            }
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        if(canvas == null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
        }

        CanvasScaler cs = go.GetComponent<CanvasScaler>();
        if (cs != null) 
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080, 1920);
        }
}
