using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

// 캐릭터(히어로)가 스킬 하나만 들고 있는게 아니기 때문에 스킬 컴포넌트를 만들어 줌
// 스킬 컴포넌트는 모든 스킬들을 다 들고 있다가 쿨타임 관리도 해주고 지금 사용할 수 있는 스킬만 건네주는 역할을 함
public class SkillComponent : InitBase
{
    // 모든 스킬 목록
    public List<SkillBase> SkillList { get; } = new List<SkillBase> ();
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase> ();

    public SkillBase DefaultSkill { get; private set; }
    public SkillBase EnvSkill { get; private set; }
    public SkillBase ASkill { get; private set; }
    public SkillBase BSkill { get; private set; }

    Creature _owner;

    public SkillBase CurrentSkill
    {
        get
        {

            if (ActiveSkills.Count == 0)
            {
                return DefaultSkill;
            }

            int randomIndex = Random.Range(0, ActiveSkills.Count);
            Debug.Log(" > RandomIndex : " + randomIndex);
            return ActiveSkills[randomIndex];
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    public void SetInfo(Creature owner, CreatureData creatureData)
    {
        _owner = owner;

        AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
        AddSkill(creatureData.EnvSkillId, ESkillSlot.Env);
        AddSkill(creatureData.SkillAId, ESkillSlot.A);
        AddSkill(creatureData.SkillBId, ESkillSlot.B);
    }

    public void AddSkill(int skillTemplateID, ESkillSlot skillSlot)
    {
        // 스킬이 없다는 뜻
        if (skillTemplateID == 0)
        {
            return;
        }

        // 데이터시트에 없는 스킬이 있다면 warning 띄우며 스킵
        if (Managers.dataManager.SkillDic.TryGetValue(skillTemplateID, out var data) == false)
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;
        if (skill == null)
        {
            return;
        }

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);

        switch (skillSlot)
        {
            case ESkillSlot.Default:
                DefaultSkill = skill;
                break;
            case ESkillSlot.Env:
                EnvSkill = skill;
                break;
            case ESkillSlot.A:
                ASkill = skill;
                ActiveSkills.Add(skill);
                break;
            case ESkillSlot.B:
                BSkill = skill;
                ActiveSkills.Add(skill);
                break;
        }
    }
}
