using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CursorData cursorData;

    void OnEnable ()
    {
        cursorData = CursorData.Instance;
    }

    public void OnPointerEnter (PointerEventData pointerEventData)
    {
        // Cursor.SetCursor(cursorData.Button.Cursor, cursorData.Button.HotSpot, CursorMode.Auto);
    }

    public void OnPointerExit (PointerEventData pointerEventData)
    {
        OnDisable();
    }

    void OnDisable ()
    {
        // Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
    }
}