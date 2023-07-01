using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action<PointerEventData> onPointerEnter = null;
    public void OnPointerEnter(PointerEventData eventData){
        _pointerStaying = true;
        onPointerEnter?.Invoke(eventData);
    }

    public Action<PointerEventData> onPointerExit = null;
    public void OnPointerExit(PointerEventData eventData){
        _pointerStaying = false;
        onPointerExit?.Invoke(eventData);
    }

    private bool _pointerStaying = false;

    public Action onPointerStay = null;
    private void Update(){
        if(_pointerStaying){
            onPointerStay?.Invoke();
        }
    }
}
