using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 모든 UI들의 조상
public class UI_Base : InitBase
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    private void Awake()
    {
        Init();
    }

    // UI를 하나씩 에디터에서 드래그 앤 드랍 하기에는 나중에 관리해야할 것들이 너무 많아진다.
    // 그러므로 소스 코드 상에서 아이디를 찾아주는 방식을 주로 사용하게 된다.
    // Bind
    // 이름으로 찾아서 그 녀석을 연결해주는 기능
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);
            }

            if (objects[i] == null)
            {
                Debug.Log($"Failed to bind({name[i]})");
            }
        }
    }

    protected void BindObejcts(Type type) { Bind<GameObject>(type); }
    protected void BindTexts(Type type) { Bind<TMP_Text>(type); }
    protected void BindButtons(Type type) { Bind<Button>(type); }
    protected void BindImages(Type type) { Bind<Image>(type); }
    protected void BindToggles(Type type) { Bind<Toggle>(type); }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }

    // BindEvent
    // EUIEvent라는 type의 이벤트가 발생하면 action이라는 함수를 호출하고 싶다. 라는 기능
    public static void BindEvent(GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch(type)
        {
            case Define.EUIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.EUIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.EUIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case Define.EUIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
        }
    }
}
