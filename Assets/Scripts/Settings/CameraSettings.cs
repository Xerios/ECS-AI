
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings")]
public class CameraSettings : ScriptableObject
{
    //----------------------------------------------------------------------
    [Header("Zoom Settings")]
    public float minZoom = 0.1f;
    public float maxZoom = 100f;
    public AnimationCurve zoomCurve;

    //----------------------------------------------------------------------
    [Header("Speeds")]
    [Range(0.1f, 1f)]
    [Tooltip("Speed at which camera interpolates between new positions, lower = smoother camera movement, higher = more rigid")]
    public float tweenTime = 0.5f;

    public AnimationCurve tweenCurve;

    [Range(0.1f, 1f)]
    public float manualZoomSteps = 0.1f;

    [Range(1f, 10f)]
    [Tooltip("Speed at which camera moves when using directional movement ( Up/Down/Left/Right )")]
    public float directionalMovementSpeed = 3f;

    [Range(0.1f, 1f)]
    [Tooltip("Horizontal Rotation Speed")]
    public float rotateHorizontalSpeed = 0.2f;

    [Range(0.1f, 1f)]
    [Tooltip("Vertical Rotation Speed")]
    public float rotateVerticalSpeed = 0.2f;

    [Header("Bounds")]
    public float RadiusBound = 10f;
}