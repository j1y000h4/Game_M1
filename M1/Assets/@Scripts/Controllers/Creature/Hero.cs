using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    public bool NeedArrange { get; set; }

    // 좀 더 잘 뭉치기 위한 
    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if(_creatureState != value)
            {
                base.CreatureState = value;

                // Grid 방식으로 가는 이상 ColliderSize를 이용한 이동 로직은 필요없어지게 된다!!
                //switch (value)
                //{
                //    case ECreatureState.Move:
                //        RigidBody.mass = CreatureData.Mass * 5.0f;
                //        break;
                //    case ECreatureState.Skill:
                //        RigidBody.mass = CreatureData.Mass * 500.0f;
                //        break;
                //    default:
                //        RigidBody.mass = CreatureData.Mass;
                //        break;
                //}
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
            switch(value)
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

        // Map
        Collider.isTrigger = true;
        RigidBody.simulated = false;

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

    protected override void UpdateIdle()
    {
        // Update할때마다 속도를 0으로
        // Grid 방식으로 가는 이상 ColliderSize를 이용한 이동 로직은 필요없어지게 된다!!
        //SetRigidBodyVelocity(Vector2.zero);

        // 우선순위 설정하기
        // 0. 이동 상태라면 강제 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
        // 0. 너무 멀어졌다면 강제로 이동

        // 1. 몬스터 사냥
        Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.objectManager.Monsters) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 2. 주변 Env 채굴
        Env env = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.objectManager.Envs) as Env;
        if (env != null)
        {
            Target = env;
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
        // 너무 멀면 강제 이동
        if (HeroMoveState == EHeroMoveState.ForcePath)
        {
            MoveByForcePath();
            return;
        }

        if (CheckHeroCampDistanceAndForcePath())
        {
            return;
        }

        // 0. 이동 상태라면 강제 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            EFindPathResult result = FindPathAndMoveToCellPos(HeroCampDest.position, HERO_DEFAULT_MOVE_DEPTH);

            return;
        }
        // 1. 주변 몬스터 서치
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 몬스터가 죽었으면 포기
            if (Target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, AttackDistance);
            return;
        }
        // 2. 주변 Env 채굴
        if (HeroMoveState == EHeroMoveState.CollectEnv)
        {
            Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.objectManager.Monsters) as Creature;
            if (creature !=null)
            {
                Target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기
            if (Target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, AttackDistance);
            return;
        }

        // 3. Camp 주변으로 모이기
        if (HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
            Vector3 destPos = HeroCampDest.position;
            if (FindPathAndMoveToCellPos(destPos, HERO_DEFAULT_MOVE_DEPTH) == EFindPathResult.Success)
            {
                return;
            }

            // 실패 사유 검사
            BaseObject obj = Managers.mapManager.GetObject(destPos);
            if (obj.IsValid() == false)
            {
                // 내가 그 자리를 차지하고 있다면
                if (obj == this)
                {
                    HeroMoveState = EHeroMoveState.None;
                    NeedArrange = false;
                    return;
                }
                
                // 다른 영웅이 멈춰 있으면
                Hero hero = obj as Hero;
                if (hero != null && hero.CreatureState == ECreatureState.Idle)
                {
                    HeroMoveState = EHeroMoveState.None;
                    NeedArrange = false;
                    return;
                }
            }
        }

        // 4. 기타 (누르다 뗐을 때)
        // 스르륵 움직이고 있는것도 움직이고 있는 것이다.
        if (LerpCellPosCompleted)
        {
            CreatureState = ECreatureState.Idle;
        }
    }

    Queue<Vector3Int> _forcePath = new Queue<Vector3Int>();

    // HeroCamp와 거리를 체크하고 너무 멀다 싶으면 길을 강제로 가라.
    bool CheckHeroCampDistanceAndForcePath()
    {
        // 너무 멀어서 못 간다.
        Vector3 destPos = HeroCampDest.position;
        Vector3Int destCellPos = Managers.mapManager.World2Cell(destPos);
        if ((CellPos - destCellPos).magnitude <= 10)
            return false;

        // 최소한 목적지가 갈 수 있는 곳인지 확인
        if (Managers.mapManager.CanGo(destCellPos, ignoreObjects: true) == false)
            return false;

        List<Vector3Int> path = Managers.mapManager.FindPath(CellPos, destCellPos, 100);

        // 못 찾으면 실패
        if (path.Count < 2)
            return false;

        HeroMoveState = EHeroMoveState.ForcePath;

        _forcePath.Clear();
        foreach (var p in path)
        {
            _forcePath.Enqueue(p);
        }
        _forcePath.Dequeue();

        return true;
    }

    // 강제로 이동 시키기
    void MoveByForcePath()
    {
        if (_forcePath.Count == 0)
        {
            HeroMoveState = EHeroMoveState.None;
            return;
        }

        Vector3Int cellPos = _forcePath.Peek();

        if (MoveToCellPos(cellPos, 2))
        {
            _forcePath.Dequeue();
            return;
        }

        // 실패 사유가 영웅이라면.
        Hero hero = Managers.mapManager.GetObject(cellPos) as Hero;
        if (hero != null && hero.CreatureState == ECreatureState.Idle)
        {
            HeroMoveState = EHeroMoveState.None;
            return;
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        // 공격을 하다가 끌고 오면 바로 끌고 갈 수 있도록
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
        // 몬스터가 죽으면 바로
        if (Target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }
    protected override void UpdateDead()
    {
        
    }

    #endregion

    // Grid 방식으로 가는 이상 ColliderSize를 이용한 이동 로직은 필요없어지게 된다!!
    //private void TryResizeCollider()
    //{
    //    // 일단 충돌체를 아주 작게
    //    ChangeColliderSize(EColliderSize.Small);

    //    foreach (var hero in Managers.objectManager.Heroes)
    //    {
    //        if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)
    //        {
    //            return;
    //        }
    //    }

    //    // ReturnToCamp가 한 명도 없으면 콜라이더 조정
    //    foreach (var hero in Managers.objectManager.Heroes)
    //    {
    //        // 단 채집이나 전투중이면 스킵.
    //        if (hero.CreatureState == ECreatureState.Idle)
    //        {
    //            hero.ChangeColliderSize(EColliderSize.Big);
    //        }
    //    }
    //}

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
    }
}
