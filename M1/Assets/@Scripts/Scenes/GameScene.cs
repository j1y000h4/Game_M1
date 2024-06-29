using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        SceneType = EScene.GameScene;

        GameObject map = Managers.resourceManager.Instantiate("BaseMap");
        map.transform.position = Vector3.zero;
        map.name = "@BaseMap";

        Hero hero = Managers.objectManager.Spawn<Hero>(Vector3.zero);
        hero.CreatureState = ECreatureState.Move;

        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        camera.Target = hero;

        Managers.uiManager.ShowBaseUI<UI_Joystick>();

        // Todo

        return true;
    }

    public override void Clear()
    {
        
    }
}
