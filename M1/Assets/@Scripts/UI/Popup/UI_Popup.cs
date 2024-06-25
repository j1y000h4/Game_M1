using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 계속 여러 개를 껐다 켰다
public class UI_Popup : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        Managers.uiManager.SetCanvas(gameObject, true);

        return true;
    }

    public virtual void ClosePopupUI()
    {
        Managers.uiManager.ClosePopupUI(this);
    }
}
