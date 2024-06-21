using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        SceneType = Define.EScene.TitleScene;

        StartLoadAssets();

        return true;
    }

    // Start is called before the first frame update
    void StartLoadAssets()
    {
        Managers.resourceManager.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {

            }
        });
    }

    public override void Clear()
    {
        
    }
}
