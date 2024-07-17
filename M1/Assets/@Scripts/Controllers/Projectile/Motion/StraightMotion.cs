using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightMotion : ProjectileMotionBase
{
    // TODO

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    // 어떤 모션은 SetInfo가 달라질 수도 있으니..
    public new void SetInfo(int projectileTemplateID, Vector3 spawnPosition, Vector3 targetPosition, Action endCallback = null)
    {
        base.SetInfo(projectileTemplateID, spawnPosition, targetPosition, EndCallback);
    }

    protected override IEnumerator CoLaunchProjectile()
    {
        float journeyLength = Vector3.Distance(StartPosition, TargetPosition);
        float totalTime = journeyLength / ProjectileData.ProjSpeed;
        float elapsedTime = 0;

        // StartPosition에서 TargetPosition까지 계속해서 업데이트 하면서 가는
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            float normalizedTime = elapsedTime / totalTime;
            transform.position = Vector3.Lerp(StartPosition, TargetPosition, normalizedTime);


            if (LookAtTarget)
            {
                LookAt2D(TargetPosition - StartPosition);
            }

            yield return null;
        }

        transform.position = TargetPosition;
        EndCallback?.Invoke();
    }
}
