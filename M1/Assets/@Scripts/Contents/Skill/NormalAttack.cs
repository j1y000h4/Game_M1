using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class NormalAttack : SkillBase
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();

        Owner.CreatureState = ECreatureState.Skill;
        Owner.PlayAnimation(0, SkillData.AnimName, false);

        Owner.LookAtTarget(Owner.Target);
    }

    protected override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        // 우리가 실행하고자하는 SkillData와 같은지 확인
        if (e.ToString().Contains(SkillData.AnimName))
        {
            OnAttackEvent();
        }
    }

    protected virtual void OnAttackEvent()
    {
        // Target이 유효하지 않으면 그냥 리턴
        if (Owner.Target.IsValid() == false)
        {
            return;
        }

        if (SkillData.ProjectileId == 0)
        {
            // Melee
            Owner.Target.OnDamaged(Owner, this);
        }
        else
        {
            // Ranged
            GenerateProjectile(Owner, Owner.CenterPosition);
        }

    }

    protected override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if (Owner.Target.IsValid() == false)
        {
            return;
        }

        if (Owner.CreatureState == ECreatureState.Skill)
        {
            Owner.CreatureState = ECreatureState.Move;
        }
    }
}
