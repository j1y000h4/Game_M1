using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 캐릭터(히어로)가 스킬 하나만 들고 있는게 아니기 때문에 스킬 컴포넌트를 만들어 줌
// 스킬 컴포넌트는 모든 스킬들을 다 들고 있다가 쿨타임 관리도 해주고 지금 사용할 수 있는 스킬만 건네주는 역할을 함
public class SkillComponent : InitBase
{
    public List<SkillBase> SkillList { get; } = new List<SkillBase> ();

    Creature _owner;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    public void SetInfo(Creature owner, List<int> skillTemplateIDs)
    {
        _owner = owner;

        foreach (int skillTemplateID in skillTemplateIDs)
        {
            AddSkill(skillTemplateID);
        }
    }

    public void AddSkill(int skillTemplateID = 0)
    {
        string className = Managers.dataManager.SkillDic[skillTemplateID].ClassName;

        SkillBase skill = gameObject.AddComponent(Type.GetType(className)) as SkillBase;
        if (skill == null)
        {
            return;
        }

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);
    }

    public SkillBase GetReadySkill()
    {
        return SkillList[0];
    }
}
