using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TitleScene : UI_Scene
{
    enum GameObjects
    {
        StartImage
    }

    enum Texts
    {
        DisplayText
    }

    public override bool Init()
    {
        if( base.Init() == false)
        {
            return false;
        }

        // UI 프레임워크 이해하기
        // 먼저 바인드를 해서 내부 코드를 통해 알아서 이름이랑 맵핑을 통해 변수를 만들어주고, 변수를 다시 사용할 때는 GetObject를 이용해서 사용하기
        BindObejcts(typeof(GameObjects));
        BindTexts(typeof(Texts));

        // StartImage를 눌렀으면, 씬을 로드하는 기능을 만들어 주겠다.
        GetObject((int)GameObjects.StartImage).BindEvent((evt) =>
        {
            Debug.Log("ChangeScene");
            Managers.sceneManager.LoadScene(Define.EScene.GameScene);
        });

        GetObject((int)GameObjects.StartImage).gameObject.SetActive(false);
        GetText((int)Texts.DisplayText).text = $"";

        StartLoadAssets();

        return true;
    }

    void StartLoadAssets()
    {
        Managers.resourceManager.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                GetObject((int)GameObjects.StartImage).gameObject.SetActive(true);
                GetText((int)Texts.DisplayText).text = "Touch To Start";
            }
        });
    }
}
