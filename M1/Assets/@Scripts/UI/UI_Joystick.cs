using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Joystick : UI_Base
{
    enum GameObjects
    {
        JoystickBG,
        JoystickCursor,
    }

    private GameObject _background;
    private GameObject _cursor;
    private float _radius;
    private Vector2 _touchPos;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        BindObejcts(typeof(GameObjects));

        _background = GetObject((int)GameObjects.JoystickBG);
        _cursor = GetObject((int)GameObjects.JoystickCursor);
        _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 2.5f;

        gameObject.BindEvent(OnPointerDown, type: EUIEvent.PointerDown);
        gameObject.BindEvent(OnPointerUp, type: EUIEvent.PointerUp);
        gameObject.BindEvent(OnDrag, type: EUIEvent.Drag);

        return true;
    }

    #region Event
    // Mouse 누르는 순간
    public void OnPointerDown(PointerEventData eventData)
    {
        // Background, cursor, touchPos는 마우스를 누르는 순간의 position이 위치한다.
        _background.transform.position = eventData.position;
        _cursor.transform.position = eventData.position;
        _touchPos = eventData.position;

        Managers.gameManager.joystickState = EJoystickState.PointerDown;
    }

    // Mouse를 누르고 있다가 땠을때
    public void OnPointerUp(PointerEventData eventData)
    {
        // 땠을때 cursor만 처음 누른 그 position으로 이동하도록
        _cursor.transform.position = _touchPos;

        Managers.gameManager.moveDir = Vector2.zero;
        Managers.gameManager.joystickState = EJoystickState.PointerUp;
    }

    // Mouse Drag
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchDir = (eventData.position - _touchPos);

        float moveDist = Mathf.Min(touchDir.magnitude, _radius);
        Vector2 moveDir = touchDir.normalized;
        Vector2 newPosition = _touchPos + moveDir * moveDist;
        _cursor.transform.position = newPosition;

        Managers.gameManager.moveDir = moveDir;
        Managers.gameManager.joystickState= EJoystickState.Drag;
    }
    #endregion
}
