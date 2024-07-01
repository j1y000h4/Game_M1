using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    bool _needArrange = true;
    public bool NeedArrange
    {
        get { return _needArrange; }
        set
        {
            _needArrange = value;

            // _needArrage == true일 경우에는 Hero들이 산개하고 활동하고 있을 때
            if (value)
            {
                ChangeColliderSize(EColliderSize.Big);
            }
            // _needArrage == false일 경우에는 Hero들이 가만히 있을 때
            else
            {
                TryResizeCollider();
            }
        }
    }

    // 좀 더 잘 뭉치기 위한 
    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if(_creatureState != value)
            {
                base.CreatureState = value;

                if (value == ECreatureState.Move)
                {
                    RigidBody.mass = CreatureData.Mass;
                }
                else
                {
                    RigidBody.mass = CreatureData.Mass * 0.1f;
                }
            }
        }
    }

    EHeroMoveState _heroMoveState = EHeroMoveState.None;
    public EHeroMoveState HeroMoveState
    {
        get { return _heroMoveState; }
        private set
        {
            _heroMoveState = value;
            switch(_heroMoveState)
            {
                case EHeroMoveState.CollectEnv:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.ReturnToCamp:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        CreatureType = ECreatureType.Hero;

        Managers.gameManager.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.gameManager.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        // State
        CreatureState = ECreatureState.Idle;
    }

    public Transform HeroCampDest
    {
        get
        {
            HeroCamp camp = Managers.objectManager.Camp;
            if (HeroMoveState == EHeroMoveState.ReturnToCamp)
            {
                return camp.Pivot;
            }

            return camp.Destination;
        }
    }

    #region AI
    public float SearchDistance { get; private set; } = 8.0f;
    public float AttackDistance
    {
        get
        {
            float targetRadius = (_target.IsValid() ? _target.ColliderRadius : 0);
            return ColliderRadius + targetRadius + 2.0f;
        }
    }

    public float StopDistance { get; private set; } = 1.0f;
    BaseObject _target;

    protected override void UpdateIdle()
    {
        // 우선순위 설정하기
        // 0. 이동 상태라면 강제 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
        // 0. 너무 멀어졌다면 강제로 이동

        // 1. 몬스터 사냥
        Creature creature = FindClosestInRange(SearchDistance, Managers.objectManager.Monsters) as Creature;
        if (creature != null)
        {
            _target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 2. 주변 Env 채굴
        Env env = FindClosestInRange(SearchDistance, Managers.objectManager.Envs) as Env;
        if (creature != null)
        {
            _target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.CollectEnv;
            return;
        }

        // 3. Camp 주변으로 모이기
        if (NeedArrange)
        {
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.ReturnToCamp;
            return;
        }
    }
    protected override void UpdateMove()
    {
        // 0. 이동 상태라면 강제 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);
            return;
        }
        // 1. 주변 몬스터 서치
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 몬스터가 죽었으면 포기
            if (_target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, SearchDistance);
            return;
        }
        // 2. 주변 Env 채굴
        if (HeroMoveState == EHeroMoveState.CollectEnv)
        {
            Creature creature = FindClosestInRange(SearchDistance, Managers.objectManager.Monsters) as Creature;
            if (creature !=null)
            {
                _target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기
            if (_target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, SearchDistance);
            return;
        }

        // 3. Camp 주변으로 모이기
        if (HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            float stopDistanceSqr = StopDistance * StopDistance;
            if (dir.sqrMagnitude <= StopDistance)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Idle;
                NeedArrange = true;
                return;
            }
            else
            {
                // 멀리 있을수록 빨라짐
                // temp
                float ratio = Mathf.Min(1, dir.magnitude);
                float moveSpeed = MoveSpeed * (float)Mathf.Pow(ratio, 3);
                SetRigidBodyVelocity(dir.normalized * moveSpeed);
                return;
            }
        }

        // 4. 기타 (누르다 뗐을 때)
        CreatureState = ECreatureState.Idle;
    }
    protected override void UpdateSkill()
    {
        // 공격을 하다가 끌고 오면 바로 끌고 갈 수 있도록
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
        // 몬스터가 죽으면 바로
        if (_target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }
    protected override void UpdateDead()
    {

    }

    // 가까이에 있는 무언가를 찾아주는 Helper 코드
    // range 범위안에 가장 가까이 있는 objs 찾기
    BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs)
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

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    void ChaseOrAttackTarget(float attackRange, float chaseRange)
    {
        Vector3 dir = (_target.transform.position - transform.position);
        float distToTargetSqr = dir.sqrMagnitude;
        float attackDistanceSqr = attackRange * attackRange;

        // 공격 범위 이내로 들어왔다면 공격
        if (distToTargetSqr <= attackDistanceSqr)
        {
            CreatureState = ECreatureState.Skill;
            return;
        }
        else
        {
            // 공격 범위 밖이라면 추적
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);

            // 너무 멀어지면 포기
            float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                _target = null;
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
            }
        }
    }
    #endregion

    private void TryResizeCollider()
    {
        // 일단 충돌체를 아주 작게
        ChangeColliderSize(EColliderSize.Small);

        foreach (var hero in Managers.objectManager.Heroes)
        {
            if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)
            {
                return;
            }
        }

        // ReturnToCamp가 한 명도 없으면 콜라이더 조정
        foreach (var hero in Managers.objectManager.Heroes)
        {
            // 단 채집이나 전투중이면 스킵.
            if (hero.CreatureState == ECreatureState.Idle)
            {
                hero.ChangeColliderSize(EColliderSize.Big);
            }
        }
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch(joystickState)
        {
            case EJoystickState.PointerDown:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;
            case EJoystickState.Drag:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;
            case EJoystickState.PointerUp:
                HeroMoveState = EHeroMoveState.None;
                break;
            default:
                break;
        }
    }

    // 애니메이션에 대한 이벤트 핸들러
    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);

        // TODO
        CreatureState = ECreatureState.Move;

        // Skill
        if (_target.IsValid() == false)
        {
            return;
        }

        _target.OnDamaged(this);
    }
}
