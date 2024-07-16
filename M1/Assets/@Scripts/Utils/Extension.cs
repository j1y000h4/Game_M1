using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    public static bool IsValid(this BaseObject bo)
    {
        if (bo == null || bo.isActiveAndEnabled == false)
        {
            return false;
        }

        Creature creature = bo as Creature;
        if (creature != null)
        {
            return creature.CreatureState != Define.ECreatureState.Dead;
        }

        return true;
    }

    // 비트플래그란.. int [............] bool형이 32개를 사용하는 효과 / 인트는 32비트니까
    public static void MakeMask(this ref LayerMask mask, List<Define.ELayer> list)
    {
        foreach (Define.ELayer layer in list)
        {
            mask |= (1 << (int)layer);
        }
    }

    public static void AddLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask |= (1 << (int)layer);
    }

    public static void RemoveLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask &= ~(1 << (int)layer);
    }

    public static void DestroyChilds(this GameObject go)
    {
        foreach(Transform child in go.transform)
        {
            Managers.resourceManager.Destory(child.gameObject);
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
