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

        HeroCamp camp = Managers.objectManager.Spawn<HeroCamp>(new Vector3Int(-10, -5, 0), 0);

        // Hero Spawn
        for (int i = 0; i < 5; i++)
        {
            Hero hero = Managers.objectManager.Spawn<Hero>(new Vector3Int(-10 + Random.Range(-5, 5), -5 + Random.Range(-5, 5), 0), HERO_KNIGHT_ID);
        }

        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        camera.Target = camp;

        Managers.uiManager.ShowBaseUI<UI_Joystick>();

        {
            Managers.objectManager.Spawn<Monster>(new Vector3Int(0, 1, 0), MONSTER_BEAR_ID);
            //Managers.objectManager.Spawn<Monster>(new Vector3(1, 1, 0), MONSTER_SLIME_ID);
        }

        {
            Env env = Managers.objectManager.Spawn<Env>(new Vector3(0, 2, 0), ENV_TREE1_ID);
            env.EnvState = EEnvState.Idle;
        }

        // Todo

        return true;
    }

    public override void Clear()
    {
        
    }
}
