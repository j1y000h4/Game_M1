using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Stun : CCBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        EffectType = EEffectType.CrowdControl;
        return true;
    }
}
