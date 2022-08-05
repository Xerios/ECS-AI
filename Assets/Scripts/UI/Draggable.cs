using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup), typeof(LayoutElement))]
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [NonSerialized]
    public Transform ParentToReturnTo = null;
    [NonSerialized]
    public Transform PlaceholderParent = null;

    public bool interactable = true;

    GameObject placeholder = null;
    DropZone previousZone = null;

    public void OnBeginDrag (PointerEventData eventData)
    {
        if (!interactable) return;
        Cursor.visible = false;

        placeholder = new GameObject();
        placeholder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        ParentToReturnTo = this.transform.parent;
        PlaceholderParent = ParentToReturnTo;
        previousZone = ParentToReturnTo.GetComponent<DropZone>();
        this.transform.SetParent(this.transform.root);

        GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Debug.Log("OnBeginDrag:" + previousZone);
    }

    public void OnDrag (PointerEventData eventData)
    {
        if (!interactable) return;
        // Debug.Log ("OnDrag");

        this.transform.position = eventData.position;

        if (placeholder.transform.parent != PlaceholderParent)
            placeholder.transform.SetParent(PlaceholderParent);

        int newSiblingIndex = PlaceholderParent.childCount;

        for (int i = 0; i < PlaceholderParent.childCount; i++) {
            if (this.transform.position.x < PlaceholderParent.GetChild(i).position.x) {
                newSiblingIndex = i;

                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;

                break;
            }
        }

        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag (PointerEventData eventData)
    {
        Cursor.visible = true;
        if (!interactable) return;
        // Debug.Log("OnEndDrag:" + previousZone);
        this.transform.SetParent(ParentToReturnTo);
        this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        Destroy(placeholder);
        var newZone = ParentToReturnTo.GetComponent<DropZone>();
        if (previousZone != newZone) {
            previousZone.OnDragOut?.Invoke(this.gameObject);
            newZone.OnDropped?.Invoke(eventData.pointerDrag);
        }else{
            newZone.OnChangeOrder?.Invoke(eventData.pointerDrag);
        }
    }
}