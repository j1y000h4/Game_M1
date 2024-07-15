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
}
