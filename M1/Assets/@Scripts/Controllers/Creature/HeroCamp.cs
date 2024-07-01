using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class HeroCamp : BaseObject
{
    Vector2 _moveDir = Vector2.zero;

    public float Speed { get; private set; } = 5.0f;

    public Transform Pivot { get; private set; }
    public Transform Destination {  get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        Managers.gameManager.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.gameManager.OnMoveDirChanged += HandleOnMoveDirChanged;

        // Obstacle과는 충돌 but, Monster와 Hero는 충돌하지 않도록
        Collider.includeLayers = (1 << (int)ELayer.Obstacle);
        Collider.excludeLayers = (1 << (int)ELayer.Monster) | (1 << (int)ELayer.Hero);

        ObjectType = EObjectType.HeroCamp;

        Pivot = Util.FindChild<Transform>(gameObject, "Pivot", true);
        Destination = Util.FindChild<Transform>(gameObject, "Destination", true);

        return true;
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;


    }
}
