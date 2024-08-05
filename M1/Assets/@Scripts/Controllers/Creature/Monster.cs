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

        // Skill
        // 크리처 쪽으로 들어갈 예정
        //Skills = gameObject.GetOrAddComponent<SkillComponent>();
        //Skills.SetInfo(this, CreatureData.SkillIdList);
    }

    private void Start()
    {
        _initPos = transform.position;
    }

    #region AI
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
        if (Target.IsValid() == false)
        {
            Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.objectManager.Heroes, func: IsValid) as Creature;
            if (creature != null)
            {
                Target = creature;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Move
            FindPathAndMoveToCellPos(_destPos, MONSTER_DEFAULT_MOVE_DEPTH);
            if (LerpCellPosCompleted)
            {
                CreatureState = ECreatureState.Idle;
                return;
            }
        }
        else
        {
            // Chase
            ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, AttackDistance);

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
        base.UpdateSkill();

        if (Target.IsValid() == false)
        {
            Target = null;
            _destPos = _initPos;
            CreatureState = ECreatureState.Move;
            return;
        }
    }
    protected override void UpdateDead()
    {
        //Debug.Log(" > " + GetType().Name + " / UpdateDead");
        //SetRigidBodyVelocity(Vector2.zero);
    }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);
    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);

        // TODO : Drop Item

        Managers.objectManager.Despawn(this);
    }
    #endregion
}
