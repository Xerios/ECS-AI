using Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using UtilityAI;

public class GameManager : MonoSingleton<GameManager>
{
    public enum ClickMode {
        Default,
        Assign,
        Build,
        Destroy
    }
    public JobInventoryData Data;
    public ClickMode clickMode = ClickMode.Default;

    public GameObject selectionPrefab, moveCommandPrefab, commandReceivePrefab, pausedScreen, poiPrefab;

    public BuildButton buildButton;
    public Func<Vector3, bool> buildCheck;
    public Action buildConfirm;
    public GameObject buildPrefab { get; set; } = null;

    private GameObject buildClone;
    private GameObject selectedGameObject;

    private ObjectPopup sideUI;

    public TMPro.TextMeshProUGUI debugText, infoText;
    private bool paused;
    private CompositeDisposable buildMouseEvents, assignMouseEvents;
    private bool buildCanPlace;

    public Vector3 StorageSpace;

    [NonSerialized]
    public ReactiveCommand AgentJobChange = new ReactiveCommand();
    public ReactiveCollection<Agent> Agents = new ReactiveCollection<Agent>();

    public NativeMultiHashMap<int, Entity> Storage;
    private GameObject currentPOI;

    // private LineRenderer lineRender;

    void OnDestroy ()
    {
        Storage.Dispose();
    }

    // Start is called before the first frame update
    void Start ()
    {
        Storage = new NativeMultiHashMap<int, Entity>(10, Allocator.Persistent);

        GameResources.Current = new GameResources {
            Population = 0,
            Wood = 50,
            Iron = 0,
        };

        GameResources.Max = new GameResources {
            Population = 0,
            Wood = 50,
            Iron = 10,
        };

        Deselect();

        // lineRender = GetComponent<LineRenderer>();

        InputManager.Instance.MousePosition.Subscribe((pos) => {
                if (clickMode == ClickMode.Build) {
                    StatsInfoTooltip.Instance.Hide();
                    return;
                }
                if (!InputManager.Instance.IsMouseNotOverUI(pos)) {
                    StatsInfoTooltip.Instance.Hide();
                    return;
                }
                GameObject go;
                var layers = RaycastManager.Instance.layerMaskUnits;
                if (RaycastManager.Instance.RaycastProximityLayer(pos, layers, out go)) {
                    StatsInfoTooltip.Instance.Show(go);
                }else{
                    StatsInfoTooltip.Instance.Hide();
                }
            });

        InputManager.Instance.Click.Subscribe((pos) => {
                if (clickMode == ClickMode.Build && buildCanPlace && buildCheck.Invoke(buildClone.transform.position)) {
                    buildConfirm.Invoke();
                    buildClone.GetComponent<Placable>().DisablePreviewMode();
                    OnBuildStart();
                    Bootstrap.world.GetOrCreateSystem<InfluenceMapSystem>().UpdateOnce(InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES);
                    return;
                }

                if (clickMode == ClickMode.Assign) {
                    var layers = RaycastManager.Instance.layerMaskBuildings | RaycastManager.Instance.layerMaskResource;
                    if (RaycastManager.Instance.RaycastProximityLayer(pos, layers, out var go)) {
                        var goEntity = go.GetComponent<EntityMonoBehaviour>();
                        if (goEntity == null) return;
                        var matches = false;

                        if (World.Active.EntityManager.HasComponent<SignalAbilityAssignment>(goEntity.GetEntity())) {
                            var compatibleAbilities = World.Active.EntityManager.GetComponentData<SignalAbilityAssignment>(goEntity.GetEntity()).Ability;
                            matches = Data.IsAbilityCompatible(FloatingPopup.Instance.LastIndex, compatibleAbilities);
                        }

                        if (matches) {
                            OnAssignCancel();
                            Select(go);
                            return;
                        }else{
                            return;
                        }
                    }

                    if (Data.IsAbilityCompatible(FloatingPopup.Instance.LastIndex, (byte)UtilityAI.AbilityTags.Defend)) {
                        if (RaycastManager.Instance.RaycastGround(pos, out Vector3 hitPos)) {
                            Deselect();
                            currentPOI = GameObject.Instantiate(poiPrefab, hitPos, Quaternion.identity);
                            currentPOI.GetComponent<EntityMonoBehaviour>().NoAutoInit();
                            currentPOI.GetComponent<EntityMonoBehaviour>().Init();
                            Select(currentPOI);
                            OnAssignCancel();
                            return;
                        }
                    }
                }

                if (clickMode == ClickMode.Destroy) {
                    // Vector3 hitPos;
                    // if (RaycastManager.Instance.RaycastGround(pos, out hitPos)) {
                    //     Collider[] hitResults = new Collider[1];
                    //     var count = Physics.OverlapSphereNonAlloc(hitPos, 2f, hitResults, RaycastManager.Instance.layerMaskBuildings);
                    //     if (count != 0) {
                    //         var gameObj = hitResults[0].transform.root.gameObject;
                    //         if (gameObj.GetComponent<EntityMonoBehaviour>() != null) Destroy(gameObj);
                    //     }
                    // }
                }

                if (clickMode == ClickMode.Default) {
                    GameObject go;
                    var layers = RaycastManager.Instance.layerMaskBuildings | RaycastManager.Instance.layerMaskResource;
                    if (RaycastManager.Instance.RaycastProximityLayer(pos, layers, out go)) {
                        if (selectedGameObject != null && currentPOI == go) {
                            // Debug.Log($"Clicked on the same {currentPOI} == {go}");
                            return;
                        }

                        if (selectedGameObject == go) {
                            if (!RaycastManager.Instance.RaycastPreciseLayer(pos, layers, out go)) {
                                Deselect();
                                // ObjectPopup.Instance.Hide();
                                return;
                            }
                        }
                        Deselect();

                        Select(go);
                    }else{
                        Deselect();
                        FloatingPopup.Instance.Hide();
                    }
                }
            });

        InputManager.Instance.SecondaryClick.Subscribe((pos) => {
                Deselect();
                FloatingPopup.Instance.Hide();
            });
        // InputManager.Instance.SecondaryClick.Subscribe((pos) => {
        //         if (clickMode == ClickMode.Default) {
        //             Vector3 hitPos;
        //             if (RaycastManager.Instance.RaycastGround(pos, out hitPos)) {
        //                 if (selectedGameObject == null) {
        //                     Collider[] hitResults = new Collider[300];
        //                     var count = Physics.OverlapSphereNonAlloc(hitPos, 40f, hitResults, RaycastManager.Instance.layerMaskUnits, QueryTriggerInteraction.Ignore);

        //                     for (int i = 0; i < count; i++) {
        //                         var agent = hitResults[i].GetComponent<Agent>();
        //                         if (agent == null) continue;
        //                         if (agent.GetMind() == null) continue;

        //                         var agentEntity = agent.GetEntity();

        //                         EntityManager mgr = AIManager.Instance.mgr;

        //                         var entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        //                         mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.TriggerAction });
        //                         mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.OVERRIDE });
        //                         mgr.AddComponentData(entity, new SignalCastToTargets { Target = agentEntity });
        //                         mgr.AddComponentData(entity, new SignalReferences { Count = -1 });
        //                         mgr.AddSharedComponentData(entity, new SignalAction { data = () => MoveCommand(hitPos) });

        //                         GameObject.Instantiate(commandReceivePrefab, agent.transform);
        //                     }
        //                 }else{
        //                     var agent = selectedGameObject.GetComponent<Agent>();
        //                     if (agent == null) return;

        //                     var agentEntity = agent.GetEntity();

        //                     EntityManager mgr = AIManager.Instance.mgr;

        //                     var entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        //                     mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.TriggerAction });
        //                     mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.OVERRIDE });
        //                     mgr.AddComponentData(entity, new SignalCastToTargets { Target = agentEntity });
        //                     mgr.AddComponentData(entity, new SignalReferences { Count = -1 });
        //                     mgr.AddSharedComponentData(entity, new SignalAction { data = () => MoveCommand(hitPos) });
        //                     // mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        //                     // Bootstrap.world.EntityManager
        //                     // .GetBuffer<AddNewTargets>(agent.GetMind().Entity)
        //                     // .Add(new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));
        //                     GameObject.Instantiate(commandReceivePrefab, agent.transform);
        //                 }

        //                 GameObject.Instantiate(moveCommandPrefab, hitPos + new Vector3(0, 0.1f, 0), Quaternion.identity);
        //             }
        //         }
        //     });
    }

    public void Select (GameObject go)
    {
        if (go.GetComponent<EntityMonoBehaviour>() == null) return;

        FloatingPopup.Instance.Show(go);

        // if (selectedGameObject != go) {
        //     selectedGameObject = go;
        //     foreach (var item in go.GetComponentsInChildren<Renderer>(true)) if (item.gameObject.name != "Shadow") item.gameObject.AddComponent<Knife.PostProcessing.OutlineRegister>();
        // }

        if (selectedGameObject != go) {
            selectedGameObject = go;
            selectionPrefab.transform.position = go.transform.position;
            selectionPrefab.GetComponent<PositionConstraint>().SetSource(0, new ConstraintSource { sourceTransform = go.transform, weight = 1.0f });
            selectionPrefab.SetActive(true);
        }
        // }
#if UNITY_EDITOR
        debugText.text = go.name;
        UnityEditor.Selection.SetActiveObjectWithContext(go, go);
#endif
    }

    public void Deselect ()
    {
        debugText.text = "(Empty)";
        selectionPrefab.SetActive(false);

        if (currentPOI != null) {
            if (currentPOI == selectedGameObject) FloatingPopup.Instance.HideInstant();

            var entity = currentPOI.GetComponent<EntityMonoBehaviour>().GetEntity();
            if (!World.Active.EntityManager.HasComponent<SignalAssigned>(entity)) {
                Destroy(currentPOI);
            }
            currentPOI = null;
        }
        if (selectedGameObject == null) return;

        // foreach (var item in selectedGameObject.GetComponentsInChildren<Knife.PostProcessing.OutlineRegister>(true)) Destroy(item);

        selectedGameObject = null;
        selectionPrefab.GetComponent<PositionConstraint>().SetSource(0, new ConstraintSource());
    }

    // ------------------------------------------------------------------

    public void OnAssignClick (short id)
    {
        // Debug.Log("OnAssignClick");
        Deselect();
        OnAssignCancel();
        OnBuildCancel();

        FloatingPopup.Instance.HideInstant();

        FloatingPopup.Instance.LastIndex = id;

        clickMode = ClickMode.Assign;

        if (assignMouseEvents != null) {
            return;
        }
        assignMouseEvents = new CompositeDisposable();
        InputManager.Instance.SecondaryClick.Skip(1).Subscribe((pos) => OnAssignCancel()).AddTo(assignMouseEvents);
        InputManager.Instance.MousePosition.Subscribe(OnAssignMove).AddTo(assignMouseEvents);
    }

    private void OnAssignMove (Vector2 pos)
    {
        if (!InputManager.Instance.IsMouseNotOverUI(pos)) {
            Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
            return;
        }
        var layers = RaycastManager.Instance.layerMaskBuildings | RaycastManager.Instance.layerMaskResource;

        if (RaycastManager.Instance.RaycastProximityLayer(pos, layers, out var go) && go != null) {
            var goEntity = go.GetComponent<EntityMonoBehaviour>();
            if (goEntity == null) return;
            var matches = false;
            var alreadyExists = false;

            if (World.Active.EntityManager.HasComponent<SignalAbilityAssignment>(goEntity.GetEntity())) {
                var compatibleAbilities = World.Active.EntityManager.GetComponentData<SignalAbilityAssignment>(goEntity.GetEntity()).Ability;
                matches = Data.IsAbilityCompatible(FloatingPopup.Instance.LastIndex, compatibleAbilities);

                alreadyExists = World.Active.EntityManager.HasComponent<SignalAssigned>(goEntity.GetEntity());
            }

            if (matches) {
                if (alreadyExists) {
                    Cursor.SetCursor(CursorData.Instance.BroadcastReplace.Cursor, CursorData.Instance.BroadcastReplace.HotSpot, CursorMode.Auto);
                }else{
                    Cursor.SetCursor(CursorData.Instance.Broadcast.Cursor, CursorData.Instance.Broadcast.HotSpot, CursorMode.Auto);
                }
            }else{
                Cursor.SetCursor(CursorData.Instance.NotAllowed.Cursor, CursorData.Instance.NotAllowed.HotSpot, CursorMode.Auto);
            }
        }else if (Data.IsAbilityCompatible(FloatingPopup.Instance.LastIndex, (byte)UtilityAI.AbilityTags.Defend)) {
            Cursor.SetCursor(CursorData.Instance.SetupPOI.Cursor, CursorData.Instance.SetupPOI.HotSpot, CursorMode.Auto);
        }else{
            Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
        }
    }

    public void OnAssignCancel ()
    {
        // Debug.Log("OnAssignCancel");
        if (clickMode != ClickMode.Assign) return;

        JobListUI.Instance.Deselect();

        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
        clickMode = ClickMode.Default;

        assignMouseEvents.Dispose();
        assignMouseEvents = null;
    }
    // ------------------------------------------------------------------

    public void OnBuildClick ()
    {
        OnAssignCancel();
        OnBuildCancel();

        OnBuildStart();
    }

    private void OnBuildStart ()
    {
        Deselect();
        // ObjectPopup.Instance.Hide();
        clickMode = ClickMode.Build;

        buildClone = GameObject.Instantiate(buildPrefab);
        buildClone.GetComponent<Placable>().PreviewMode = true;

        if (buildMouseEvents != null) {
            OnBuildMove(InputManager.Instance.GetMousePos());
            return;
        }
        buildMouseEvents = new CompositeDisposable();
        InputManager.Instance.SecondaryClick.Skip(1).Subscribe((pos) => OnBuildCancel()).AddTo(buildMouseEvents);
        InputManager.Instance.MousePosition.Subscribe(OnBuildMove).AddTo(buildMouseEvents);
    }

    private void OnBuildMove (Vector2 pos)
    {
        buildCanPlace = false;
        if (RaycastManager.Instance.RaycastGround(pos, out Vector3 hitPos)) {
            hitPos.y = 0;
            var gridPos = (Vector3)(math.floor(hitPos / InfluenceMapSystem.WORLD_SCALE) + new float3(0.5f, 0f, 0.5f)) * InfluenceMapSystem.WORLD_SCALE;

            buildClone.transform.position = gridPos;

            buildCanPlace = buildCheck.Invoke(gridPos);
        }

        buildClone.GetComponent<Placable>().PreviewMaterial.color = buildCanPlace ? new Color(0.2f, 0.5f, 0f) : new Color(0.5f, 0.1f, 0f);
    }

    private void OnBuildCancel ()
    {
        if (clickMode != ClickMode.Build) return;

        clickMode = ClickMode.Default;
        buildMouseEvents.Dispose();
        buildMouseEvents = null;
        Destroy(buildClone);
    }

    // ------------------------------------------------------------------

    private StateScript MoveCommand (Vector3 pos)
    {
        return new StateScript("Move Command",
                   new StateDefinition("move") {
                       {
                           StateDefinition.____BEGIN____,
                           //    (context) => Debug.Log("Start Move Command"),
                           (context) => context.data.SetVec("destination", pos),
                           ActionsTest.MoveToDestination,
                           ActionsTest.HasArrivedToDestination
                       },
                       {
                           StateDefinition.____END____,
                           ActionsTest.Forget
                       }
                   });
    }
    // ------------------------------------------------------------------

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            paused = !paused;
            // Bootstrap.loop.enabled = !paused;
            FindObjectOfType<TimelineUI>().ChangeTime(paused ? 0 : 1);

            var canvas = pausedScreen.GetComponent<CanvasGroup>();
            LeanTween.cancel(pausedScreen);
            if (paused) {
                pausedScreen.SetActive(true);
                canvas.alpha = 0;
                LeanTween.alphaCanvas(canvas, 1f, 0.4f).setEaseOutExpo().setIgnoreTimeScale(true);
            }else{
                LeanTween.alphaCanvas(canvas, 0f, 0.4f).setEaseInExpo().setOnComplete(() => pausedScreen.SetActive(false)).setIgnoreTimeScale(true);
            }
        }

        infoText.text = $"Population:<indent=55%>{GameResources.Current.Population} <color=#666>/ {GameResources.Max.Population}</color></indent>\n" +
            $"Wood:<indent=55%>{GameResources.Current.Wood} <color=#666>/ {GameResources.Max.Wood}</color></indent>\n" +
            $"Iron:<indent=55%>{GameResources.Current.Iron} <color=#666>/ {GameResources.Max.Iron}</color></indent>\n" +
            $"Faith:<indent=55%>{GameResources.Current.Faith} <color=#666>/ {GameResources.Max.Faith}</color></indent>\n";
        // debugText.text = ";
    }
}