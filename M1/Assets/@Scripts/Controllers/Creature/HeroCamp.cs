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

        // Obstacle과는 충돌 but, Monster와 Hero는 충돌하지 않도록 (include = 포함 / exclude 불포함)
        Collider.includeLayers = (1 << (int)ELayer.Obstacle);
        Collider.excludeLayers = (1 << (int)ELayer.Monster) | (1 << (int)ELayer.Hero);

        ObjectType = EObjectType.HeroCamp;

        Pivot = Util.FindChild<Transform>(gameObject, "Pivot", true);
        Destination = Util.FindChild<Transform>(gameObject, "Destination", true);

        return true;
    }
    private void Update()
    {
        //transform.Translate(_moveDir * Time.deltaTime * Speed);

        Vector3 dir = _moveDir * Time.deltaTime * Speed;
        Vector3 newPos = transform.position + dir;

        if (Managers.mapManager == null)
        {
            return;
        }

        // Camp는 그 위치로 갈 수 있느냐 없느냐 정도만 체크
        if (Managers.mapManager.CanGo(newPos, ignoreObjects: true, ignoreSemiWall: true) == false)
        {
            return;
        }

        transform.position = newPos;
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;

        // Pviot을 돌려주는 공식
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;
            Pivot.eulerAngles = new Vector3(0, 0, angle);
        }
    }
}
