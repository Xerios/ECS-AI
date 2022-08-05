using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public void OnPointerEnter (PointerEventData eventData)
    {
        StartHover(new Vector3(eventData.position.x, eventData.position.y - 18f, 0f));
    }
    public void OnSelect (BaseEventData eventData)
    {
        StartHover(transform.position);
    }
    public void OnPointerExit (PointerEventData eventData)
    {
        StopHover();
    }
    public void OnDeselect (BaseEventData eventData)
    {
        StopHover();
    }

    void StartHover (Vector3 position)
    {
        Tooltip.Instance.Show(this.gameObject);
    }
    void StopHover ()
    {
        Tooltip.Instance.Hide();
    }
}