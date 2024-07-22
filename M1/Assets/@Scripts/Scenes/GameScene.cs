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

        Managers.mapManager.LoadMap("BaseMap");

        HeroCamp camp = Managers.objectManager.Spawn<HeroCamp>(Vector3.zero, 0);
        camp.SetCellPos(new Vector3Int(0, 0, 0), true);

        // Hero Spawn
        for (int i = 0; i < 10; i++)
        {
            int heroTemplateID = HERO_WIZARD_ID + Random.Range(0, 5);

            //int heroTemplateID = HERO_KNIGHT_ID;
            //int heroTemplateID = HERO_WIZARD_ID;

            Vector3Int randCellPos = new Vector3Int(0 + Random.Range(-3, 3), 0 + Random.Range(-3, 3), 0);

            // 내가 갈 수 있는 영역인가?
            if (Managers.mapManager.CanGo(randCellPos) == false)
                continue;

            Hero hero = Managers.objectManager.Spawn<Hero>(new Vector3Int(1, 0, 0), heroTemplateID);
            //hero.SetCellPos(randCellPos, true);
            Managers.mapManager.MoveTo(hero, randCellPos, true);

        }

        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        camera.Target = camp;

        Managers.uiManager.ShowBaseUI<UI_Joystick>();

        {
            Monster monster = Managers.objectManager.Spawn<Monster>(new Vector3(1, 1, 0), MONSTER_SLIME_ID);
            Managers.mapManager.MoveTo(monster, new Vector3Int(0, 4, 0), true);
            //Managers.objectManager.Spawn<Monster>(new Vector3(1, 1, 0), MONSTER_GOBLIN_ARCHER_ID);
        }

        {
            //Env env = Managers.objectManager.Spawn<Env>(new Vector3(0, 2, 0), ENV_TREE1_ID);
            //env.EnvState = EEnvState.Idle;
        }

        // Todo

        return true;
    }

    public override void Clear()
    {
        
    }
}
