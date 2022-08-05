using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CursorData")]
public class CursorData : SingletonScritableObject<CursorData>
{
    public CursorPointer Button, NotAllowed, Broadcast, BroadcastReplace, SetupPOI;
}

[Serializable]
public struct CursorPointer
{
    public Vector2 HotSpot;
    public Texture2D Cursor;
}