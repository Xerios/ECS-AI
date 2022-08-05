using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Action<GameObject> OnDropped, OnChangeOrder, OnDragOut;


    public void OnPointerEnter (PointerEventData eventData)
    {
        // Debug.Log("OnPointerEnter");
        if (eventData.pointerDrag == null)
            return;

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null) {
            d.PlaceholderParent = this.transform;
        }
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        // Debug.Log("OnPointerExit");
        if (eventData.pointerDrag == null)
            return;

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
        if (d != null && d.PlaceholderParent == this.transform) {
            d.PlaceholderParent = d.ParentToReturnTo;
        }
    }

    public void OnDrop (PointerEventData eventData)
    {
        // Debug.Log(eventData.pointerDrag.name + " was dropped on " + gameObject.name);

        Draggable d = eventData.pointerDrag.GetComponent<Draggable>();

        if (d != null) {
            d.ParentToReturnTo = this.transform;
        }
    }
}