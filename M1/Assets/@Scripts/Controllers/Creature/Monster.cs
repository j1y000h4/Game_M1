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
    public float AttackDistance { get; private set; } = 4.0f;

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
            Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.objectManager.Heroes, func: IsValid) as Creature;
            if (creature != null)
            {
                Target = creature;
                CreatureState = ECreatureState.Move;
                return;
            }
        }
        
    }
    protected override void UpdateMove()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateMove");

        // 타겟이 사라지면or없어지면
        if (Target == null)
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
            ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, 5.0f);

            // 너무 멀어지면 포기
            if (Target.IsValid() == false)
            {
                Target = null;
                _destPos = _initPos;
                return;
            }
        }
    }
    protected override void UpdateSkill()
    {
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
