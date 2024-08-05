using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// 즉발? 지속?
public class EffectBase : BaseObject
{
    public Creature Owner;
    public EffectData EffectData;
    public EEffectType EffectType;
}
