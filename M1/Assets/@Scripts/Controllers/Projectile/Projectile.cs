using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Projectile : BaseObject
{
    // 나를 쏜 주체
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }
    public ProjectileMotionBase ProjectileMotion { get; private set; }

    private SpriteRenderer _spriteRenderer;

    // 생성자처럼 사용
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        ObjectType = EObjectType.Projectile;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;

        return true;
    }

    // 인게임에서 설치하는 기능
    public void SetInfo(int dataTemplateID)
    {
        ProjectileData = Managers.dataManager.ProjectileDic[dataTemplateID];
        _spriteRenderer.sprite = Managers.resourceManager.Load<Sprite>(ProjectileData.ProjectileSpriteName);

        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"Projectile Sprite Missing {ProjectileData.ProjectileSpriteName}");
            return;
        }
    }

    public void SetSpawnInfo(Creature owner, SkillBase skill, LayerMask layer)
    {
        Owner = owner;
        Skill = skill;

        // Rule
        Collider.excludeLayers = layer;

        // TODO

        if (ProjectileMotion != null)
        {
            Destroy(ProjectileMotion);
        }

        string componentName = skill.SkillData.ComponentName;
        ProjectileMotion = gameObject.AddComponent(Type.GetType(componentName)) as ProjectileMotionBase;

        StraightMotion straightMotion = ProjectileMotion as StraightMotion;
        if (straightMotion != null)
        {
            // 다끝나면 ()=> 자폭!(Despawn)
            straightMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.objectManager.Despawn(this); });
        }

        ParabolaMotion parabolaMotion = ProjectileMotion as ParabolaMotion;
        if (parabolaMotion != null)
        {
            parabolaMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.objectManager.Despawn(this); });
        }


        StartCoroutine(CoReserveDestory(5.0f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseObject target = other.GetComponent<BaseObject>();
        if (target.IsValid() == false)
        {
            return;
        }

        // TODO
        // 하나하나 부품 조립하듯이! 컴포넌트로 쪼개서 관리 조립하자
        target.OnDamaged(Owner, Skill);
        Managers.objectManager.Despawn(this);
    }

    private IEnumerator CoReserveDestory(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.objectManager.Despawn(Owner);
    }
}
