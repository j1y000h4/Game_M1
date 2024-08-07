using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class AreaAirborne : AreaSkill
{
    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);

        // 세부적인 내용
        _angleRange = 360;

        if (_indicator != null)
            _indicator.SetInfo(Owner, SkillData, EIndicatorType.Cone);

        _indicatorType = EIndicatorType.Cone;
    }

    public override void DoSkill()
    {
        base.DoSkill();
    }
}
