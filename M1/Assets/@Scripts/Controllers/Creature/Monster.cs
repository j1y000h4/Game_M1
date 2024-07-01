using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public override ECreatureState CreatureState 
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
                switch (value)
                {
                    case ECreatureState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;
                }
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        CreatureType = ECreatureType.Monster;
        MoveSpeed = 3.0f;

        // AI
        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        // State
        // Init에 넣기에는 너무 이르다.
        CreatureState = ECreatureState.Idle;
    }

    private void Start()
    {
        _initPos = transform.position;
    }

    #region AI
    // 일단은 하드코딩. 추후에는 Data로 빼주는게 맞다.
    public float SearchDistance { get; private set; } = 8.0f;
    public float AttackDistance { get; private set; } = 4.0f;
    Creature _target;
    Vector3 _destPos;
    Vector3 _initPos;

    protected override void UpdateIdle()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateIdle");

        // Patrol
        {
            // 확률을 부여해서 Idle 상태에서 0.5초마다 체크하는데 10%확률로 랜덤하게 패트롤한다.
            int patrolPercent = 10;
            int rand = Random.Range(0, 100);
            if (rand <= patrolPercent)
            {
                _destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
                CreatureState = ECreatureState.Move;
                return;
            }
        }

        // Serach Player
        {
            Creature target = null;

            // 가장 가까이 있는 target을 구하기 위해 초기값을 최대값으로 설정
            float bestDistanceSqr = float.MaxValue;
            float searchDistanceSqr = SearchDistance * SearchDistance;


            foreach (Hero hero in Managers.objectManager.Heroes)
            {
                Vector3 dir = hero.transform.position - transform.position;
                float distToTargetSqr = dir.sqrMagnitude;

                //Debug.Log(" > " + GetType().Name + " / UpdateIdle / DistToTargetSqr : " + distToTargetSqr);

                // 서치 범위보다 타겟이 더 먼 곳에 있으면 스킵
                if (distToTargetSqr > searchDistanceSqr)
                {
                    continue;
                }

                // 현재 타겟보다 더 먼게 잡히면 스킵
                if (distToTargetSqr > bestDistanceSqr)
                {
                    continue; 
                }

                target = hero;
                bestDistanceSqr = distToTargetSqr;
            }

            _target = target;

            // 같이 일어나야 해서 묶여야 될 부분을 따로따로 코드로 작업하면 유지보수에서 실수할 가능성이 높다.
            if (_target != null)
            {
                CreatureState = ECreatureState.Move;
            }
        }
    }
    protected override void UpdateMove()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateMove");

        // 타겟이 사라지면or없어지면
        if (_target == null)
        {
            // Patrol or Return
            Vector3 dir = (_destPos - transform.position);

            // initPos에 거의 가까워지면
            if (dir.sqrMagnitude <= 0.01f)
            {
                CreatureState = ECreatureState.Idle;
            }

            SetRigidBodyVelocity(dir.normalized * MoveSpeed);
        }
        else
        {
            // Chase
            Vector3 dir = (_target.transform.position - transform.position);
            float distToTargetSqr = dir.sqrMagnitude;
            float attackDistanceSqr = AttackDistance * AttackDistance;

            // 공격 범위 이내로 들어왔으면 공격한다.
            if (distToTargetSqr < attackDistanceSqr)
            {
                CreatureState = ECreatureState.Skill;
                StartWait(2.0f);
            }
            // 공격 범위 밖이라면 추적
            else
            {
                SetRigidBodyVelocity(dir.normalized * MoveSpeed);

                // 서치 범위 밖이라면 포기하고 제자지로
                float serachDistanceSqr = SearchDistance * SearchDistance;
                if (distToTargetSqr > serachDistanceSqr)
                {
                    _destPos = _initPos;
                    _target = null;
                    CreatureState = ECreatureState.Move;
                }
            }
        }
    }
    protected override void UpdateSkill()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateSkill");

        // _coWait가 null이 아니면 기다리고 있다는 뜻
        if (_coWait != null)
        {
            return;
        }

        CreatureState = ECreatureState.Move;
    }
    protected override void UpdateDead()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateDead");
    }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker)
    {
        base.OnDamaged(attacker);
    }

    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);

        // TODO : Drop Item

        Managers.objectManager.Despawn(this);
    }
    #endregion
}
