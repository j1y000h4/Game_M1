using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMotionBase : InitBase
{
    // 최대한 공통적인 부분을 Base 스크립트에 추가해서 상속시켜주기
    public Vector3 StartPosition { get; private set; }
    public Vector3 EndPosition { get;private set; }
    public bool LookAtTarget { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }
    protected Action EndCallback { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    protected void SetInfo(int projectileTemplateID, Vector3 spawnPosition, Vector3 targetPosition, Action endCallback = null)
    {
        
    }
}
