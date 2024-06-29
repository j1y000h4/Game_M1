using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    Vector2 _moveDir = Vector2.zero;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        CreatureType = ECreatureType.Hero;
        CreatureState = ECreatureState.Idle;
        Speed = 5.0f;

        Managers.gameManager.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.gameManager.OnMoveDirChanged += HandleOnMoveDirChanged;
        Managers.gameManager.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.gameManager.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        return true;
    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * Speed);
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;
        if (_moveDir.x < 0)
        {
            isLookLeft = true;
        }
        else if (_moveDir.x > 0)
        {
            isLookLeft = false;
        }
    }
    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch(joystickState)
        {
            case EJoystickState.PointerDown:
                CreatureState = ECreatureState.Move;
                break;
            case EJoystickState.Drag:
                break;
            case EJoystickState.PointerUp:
                CreatureState = ECreatureState.Idle;
                break;
        }
    }
}
