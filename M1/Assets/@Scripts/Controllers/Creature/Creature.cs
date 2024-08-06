using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class Creature : BaseObject
{
    public BaseObject Target { get;protected set; }
    public SkillComponent Skills { get; protected set; }
    public Data.CreatureData CreatureData { get; protected set; }
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

    // EffectComponent를 Creature에 붙이기. 
    public EffectComponent Effects { get; set; }

    #region Stats
    // 초기값들. Creature의 공통적인 부분들
    // Stat을 두가지로 설정해서 관리. Base와 변경된 Stat들
    public float Hp { get; set; }
    public CreatureStat MaxHp;
    public CreatureStat Atk;
    public CreatureStat CriRate;
    public CreatureStat CriDamage;
    public CreatureStat ReduceDamageRate;
    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; // 쏜즈
    public CreatureStat MoveSpeed;
    public CreatureStat AttackSpeedRate;
    #endregion

    protected float AttackDistance
    {
        get
        {
            float env = 2.2f;
            if (Target != null && Target.ObjectType == EObjectType.Env)
            {
                return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);
            }

            float baseValue = CreatureData.AtkRange;
            return baseValue;
        }
    }

    // state에 따라 애니메이션을 설정하는 게 편하다.
    protected ECreatureState _creatureState = ECreatureState.None;
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        ObjectType = EObjectType.Creature;
        //CreatureState = ECreatureState.Idle;

        return true;
    }

    // 현재 데이터시트 아이디가 무엇인지에 따라 Creature를 셋팅하는 공통 부분
    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;

        if (CreatureType == ECreatureType.Hero)
        {
            CreatureData = Managers.dataManager.HeroDic[templateID];
        }
        else
        {
            CreatureData = Managers.dataManager.MonsterDic[templateID];
        }
        
        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        // Collider
        Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
        RigidBody.mass = 0;

        // Spine
        SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);

        // Skills
        Skills = gameObject.GetOrAddComponent<SkillComponent>();
        Skills.SetInfo(this, CreatureData);

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = new CreatureStat(CreatureData.MaxHp);
        Atk = new CreatureStat(CreatureData.Atk);
        CriRate = new CreatureStat(CreatureData.CriRate);
        CriDamage = new CreatureStat(CreatureData.CriDamage);
        ReduceDamageRate = new CreatureStat(0);
        LifeStealRate = new CreatureStat(0);
        ThornsDamageRate = new CreatureStat(0);
        MoveSpeed = new CreatureStat(CreatureData.MoveSpeed);
        AttackSpeedRate = new CreatureStat(1);

        // State
        CreatureState = ECreatureState.Idle;

        // Effects
        Effects = gameObject.AddComponent<EffectComponent>();
        Effects.SetInfo(this);

        // Map
        StartCoroutine(CoLerpToCellPos());
    }

    protected override void UpdateAnimation()
    {
        switch(CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;

            case ECreatureState.Skill:
                //PlayAnimation(0, AnimName.ATTACK_A, true);
                break;

            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;

            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
                break;
        }
    }

    // Grid 방식으로 가는 이상 ColliderSize를 이용한 이동 로직은 필요없어지게 된다!!
    //public void ChangeColliderSize(EColliderSize size = EColliderSize.Normal)
    //{
    //    switch(size)
    //    {
    //        case EColliderSize.Small:
    //            Collider.radius = CreatureData.ColliderRadius * 0.8f;
    //            break;
    //        case EColliderSize.Normal:
    //            Collider.radius = CreatureData.ColliderRadius;
    //            break;
    //        case EColliderSize.Big:
    //            Collider.radius = CreatureData.ColliderRadius * 1.2f;
    //            break;
    //    }
    //}

    #region AI

    public float UpdateAITick { get; protected set; } = 0.0f;

    // 코루틴을 통해 상황에 따라 업데이트 주기를 조절할 수 있음
    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            // State 기반으로 관리가 핵심
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case ECreatureState.Move:
                    UpdateMove();
                    break;
                case ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            if (UpdateAITick > 0)
            {
                yield return new WaitForSeconds(UpdateAITick);
            }
            else
            {
                yield return null;
            }
        }
    }

    //// bool 형으로 else if 남발하는 것보다 훨씬 깔끔
    //private void Update()
    //{
    //    switch(CreatureState)
    //    {
    //        case ECreatureState.Idle:
    //            UpdateIdle();
    //            break;
    //        case ECreatureState.Move:
    //            UpdateMove();
    //            break;
    //        case ECreatureState.Skill:
    //            UpdateSkill();
    //            break;
    //        case ECreatureState.Dead:
    //            UpdateDead();
    //            break;
    //    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateSkill()
    {
        if (_coWait != null)
        {
            return;
        }

        // 공통적인 부분 (Hero/Monster)
        if (Target.IsValid() == false || Target.ObjectType == EObjectType.HeroCamp)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        Vector3 dir = (Target.CenterPosition - CenterPosition);
        float distToTargetSqr = dir.sqrMagnitude;
        float attackDistanceSqr = AttackDistance * AttackDistance;

        if (distToTargetSqr > attackDistanceSqr)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        // 스킬을 사용할 수 있는 상태
        // DoSkill
        Skills.CurrentSkill.DoSkill();

        LookAtTarget(Target);

        var trackEntry = SkeletonAnim.state.GetCurrent(0);
        float delay = trackEntry.Animation.Duration;

        StartWait(delay);

    }
    protected virtual void UpdateDead() { }
    #endregion

    #region Wait
    protected Coroutine _coWait;
    protected void StartWait(float seconds)
    {
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if (_coWait != null)
        {
            StopCoroutine(_coWait);
        }

        _coWait = null;
    }

    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

        // 예외처리 코드 추가
        if (attacker.IsValid() == false)
        {
            return;
        }

        // Creature로 캐스팅
        Creature creature = attacker as Creature;
        if (creature == null)
        {
            return;
        }

        // Hp를 작성할때 Clamp로 처리해주기
        float finalDamage = creature.Atk.Value;
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value);

        Managers.objectManager.ShowDamageFont(CenterPosition, finalDamage, transform, false);

        if (Hp <= 0)
        {
            OnDead(attacker, skill);
            CreatureState = ECreatureState.Dead;
        }
    }
    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);
    }

    // 가까이에 있는 무언가를 찾아주는 Helper 코드
    // range 범위안에 가장 가까이 있는 objs 찾기
    protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude;

            // 서치 범위보다 멀리 있으면 스킵
            if (distToTargetSqr > searchDistanceSqr)
            {
                continue;
            }
            // 이미 더 좋은 후보를 찾았으면 스킵
            if (distToTargetSqr > bestDistanceSqr)
            {
                continue;
            }

            if ((func != null && func.Invoke(obj) == false))
            {
                continue;
            }

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
    {
        Vector3 dir = (Target.transform.position - transform.position);
        float distToTargetSqr = dir.sqrMagnitude;
        float attackDistanceSqr = attackRange * attackRange;



        // 공격 범위 내로 들어왔다면 공격
        if (distToTargetSqr <= attackDistanceSqr)
        {
            CreatureState = ECreatureState.Skill;
            // 스킬 상태로 판단
            //skill.DoSkill();
            return;
        }
        // 공격 범위 밖이면 추적
        else
        {
            //SetRigidBodyVelocity(dir.normalized * MoveSpeed);
            FindPathAndMoveToCellPos(Target.transform.position, HERO_DEFAULT_MOVE_DEPTH);

            // 너무 멀어지면 포기
            float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                Target = null;
                CreatureState = ECreatureState.Move;
            }

            return;
        }
    }
    #endregion

    #region Misc
    protected bool IsValid(BaseObject bo)
    {
        return bo.IsValid();
    }
    #endregion

    #region Map
    // WorldPos 기반
    public EFindPathResult FindPathAndMoveToCellPos(Vector3 destWorldPos, int maxDepth, bool forceMoveCloser = false)
    {
        Vector3Int destCellPos = Managers.mapManager.World2Cell(destWorldPos);
        return FindPathAndMoveToCellPos(destCellPos, maxDepth, forceMoveCloser);
    }

    // CellPos 기반
    public EFindPathResult FindPathAndMoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
    {
        if (LerpCellPosCompleted == false)
            return EFindPathResult.Fail_LerpCell;

        // A*
        List<Vector3Int> path = Managers.mapManager.FindPath(CellPos, destCellPos, maxDepth);
        if (path.Count < 2)
            return EFindPathResult.Fail_NoPath;

        if (forceMoveCloser)
        {
            Vector3Int diff1 = CellPos - destCellPos;
            Vector3Int diff2 = path[1] - destCellPos;
            if (diff1.sqrMagnitude <= diff2.sqrMagnitude)
                return EFindPathResult.Fail_NoPath;
        }

        Vector3Int dirCellPos = path[1] - CellPos;
        //Vector3Int dirCellPos = destCellPos - CellPos;
        Vector3Int nextPos = CellPos + dirCellPos;

        if (Managers.mapManager.MoveTo(this, nextPos) == false)
            return EFindPathResult.Fail_MoveTo;

        return EFindPathResult.Success;
    }

    public bool MoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
    {
        // LerpCellPosCompleted면 시도 x
        if (LerpCellPosCompleted == false)
            return false;

        return Managers.mapManager.MoveTo(this, destCellPos);
    }

    // 매 프레임마다 내 셀 위치랑 어긋나 있으면 해당 위치로 이동하려고 시도를 하는 코드
    protected IEnumerator CoLerpToCellPos()
    {
        while (true)
        {
            Hero hero = this as Hero;

            // hero의 경우 멀면 멀수록 더 빨리 쫓아올 수 있도록
            if (hero != null)
            {
                float div = 5;
                Vector3 campPos = Managers.objectManager.Camp.Destination.transform.position;
                Vector3Int campCellPos = Managers.mapManager.World2Cell(campPos);
                float ratio = Math.Max(1, (CellPos - campCellPos).magnitude / div);

                LerpToCellPos(CreatureData.MoveSpeed * ratio);
            }
            else
                LerpToCellPos(CreatureData.MoveSpeed);

            yield return null;
        }
    }
    #endregion
}
