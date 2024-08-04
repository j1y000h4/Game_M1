using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class AreaAttack : AreaSkill
{
    // 스킬별로 다를것!
    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);

        _angleRange = 90;

        AddIndicatorComponent();

        if (_indicator != null)
        {
            _indicator.SetInfo(Owner, SkillData, EIndicatorType.Cone);
        }
    }

    public override void DoSkill()
    {
        base.DoSkill();

        SpawnSpellIndicator();
    }
}
