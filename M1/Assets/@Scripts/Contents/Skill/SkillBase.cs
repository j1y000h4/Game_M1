using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

public abstract class SkillBase : InitBase
{
    public Creature Owner { get; protected set; }
    public float RemainCoolTime {  get; protected set; }
    public Data.SkillData SkillData { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    // 스킬 베이스이기 때문에 베이스 클래스가 들고 있을 것 같은거는 당연히 오너가 있어야함
    // 데이터 로딩함수
    public virtual void SetInfo(Creature owner, int skillTemplateID)
    {
        Owner = owner;
        SkillData = Managers.dataManager.SkillDic[skillTemplateID];

        if (Owner.SkeletonAnim != null && Owner.SkeletonAnim.AnimationState != null)
        {
            // 스킬 애니메이션 시작할때의 이벤트 핸들러
            Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnOwnerAnimEventHandler;
        }
    }

    // 스킬이 비활성화 되면
    private void OnDisable()
    {
        if (Managers.gameManager == null)
        {
            return;
        }
        if (Owner.IsValid() == false)
        {
            return;
        }
        if (Owner.SkeletonAnim == null)
        {

        }
        if (Owner.SkeletonAnim.AnimationState == null)
        {
            return;
        }

        Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
    }

    public virtual void DoSkill()
    {
        RemainCoolTime = SkillData.CoolTime;

        // 준비된 스킬 리스트에서 해제 시켜준다.
        if (Owner.Skills != null)
        {
            Owner.Skills.ActiveSkills.Remove(this);
        }

        float timeScale = 1.0f;

        if (Owner.Skills.DefaultSkill == this)
        {
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = timeScale;
        }
        else
        {
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = 1;
        }

        StartCoroutine(CoCountdownCooldown());
    }

    private IEnumerator CoCountdownCooldown()
    {
        RemainCoolTime = SkillData.CoolTime;
        yield return new WaitForSeconds(SkillData.CoolTime);
        RemainCoolTime = 0;

        // 쿨타임이 다 지나면
        // 준비된 스킬에 추가
        if (Owner.Skills != null)
        {
            Owner.Skills.ActiveSkills.Add(this);
        }
    }

    protected virtual void GenerateProjectile(Creature owner, Vector3 spawnPos)
    {
        Projectile projectile = Managers.objectManager.Spawn<Projectile>(spawnPos, SkillData.ProjectileId);

        // 모두랑 충돌
        LayerMask exclueMask = 0;
        // 충돌하기 싫은걸 추가
        exclueMask.AddLayer(Define.ELayer.Default);
        exclueMask.AddLayer(Define.ELayer.Projectile);
        exclueMask.AddLayer(Define.ELayer.Env);
        exclueMask.AddLayer(Define.ELayer.Obstacle);

        switch (owner.CreatureType)
        {
            case Define.ECreatureType.Hero:
                exclueMask.AddLayer(Define.ELayer.Hero);
                break;
            case Define.ECreatureType.Monster:
                exclueMask.AddLayer(Define.ELayer.Monster);
                break;
        }

        projectile.SetSpawnInfo(Owner, this, exclueMask);
    }

    private void OnOwnerAnimEventHandler(TrackEntry trackEntry, Event e)
    {
        // 다른 스킬의 애니메이션 이벤트도 받기 때문에 자기꺼만 써야함
        if (trackEntry.Animation.Name == SkillData.AnimName)
        {
            OnAttackEvent();
        }
    }

    protected abstract void OnAttackEvent();
}
