using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

public abstract class SkillBase : InitBase
{
    public Creature Owner { get; protected set; }
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
            Owner.SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnAnimEventHandler;

            // 스킬 애니메이션 시작할때의 이벤트 핸들러
            Owner.SkeletonAnim.AnimationState.Complete -= OnAnimCompleteHandler;
            Owner.SkeletonAnim.AnimationState.Complete += OnAnimCompleteHandler;
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

        Owner.SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
        Owner.SkeletonAnim.AnimationState.Complete -= OnAnimCompleteHandler;
    }

    public virtual void DoSkill()
    {

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

        projectile.SpawnInfo(Owner, this, exclueMask);
    }

    protected abstract void OnAnimEventHandler(TrackEntry trackEntry, Event e);
    protected abstract void OnAnimCompleteHandler(TrackEntry trackEntry);
}
