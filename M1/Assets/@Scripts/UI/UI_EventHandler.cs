using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UI에다가 어떠한 이벤트를 붙이고 싶을때
public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public event Action<PointerEventData> OnClickHandler = null;
    public event Action<PointerEventData> OnPointerDownHandler = null;
    public event Action<PointerEventData> OnPointerUpHandler = null;
    public event Action<PointerEventData> OnDragHandler = null;

    // Drag
    public void OnDrag(PointerEventData eventData)
    {
        OnDragHandler?.Invoke(eventData);
    }

    // Click
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke(eventData);
    }

    // PointerDown
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownHandler?.Invoke(eventData);
    }

    // PointerUp
    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpHandler?.Invoke(eventData);
    }
}
