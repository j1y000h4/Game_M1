using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class BaseObject : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    public SkeletonAnimation SkeletonAnim { get; private set; }
    public Rigidbody2D RigidBody { get; private set; }

    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

    public int DataTemplateID { get; set; }

    bool _isLookLeft = true;
    public bool isLookLeft
    {
        get { return _isLookLeft; }
        set
        {
            _isLookLeft = value;
            Flip(!value);
        }
    }

    // 초기화
    public override bool Init()
    {
        if(base.Init() == false)
        {
            return false;
        }

        // GetOrAddComponent 없으면 추가
        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        SkeletonAnim = GetComponent<SkeletonAnimation>();
        RigidBody = GetComponent<Rigidbody2D>();

        return true;
    }

    public void LookAtTarget(BaseObject target)
    {
        Vector2 dir = target.transform.position - transform.position;
        if (dir.x < 0)
        {
            isLookLeft = true;
        }
        else
        {
            isLookLeft = false;
        }
    }

    #region Battle
    public virtual void OnDamaged(BaseObject attacker, SkillBase skill)
    {

    }    
    public virtual void OnDead(BaseObject attacker, SkillBase skill)
    {

    }
    #endregion

    #region Spine
    protected virtual void SetSpineAnimation(string dataLabel, int sortingOrder)
    {
        if (SkeletonAnim == null)
            return;

        SkeletonAnim.skeletonDataAsset = Managers.resourceManager.Load<SkeletonDataAsset>(dataLabel);
        SkeletonAnim.Initialize(true);

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer,MeshRenderer을 같이 계산함.
        SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
        sg.sortingOrder = sortingOrder;
    }

    protected virtual void UpdateAnimation()
    {

    }

    // Grid 방식으로 가는 이상 ColliderSize를 이용한 이동 로직은 필요없어지게 된다!!
    // RigidBody를 사용하는거라면 유니티 물리를 사용해 이동해주는게 좋다.
    //public void SetRigidBodyVelocity(Vector2 velocity)
    //{
    //    if (RigidBody == null)
    //    {
    //        return;
    //    }

    //    RigidBody.velocity = velocity;

    //    if (velocity.x < 0)
    //    {
    //        isLookLeft = true;
    //    }
    //    else if (velocity.x > 0)
    //    {
    //        isLookLeft = false;
    //    }
    //}
    public void PlayAnimation(int trackIndex, string AnimName, bool loop)
    {
        if (SkeletonAnim == null)
        {
            return;
        }

        SkeletonAnim.AnimationState.SetAnimation(trackIndex, AnimName, loop);
    }

    public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
    {
        if (SkeletonAnim == null)
        {
            SkeletonAnim.AnimationState.AddAnimation(trackIndex,AnimName, loop, delay);
        }
    }

    // Spine이 왼쪽이냐 오른쪽이냐
    public void Flip(bool flag)
    {
        if(SkeletonAnim == null)
        {
            return;
        }

        SkeletonAnim.Skeleton.ScaleX = flag ? -1 : 1;
    }
    // 애니메이션에 대한 이벤트를 전달/받는 함수
    public virtual void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        //Debug.Log("OnAnimEventHandler");
    }
    #endregion

    #region Map
    public bool LerpCellPosCompleted { get; protected set; }

    // 모든 오브젝트마다 벡터3 인트 타입으로 좌표를 관리하는 것
    Vector3Int _cellPos;
    public Vector3Int CellPos
    {
        get { return _cellPos; }
        protected set
        {
            _cellPos = value;
            LerpCellPosCompleted = false;
        }
    }

    public void SetCellPos(Vector3Int cellPos, bool forceMove = false)
    {
        // CellPos과 position의 차이를 헷갈리지않게 잘 이해하기
        CellPos = cellPos;
        LerpCellPosCompleted = false;

        if (forceMove)
        {
            transform.position = Managers.mapManager.Cell2World(CellPos);
            LerpCellPosCompleted = true;
        }
    }

    // 정확히 Cell 위치에 있지 않는 애를 자연스럽게 이동할 수 있도록
    public void LerpToCellPos(float moveSpeed)
    {
        if (LerpCellPosCompleted)
            return;

        Vector3 destPos = Managers.mapManager.Cell2World(CellPos);
        Vector3 dir = destPos - transform.position;

        if (dir.x < 0)
            isLookLeft = true;
        else
            isLookLeft = false;

        if (dir.magnitude < 0.01f)
        {
            transform.position = destPos;
            LerpCellPosCompleted = true;
            return;
        }

        // 실질적인 이동
        float moveDist = Mathf.Min(dir.magnitude, moveSpeed * Time.deltaTime);
        transform.position += dir.normalized * moveDist;
    }
    #endregion

}
