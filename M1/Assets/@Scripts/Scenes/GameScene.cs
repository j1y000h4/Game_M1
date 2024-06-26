using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    public override bool Init()
    {
        if(base.Init() == false)
        {
            return false;
        }

        SceneType = EScene.GameScene;

        GameObject map = Managers.resourceManager.Instantiate("BaseMap");
        map.transform.position = Vector3.zero;
        map.name = "@BaseMap";
        

        // Todo

        return true;
    }

    public override void Clear()
    {
        
    }
}
