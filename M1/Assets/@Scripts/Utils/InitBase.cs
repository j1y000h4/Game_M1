using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBase : MonoBehaviour
{
    protected bool _init = false;

    public virtual bool Init()
    {
        // 한번이라도 초기화 했으면 false
        if (_init)
        {
            return false;
        }

        // 아니면 true
        _init = true;
        return true;
    }

    private void Awake()
    {
        Init();
    }
}
