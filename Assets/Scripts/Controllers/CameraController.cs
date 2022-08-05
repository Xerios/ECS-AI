using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CameraController : MonoSingleton<CameraController>
{
    public ReactiveProperty<bool> Use2DMode = new ReactiveProperty<bool>(false);

    public bool lockPositionToZero;

    [Header("Zoom Level")]
    [Range(0f, 1f)]
    public float zoom = 1f;

    [Header("Mouse Reference")]
    public MouseController MouseCtrl;

    [Header("Camera Settings")]
    public CameraSettings settings;

    // ----------------------------------------------------------------------
    private new Transform camera;
    private Cinemachine.CinemachineVirtualCamera virtualCamera;

    private float zoomSmoothed = 1f;

    private Vector3 dragMousePos;
    private Vector3 dragOriginPosition;
    private Vector3 dragStartPosition;
    private Vector2 rotateStartPosition;

    private Vector2 rotatePitchClamp = new Vector2(20f, 85f);

    private Vector3 destinationPosition;

    float zoomTimer = 0f, positionTimer = 0f;
    float startZoom = 0f;
    Vector3 startPosition;

    // float pitchValue = 0;

    // ----------------------------------------------------------------------

    private CompositeDisposable disposables;

    // Use this for initialization
    private void OnEnable ()
    {
        var raycastMgr = RaycastManager.Instance;
        var inputMgr = InputManager.Instance;

        camera = transform.GetChild(0);
        virtualCamera = camera.GetComponent<Cinemachine.CinemachineVirtualCamera>();

        destinationPosition = transform.localPosition;

        disposables = new CompositeDisposable();


        // Directional movement relative to the camera ( Up/Down/Left/Right )
        inputMgr.Movement
        .Subscribe(pos =>
            {
                if (!lockPositionToZero) {
                    // Transform direction to camera's rotation
                    var move = camera.transform.rotation * new Vector3(pos.x, 0, pos.y);
                    move.y = 0;
                    move = move.normalized;

                    // Accelerate based on zoom out
                    move *= (1 + zoom);
                    // Multiply by the movement speed
                    move *= settings.directionalMovementSpeed;

                    destinationPosition = ConstrainToBounds(this.transform.localPosition + move);
                }
            })
        .AddTo(disposables);

        // Mouse drag script ( save initial drag position for later use )
        inputMgr.DragCameraDown
        .Subscribe(pos =>
            {
                if (!lockPositionToZero) {
                    dragMousePos = pos;
                    dragOriginPosition = this.transform.localPosition;

                    // Mouse specific code
                    raycastMgr.RaycastGround(dragMousePos, out dragStartPosition);
                    MouseCtrl.Show(dragStartPosition);

                    // Debug stuff
                    // Debug.DrawRay(dragStartPosition, Vector3.up * 5, Color.red, 1f);
                }
            })
        .AddTo(disposables);

        inputMgr.DragCameraUp
        .Subscribe(pos => MouseCtrl.Hide())
        .AddTo(disposables);

        // Mouse drag script
        inputMgr.DragCamera.Subscribe(pos =>
            {
                if (!lockPositionToZero) {
                    Vector3 startDragPosition, currentDragPosition;

                    // --------------------
                    // Get difference from first drag position and new and then set the new camera destination pos
                    if (raycastMgr.RaycastPlane(dragMousePos, dragStartPosition.y, out startDragPosition) && raycastMgr.RaycastPlane(pos, dragStartPosition.y, out currentDragPosition)) {
                        positionTimer = 0f;
                        startPosition = this.transform.localPosition;
                        destinationPosition = ConstrainToBounds(dragOriginPosition + (startDragPosition - currentDragPosition));
                    }
                }
            })
        .AddTo(disposables);

        // Mouse zoom in to origin
        inputMgr.Zoom
        .Subscribe(delta =>
            {
                zoomTimer = 0f;
                startZoom = zoomSmoothed;

                var deltaMod = (settings.zoomCurve.Evaluate(zoom) * delta);         // Make sure we zoom less the more we zoom in
                zoom = Mathf.Clamp01(zoom - deltaMod);

                // Get current mouse raycast position
                Vector3 zoomPos;
                raycastMgr.RaycastGround(inputMgr.GetMousePos(), out zoomPos);

                // Update zoom
                if (!lockPositionToZero) {
                    // Save original position
                    Vector3 originalLocalPosition = camera.localPosition;
                    Vector3 originalPosition = camera.position;

                    UpdateCameraTransform(zoom);
                    Camera.main.transform.position = camera.position;

                    // Debug.Log(Camera.main);

                    // Calculate new mouse position
                    Vector3 zoomMouseNewPosition;
                    if (raycastMgr.RaycastPlane(inputMgr.GetMousePos(), zoomPos.y, out zoomMouseNewPosition)) {
                        // Adjust camera position so that we zoom-in to point instead of center
                        var deltaPos = (zoomMouseNewPosition - zoomPos);
                        positionTimer = 0f;
                        startPosition = this.transform.localPosition;
                        destinationPosition = ConstrainToBounds(this.transform.localPosition - deltaPos);
                        // Debug.Log(deltaPos);
                    }

                    // Set back to original position ( zoom-in and translation is taken care in update with smoothing)
                    camera.localPosition = originalLocalPosition;
                    Camera.main.transform.position = originalPosition;
                }
                // Debug.DrawRay(zoomPos, Vector3.up * 5, Color.green, 1f);
            })
        .AddTo(disposables);


        // Mouse drag to rotate script ( save initial drag position for later use )
        inputMgr.RotateDown
        .Subscribe(pos =>
            {
                rotateStartPosition = pos;
                dragOriginPosition = this.transform.localPosition;
                raycastMgr.RaycastGround(pos, out dragStartPosition);

                // Mouse specific code
                MouseCtrl.Show(dragStartPosition);
            })
        .AddTo(disposables);

        inputMgr.RotateUp
        .Subscribe(pos => MouseCtrl.Hide())
        .AddTo(disposables);

        // Mouse drag to rotate script
        inputMgr.Rotate
        .Subscribe(pos =>
            {
                var deltaYaw = (rotateStartPosition.x - pos.x) * settings.rotateHorizontalSpeed;
                var deltaPitch = (rotateStartPosition.y - pos.y) * settings.rotateVerticalSpeed;
                rotateStartPosition = pos;

                if (lockPositionToZero) {
                    Vector3 angles = transform.localEulerAngles;
                    angles.x += deltaPitch;
                    angles.y += deltaYaw;
                    angles.z = 0;

                    transform.localEulerAngles = angles;
                    return;
                }
                // Rotate horizontally
                // --------------------
                transform.RotateAround(dragStartPosition, Vector3.up, deltaYaw);
                if (!Use2DMode.Value) {
                    Vector3 originalPosition = transform.localPosition;
                    Quaternion originalRotation = transform.localRotation;


                    Vector3 newPos;
                    Quaternion newRot;
                    RotateAround(dragStartPosition, transform.right, deltaPitch, out newPos, out newRot);

                    // pitchValue = clampedRotation.eulerAngles.x;

                    var clampedRotation = ClampRotationAroundXAxis(newRot);

                    Vector3 angles = clampedRotation.eulerAngles;
                    angles.y = originalRotation.eulerAngles.y;
                    angles.z = 0;

                    transform.localPosition = newPos;
                    transform.localEulerAngles = angles;


                    if (!isEqual(clampedRotation.eulerAngles.x, newRot.eulerAngles.x)) {
                        // Debug.Log($"OUTSIDE {clampedRotation.eulerAngles.x} = {newRot.eulerAngles.x}");
                        transform.localPosition = ConstrainToBounds(originalPosition);
                        transform.localRotation = originalRotation;
                    }else{
                        float distance = 0f;
                        var ray = new Ray(camera.transform.position, camera.transform.forward);
                        var raycastResult = new Plane(Vector3.up, 0).Raycast(ray, out distance);

                        if (raycastResult && distance <= (settings.minZoom + settings.maxZoom)) {
                            var zoomEvalReverse = (distance - settings.minZoom) / settings.maxZoom;
                            zoom = Mathf.Clamp01(zoomEvalReverse);
                            UpdateCameraTransform(zoom);

                            this.transform.localPosition = ConstrainToBounds(ray.GetPoint(distance));

                            // Debug.Log("INSIDE");
                        }else{
                            // Debug.Log($"INSIDE ( could not raycast ) {distance}");
                            // transform.localRotation = originalRotation;
                            transform.localPosition = originalPosition;
                        }
                    }
                }

                // --------------------
                // Force new position ( prevents smooth camera movement issues )
                startPosition = destinationPosition = this.transform.localPosition;

                // Debug.DrawRay(dragStartPosition, Vector3.up * 5, Color.red, 1f);
            })
        .AddTo(disposables);

        Use2DMode
        .Subscribe(enabled =>
            {
                Vector3 angles = transform.localRotation.eulerAngles;
                LeanTween.rotate(this.gameObject, new Vector3(enabled ? 0 : 45, angles.y, 0), 0.5f).setEase(LeanTweenType.easeInOutCirc);
            })
        .AddTo(disposables);
    }

    private void OnDisable ()
    {
        disposables.Dispose();
    }

    // ------------------------------------------------
    // Updates camera position instantly ( used in zoom and normal update )
    private void UpdateCameraTransform (float value)
    {
        var zoomEval = settings.minZoom + (value * settings.maxZoom);

        camera.localPosition = new Vector3(0, 0, -zoomEval);
    }

    // Update camera position in a smooth silky way
    private void Update ()
    {
        var d = Time.unscaledDeltaTime / settings.tweenTime; // We want to move the timer from 0 to 1, so we divide Time.deltaTime by totalTime

        zoomTimer += d;
        positionTimer += d;
        zoomSmoothed = Mathf.Lerp(startZoom, zoom, settings.tweenCurve.Evaluate(zoomTimer));

        UpdateCameraTransform(zoomSmoothed);

        this.transform.localPosition = Vector3.Lerp(startPosition, destinationPosition, settings.tweenCurve.Evaluate(positionTimer));
    }

    private void LateUpdate ()
    {
        // Update mouse anchor
        Vector3 originalPosition = camera.position;
        Quaternion originalRotation = camera.rotation;

        Camera.main.transform.SetPositionAndRotation(camera.position, camera.rotation);

        MouseCtrl.SetWorldPosition(dragStartPosition);

        Camera.main.transform.SetPositionAndRotation(originalPosition, originalRotation);
    }

    // ------------------------------------------------


    public void Toggle2DMode ()
    {
        Use2DMode.Value = !Use2DMode.Value;
    }

    public void ZoomBy (float delta)
    {
        zoomTimer = 0f;
        startZoom = zoomSmoothed;
        zoom = Mathf.Clamp01(zoom - delta * settings.manualZoomSteps * settings.zoomCurve.Evaluate(zoom));
    }

    public void RotateByDegrees (int degree)
    {
        Vector3 angles = transform.localRotation.eulerAngles;

        // Transform current rotation to snap to 90 degrees ( floor or ceil depending on )
        float deltaYaw = Mathf.DeltaAngle(0, angles.y) / 90f;

        deltaYaw = (degree > 0) ? Mathf.FloorToInt(deltaYaw) : Mathf.CeilToInt(deltaYaw);
        deltaYaw *= 90f;

        if (!LeanTween.isTweening(this.gameObject)) {
            LeanTween.rotate(this.gameObject, new Vector3(angles.x, deltaYaw + degree, 0), 0.5f).setEase(LeanTweenType.easeInOutCirc);
        }
    }

    public void ResetToNorth ()
    {
        Vector3 angles = transform.localRotation.eulerAngles;

        if (!LeanTween.isTweening(this.gameObject)) {
            LeanTween.rotate(this.gameObject, new Vector3(angles.x, 0, 0), 0.5f).setEase(LeanTweenType.easeInOutCirc);
        }
    }

    public void SetDestination (Vector3 pos)
    {
        positionTimer = 0f;
        startPosition = this.transform.localPosition;
        destinationPosition = pos;
    }

    // Helper function
    public Vector3 ConstrainToBounds (Vector3 p)
    {
        var constrainedPos = Vector3.ClampMagnitude(p, settings.RadiusBound);

        constrainedPos.y = 0;
        return constrainedPos;
    }

    private void RotateAround (Vector3 center, Vector3 axis, float angle, out Vector3 pos, out Quaternion rotation)
    {
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = this.transform.position - center; // find current direction relative to center

        dir = rot * dir; // rotate the direction
        pos = center + dir; // define new position
        Quaternion myRot = transform.rotation;
        rotation = myRot * Quaternion.Inverse(myRot) * rot * myRot;
    }

    bool isEqual (float a, float b)
    {
        if (a >= b - 0.0001f && a <= b + 0.0001f)
            return true;
        else
            return false;
    }

    Quaternion ClampRotationAroundXAxis (Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, rotatePitchClamp.x, rotatePitchClamp.y);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
    // ------------------------------------------------

    // Debug viz
    private void OnDrawGizmosSelected ()
    {
        // Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        // Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = new Color(0f, 0f, 1f, 1f);
        // Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.DrawWireSphere(Vector3.zero, settings.RadiusBound);
    }
}