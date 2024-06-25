using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 딱 하나만 배치
public class UI_Scene : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        //Managers.uiManager.SetCanvas(gameObject, false);
        return true;
    }
}
