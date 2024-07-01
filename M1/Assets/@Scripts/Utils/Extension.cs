using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    // GameObject에다가 BindEvent함수를 붙이겠다.
    public static void BindEvent(this GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        UI_Base.BindEvent(go, action, type);
    }

    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }

    public static void DestroyChilds(this GameObject go)
    {
        foreach(Transform child in go.transform)
        {
            Managers.resourceManager.Destory(child.gameObject);
        }
    }

    public static void TranslateEx(this Transform transform, Vector3 dir)
    {
        BaseObject bo = transform.GetComponent<BaseObject>();
        if(bo != null)
        {
            bo.TranslateEx(dir);
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
