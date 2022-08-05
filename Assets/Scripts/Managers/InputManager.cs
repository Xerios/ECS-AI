using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoSingleton<InputManager>
{
    private const float mouseDistanceToDrag = 20f;

    // ------------------------------------------
    public IObservable<Vector2> DragCamera { get; private set; }
    public IObservable<Vector2> DragCameraDown { get; private set; }
    public IObservable<Vector2> DragCameraUp { get; private set; }
    // ------------------------------------------
    public IObservable<Vector2> Click { get; private set; }
    public IObservable<Vector2> SecondaryClick { get; private set; }
    // ------------------------------------------
    public IObservable<Vector2> Rotate { get; private set; }
    public IObservable<Vector2> RotateDown { get; private set; }
    public IObservable<Vector2> RotateUp { get; private set; }
    // ------------------------------------------
    public IObservable<Vector2> MousePosition { get; private set; }
    public IObservable<Vector2> Movement { get; private set; }
    public IObservable<float> Zoom { get; private set; }
    // ------------------------------------------
    private ReactiveProperty<Vector2> mousePos = new ReactiveProperty<Vector2>();

    private ReactiveProperty<Vector2> clickDown = new ReactiveProperty<Vector2>();
    private Subject<Vector2> clickUp = new Subject<Vector2>();

    private ReactiveProperty<Vector2> secondaryDown = new ReactiveProperty<Vector2>();
    private Subject<Vector2> secondaryUp = new Subject<Vector2>();

    private Subject<Vector2> rotateDown = new Subject<Vector2>();
    private Subject<Vector2> rotateUp = new Subject<Vector2>();

    private Subject<Vector2> movement = new Subject<Vector2>();
    private Subject<float> zoom = new Subject<float>();
    // ------------------------------------------


    public void OnEnable ()
    {
        // ------------------------------------------
        MousePosition = mousePos.Publish().RefCount();
        Movement = movement.Publish().RefCount();
        Zoom = zoom.Where(IsMouseNotOverUI).Where(IsMouseInsideWindow).Publish().RefCount();

        // ------------------------------------------
        // Mouse drag logic

        DragCameraDown = secondaryDown.Skip(1)
            .Where(IsMouseNotOverUI)
            .SkipWhile(x => Input.GetMouseButton(0))
            .SelectMany(dragDownValue => mousePos.TakeUntil(secondaryUp)
                .Where(IsOutsideSecondaryClickRange)
                .Take(1)
                )
            .Select(x => secondaryDown.Value)
            .ToReadOnlyReactiveProperty();

        DragCamera = DragCameraDown
            .SelectMany(x => mousePos.TakeUntil(secondaryUp).Skip(1))
            .ToReadOnlyReactiveProperty();

        DragCameraUp = secondaryUp.ToReadOnlyReactiveProperty();

        // ------------------------------------------

        Click = clickUp
            .Skip(1)
            .Where(IsMouseNotOverUI)
            .SkipWhile(x => Input.GetMouseButton(1))
            .Where(IsInsideClickRange)         // Make sure we haven't been dragging our mouse beforehand
            .RepeatSafe()
            .ToReadOnlyReactiveProperty();

        SecondaryClick = secondaryUp
            .Skip(1)
            .Where(IsMouseNotOverUI)
            .SkipWhile(x => Input.GetMouseButton(1))
            .Where(IsInsideSecondaryClickRange)         // Make sure we haven't been dragging our mouse beforehand
            .RepeatSafe()
            .ToReadOnlyReactiveProperty();

        // ------------------------------------------
        RotateDown = rotateDown
            .Where(IsMouseNotOverUI)
            .SkipWhile(x => Input.GetMouseButton(1))
            .Publish()
            .RefCount();

        Rotate = RotateDown
            .SelectMany(x => mousePos.TakeUntil(rotateUp))
            .Publish()
            .RefCount();

        RotateUp = rotateUp.Publish().RefCount();
    }


    // Update is called once per frame
    public void Update ()
    {
        if (Input.GetKey(KeyCode.UpArrow)) movement.OnNext(Vector2.up);
        if (Input.GetKey(KeyCode.DownArrow)) movement.OnNext(Vector2.down);
        if (Input.GetKey(KeyCode.LeftArrow)) movement.OnNext(Vector2.left);
        if (Input.GetKey(KeyCode.RightArrow)) movement.OnNext(Vector2.right);

        var mouseRaw = Input.mousePosition;
        if (!mouseRaw.Equals(mousePos.Value)) mousePos.Value = mouseRaw;
        if (Input.mouseScrollDelta.y != 0) zoom.OnNext(Input.mouseScrollDelta.y);

        if (Input.GetMouseButtonDown(0)) clickDown.Value = mousePos.Value;
        if (Input.GetMouseButtonUp(0)) clickUp.OnNext(mousePos.Value);

        if (Input.GetMouseButtonDown(1)) secondaryDown.Value = mousePos.Value;
        if (Input.GetMouseButtonUp(1)) secondaryUp.OnNext(mousePos.Value);

        if (Input.GetMouseButtonDown(2)) rotateDown.OnNext(mousePos.Value);
        if (Input.GetMouseButtonUp(2)) rotateUp.OnNext(mousePos.Value);
    }

    public Vector2 GetMousePos () => mousePos.Value;

    private bool IsMouseInsideWindow (float _) => mousePos.Value.x > 0 && mousePos.Value.y > 0 && mousePos.Value.x < Screen.width && mousePos.Value.y < Screen.height;
    private bool IsMouseNotOverUI (float _) => !EventSystem.current.IsPointerOverGameObject();
    public bool IsMouseNotOverUI (Vector2 _) => !EventSystem.current.IsPointerOverGameObject();
    private bool IsOutsideClickRange (Vector2 pos) => !IsInsideClickRange(pos);
    private bool IsInsideClickRange (Vector2 pos) => Vector3.Distance(pos, clickDown.Value) <= mouseDistanceToDrag;

    private bool IsOutsideSecondaryClickRange (Vector2 pos) => !IsInsideSecondaryClickRange(pos);
    private bool IsInsideSecondaryClickRange (Vector2 pos) => Vector3.Distance(pos, secondaryDown.Value) <= mouseDistanceToDrag;
}