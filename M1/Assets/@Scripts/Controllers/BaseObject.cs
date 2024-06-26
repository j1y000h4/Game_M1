using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BaseObject : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; protected set; }
    public SkeletonAnimation SkeletonAnim { get; protected set; }
    public Rigidbody2D Rigidbody { get; protected set; }

    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
    //public float ColliderRadius { get { return Collider?.radius ?? 0.0f; } }

    bool _isLookLeft = false;
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
        Rigidbody = GetComponent<Rigidbody2D>();

        return true;
    }

    #region Spine
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

        SkeletonAnim.skeleton.ScaleX = flag ? -1 : 1;
    }
    #endregion
}
