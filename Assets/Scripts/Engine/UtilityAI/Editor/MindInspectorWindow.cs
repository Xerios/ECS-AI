using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UtilityAI;

public class MindInspectorWindow : EditorWindow
{
    public static Transform selectedObject;
    public static Agent selectedAgent;

    private Vector2 scrollPosition = Vector2.zero;
    private float timelineStart = 0, timelineEnd = 10;
    private const float timelineMax = 30f;

    private bool showDetails = false, hideNegative = false, sortDecisions = true;
    private bool[] toggleBools = new bool[10];


    [MenuItem("Tools/AI/Mind Inspector")]
    public static void ShowWindow ()
    {
        MindInspectorWindow window = (MindInspectorWindow)EditorWindow.GetWindow(typeof(MindInspectorWindow), false, "Mind Debugger");

        window.Show();
    }

    public MindInspectorWindow()
    {
        toggleBools[1] = true;
    }

    public void OnSelectionChange ()
    {
        selectedObject = Selection.activeTransform;
        if (selectedObject != null) selectedAgent = selectedObject.GetComponentInChildren<Agent>();

        Repaint();
    }

    private void OnHierarchyChange ()
    {
        OnSelectionChange();
    }

    public void OnGUI ()
    {
        GUI.color = Color.white;
        GUILayout.BeginHorizontal();
        GUILayout.Label("TimeScale: ");
        Time.timeScale = EditorGUILayout.Slider(Time.timeScale, 0.0f, 10.0f);
        GUILayout.EndHorizontal();
        var newDebugger = (Agent)EditorGUILayout.ObjectField("Selected Debugger:", selectedAgent, typeof(Agent), true);

        if (newDebugger != selectedAgent) {
            selectedAgent = newDebugger;
            if (newDebugger != null) selectedObject = selectedAgent.transform;
        }

        if (!Application.isPlaying) {
            EditorGUILayout.HelpBox("Cannot use this utility in Editor Mode", MessageType.Info);
            return;
        }
        if (selectedObject == null) {
            EditorGUILayout.HelpBox("Please select an object", MessageType.Info);
            return;
        }else if (selectedAgent == null) {
            EditorGUILayout.HelpBox("This object does not contain an Agent component", MessageType.Info);
            return;
        }else if (selectedAgent.GetMind() == null) {
            EditorGUILayout.HelpBox("Mind is null", MessageType.Info);
            return;
        }

        Mind ai = selectedAgent.GetMind();

        if (ai.GetEntity().Index < 0 || ai.GetEntity().Equals(Entity.Null)) {
            EditorGUILayout.HelpBox("Entity is null, not yet initialized?", MessageType.Info);
            return;
        }

        var decisionList = UtilityAILoop.Instance.calculateDecisionsSystem.decisions;


        // EditorGUILayout.LabelField("ID: " + ai.GetID(), EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.MinMaxSlider(ref timelineStart, ref timelineEnd, 0f, timelineMax);
        // timelineScale = EditorGUILayout.Slider(timelineScale, 1f, 30f);

        var containerRect = GUILayoutUtility.GetLastRect();
        var dragRect = new Rect(containerRect.x, containerRect.y, containerRect.width, 60);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUIStyle historyFontStyle = new GUIStyle(EditorStyles.miniLabel);

        if (m_Dragging && m_PositionDelta.sqrMagnitude > 1f) {
            var posXinContainer = (Event.current.mousePosition.x - containerRect.x) / containerRect.xMax;
            var deltaXinContainer = m_PositionDelta.x / containerRect.width;
            var sliderWidth = (timelineEnd - timelineStart);

            var newTimeStart = timelineStart - deltaXinContainer * (sliderWidth);
            var newTimeEnd = timelineEnd - deltaXinContainer * (sliderWidth);

            // var newTimeStart = timelineStart + m_Position.x * -1 / containerRect.width * (timelineEnd - timelineStart);
            // var newTimeEnd = timelineEnd + m_Position.x * -1 / containerRect.width * (timelineEnd - timelineStart);

            SetTimeRangeAndClamp(newTimeStart, newTimeEnd, sliderWidth);

            var timeCenter = posXinContainer * sliderWidth;// (sliderWidth/timelineMax) * posXinContainer * m_Position;
            var scrollAmount = m_PositionDelta.y * -0.01f;

            // Debug.Log($"M:{timeCenter}, ScrollAmmount:{scrollAmount}");

            newTimeStart = timelineStart - (posXinContainer) * sliderWidth * scrollAmount;
            newTimeEnd = timelineEnd + (1f - posXinContainer) * sliderWidth * scrollAmount;
            if (newTimeEnd > timelineMax) {
                newTimeStart += timelineMax - newTimeEnd;
            }
            timelineStart = Mathf.Clamp(newTimeStart, 0f, timelineMax - 1f);
            timelineEnd = Mathf.Clamp(newTimeEnd, timelineStart + 1f, timelineMax);
        }

        // -----------------------------------------------------------------------------------------------
        containerRect.y -= 16;

        var buffer = AIManager.Instance.mgr.GetBuffer<DecisionHistoryRecord>(ai.GetEntity());

        DrawLinearTimeline(containerRect, 16, buffer, timelineStart, timelineEnd, (dseId, target, rect) => {
                Decision decision;
                decisionList.TryGetValue(dseId, out decision);

                Color color = ColorExtensions.GetColorFromString(decision?.ToString());

                EditorGUI.DrawRect(rect, color);
                historyFontStyle.normal.textColor = color.GetOppositeColor();

                GUI.Label(rect, new GUIContent($"{decision} #{target.Index}"), historyFontStyle);

                EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, 1, rect.height), Color.black);
            });

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        // -----------------------------------------------------------------------------------------------

        showDetails = EditorGUILayout.ToggleLeft("Show details", showDetails, GUILayout.MinWidth(20));

        // -----------------------------------------------------------------------------------------------

        // DrawHeaderToggle("DecisionHistory", ref toggleBools[0]);

        // for (int i = 0; i < buffer.Length; i++) {
        //     EditorGUILayout.LabelField($"{i}.  {buffer[i].dseId} ({buffer[i].target}) : {buffer[i].StartTime.ToString("F1")} -> {buffer[i].EndTime.ToString("F1")}");
        // }

        // -----------------------------------------------------------------------------------------------
        DrawHeaderToggle("Sequence", ref toggleBools[0]);

        if (toggleBools[0]) {
            var actionMgr = selectedAgent.GetActionManager();
            EditorGUILayout.LabelField($"Count: {actionMgr.SMachine.GetStates().Count()}", EditorStyles.centeredGreyMiniLabel);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var stateScript in actionMgr.SMachine.GetStates()) {
                EditorGUILayout.LabelField($"StateScript: {stateScript.Name}");

                foreach (var state in stateScript.States) {
                    GUI.color = state.IsActive() ? Color.green : Color.white;
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"State: {state.meta.Id} ({state.CurrentEvent})");
                    if (state.GetTracks() != null) {
                        foreach (var track in state.GetTracks()) {
                            EditorGUILayout.LabelField($"▷ Track({track.Meta.Id}) : #{track.CurrentIndex} out of {track.Length}");
                            if (track.CurrentIndex >= 0 && track.CurrentIndex != track.Meta.Commands.Length) {
                                var name = track.Meta.Commands[track.CurrentIndex].Method.Name;
                                var isWaiting = track.IsWaiting - Time.time;
                                EditorGUILayout.LabelField($"▷▷ Cmd: {name} {(isWaiting > 0 ? $"[Waiting {isWaiting.ToString("F1")}s]" : "")}");
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
//             var currentInteraction = actionMgr.CurrentInteraction;

//             if (actionMgr.instance.CurrentInteraction != null) {
//                 EditorGUILayout.LabelField($"Node Count: {actionMgr.instance.NodeCount}", EditorStyles.centeredGreyMiniLabel);
//                 GUILayout.BeginVertical(EditorStyles.helpBox);
//                 for (int i = 0; i < actionMgr.instance.CurrentInteraction.Nodes.Length; i++) {
//                     EditorGUILayout.LabelField($"Node: {actionMgr.instance.CurrentInteraction.Meta.Nodes[i].Name} = {actionMgr.instance.NodeStates[i]}");
//                 }
//                 GUILayout.EndVertical();
//             }

//             if (showDetails) {
//                 GUILayout.BeginVertical(EditorStyles.helpBox);
//                 {
// #if UNITY_EDITOR && !NO_DEBUG
//                     GUIStyle richtext = new GUIStyle { richText = true };
//                     float ctime = 0;
//                     Interaction cinteraction = default(Interaction);
//                     DecisionHistory cdecision = default(DecisionHistory);
//                     ActionResult cresult = ActionResult.Uninitialized;
//                     short cnodeIndex = -999;

//                     foreach (var logitem in actionMgr.DebugHistory) {
//                         if (!ctime.Equals(logitem.time)) {
//                             ctime = logitem.time;
//                             EditorGUILayout.LabelField(logitem.time.ToString("F1"), EditorStyles.centeredGreyMiniLabel);
//                         }
//                         if (cinteraction != logitem.interaction) {
//                             cinteraction = logitem.interaction;
//                             EditorGUILayout.LabelField($"<color=grey>[{logitem.interaction}]</color>", richtext);
//                         }
//                         if (!cdecision.Equals(logitem.decision)) {
//                             cdecision = logitem.decision;
//                             EditorGUILayout.LabelField($"<color=grey><i>[{logitem.decision}]</i></color>", richtext);
//                         }
//                         if (cnodeIndex != logitem.nodeIndex && logitem.nodeIndex != (byte)255) {
//                             cnodeIndex = logitem.nodeIndex;
//                             EditorGUILayout.LabelField($"<b>{logitem.nodeIndex}. [{cinteraction?.Meta.Nodes[logitem.nodeIndex].Name}]</b>", richtext);
//                         }
//                         if (cresult != logitem.result && logitem.result != ActionResult.Uninitialized) {
//                             cresult = logitem.result;
//                             EditorGUILayout.LabelField(logitem.result.ToString(), EditorStyles.miniLabel);
//                         }

//                         if (logitem.action != SubActionDebug.Actions.Null) {
//                             string logitemText = logitem.action.ToString();
//                             if (logitem.action == SubActionDebug.Actions.RequestNodeExit) logitemText += " > [" + logitem.transition.ToString() + "] > " + logitem.childIndex;
//                             if (logitem.action == SubActionDebug.Actions.ActivateNode) logitemText += " +++> " + logitem.nodeIndex;
//                             if (logitem.action == SubActionDebug.Actions.DeactivateNode) logitemText += " ---> " + logitem.nodeIndex;
//                             if (logitem.action == SubActionDebug.Actions.NodeActivated) logitemText += " +++> " + logitem.nodeIndex;
//                             if (logitem.action == SubActionDebug.Actions.NodeDeactivated) logitemText += " ---> " + logitem.nodeIndex;
//                             EditorGUILayout.LabelField(logitemText, EditorStyles.miniLabel);
//                         }
//                     }
// #endif
//                 }
//                 GUILayout.EndVertical();
//             }
//             // -----------------------------------------------------------------------------------------------
//             DrawSideHeader("Sequence History", ref toggleBools[2]);

//             if (toggleBools[2]) {
// #if UNITY_EDITOR && !NO_DEBUG
//                 showDetails = EditorGUILayout.ToggleLeft("Show details", showDetails, GUILayout.MinWidth(20));
//                 GUILayout.BeginVertical(EditorStyles.helpBox);
//                 {
//                     // var defaultTextColor = GUI.color;
//                     foreach (var seqHist in actionMgr.DebugHistory) {
//                         var current = seqHist.decision.GetDSEId() != 0 ? $"[{decisionList[seqHist.decision.GetDSEId()]}] #{seqHist.decision.GetSignalId().Index}" : "n/a";
//                         var newDec = seqHist.bestDecision.GetDSEId() != 0 ? $" -> [{decisionList[seqHist.bestDecision.GetDSEId()]}] #{seqHist.bestDecision.GetSignalId().Index}" : string.Empty;
//                         var icon = '►';
//                         if (seqHist.result == ActionResult.Exit) icon = '▬';
//                         else if (seqHist.result == ActionResult.Interrupted) icon = '▼';
//                         else if (seqHist.result == ActionResult.SuccessImmediate) icon = '▷';
//                         else if (seqHist.result == ActionResult.Repeat) icon = '←';
//                         else if (seqHist.result == ActionResult.Failure) icon = '���';
//                         else if (seqHist.result == ActionResult.Uninitialized) icon = 'X';
//                         else if (seqHist.result == ActionResult.Running) icon = '֍';
//                         // GUI.color = ColorExtensions.GetColorFromString(current.ToString());
//                         var group = 0;
//                         var name = "_____Catch_____";

//                         if (seqHist.action != -1) {
//                             group = seqHist.interaction.actions[seqHist.action].Group;
//                             name = seqHist.interaction.actions[seqHist.action].Name;
//                         }

//                         GUILayout.Label($"{seqHist.time.ToString().PadLeft(5)} {icon} {new String('│', group)}┌ {seqHist.interaction.Name}.{name}".PadRight(70) + $"( {current} )");
//                         if (showDetails) GUILayout.Label($"{seqHist.result} {newDec}", EditorStyles.miniLabel);
//                     }
//                     // GUI.color = defaultTextColor;
//                 }
//                 GUILayout.EndVertical();
// #else
//                 EditorGUILayout.HelpBox("NO_DEBUG flag is set", MessageType.Error);
// #endif
//             }
        }
        // -----------------------------------------------------------------------------------------------
        // DrawHeaderToggle("Properties", ref toggleBools[6]);

        // if (toggleBools[6]) {
        //     var propertiesBuffer = AIManager.Instance.mgr.GetBuffer<PropertyData>(ai.GetEntity());

        //     GUILayout.BeginVertical(EditorStyles.helpBox);
        //     for (int i = 0; i < propertiesBuffer.Length; i++) {
        //         var type = (PropertyType)propertiesBuffer[i].Type;
        //         var value = propertiesBuffer[i].Value;
        //         var max = 1000f;
        //         EditorGUILayout.LabelField(type.ToString(), value.ToString() + " / " + max.ToString());

        //         Rect rect = GUILayoutUtility.GetLastRect();
        //         float scale = value / max;
        //         Color color = new Color(1f - Mathf.Pow(scale, 20), scale, 0f, 0.3f);
        //         if (value != 0) EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width * scale, rect.height), color);
        //     }
        //     GUILayout.EndVertical();
        // }
        // -----------------------------------------------------------------------------------------------

        DrawHeaderToggle("Decisions", ref toggleBools[1]);

        if (toggleBools[1]) {
#if UNITY_EDITOR && !NO_DEBUG
            GUILayout.BeginHorizontal();
            sortDecisions = EditorGUILayout.ToggleLeft("Sort", sortDecisions, GUILayout.MinWidth(20));
            hideNegative = EditorGUILayout.ToggleLeft("Hide negative decisions", hideNegative, GUILayout.MinWidth(20));
            GUILayout.EndHorizontal();

            var decisionBuffer = AIManager.Instance.mgr.GetBuffer<DecisionOption>(ai.GetEntity());


            var list = new List<DecisionOption>(decisionBuffer.Length);
            for (int i = 0; i < decisionBuffer.Length; i++) list.Add(decisionBuffer[i]);

            var maxScore = Mathf.Max(1.0f, list.Count != 0 ? list.Max(x => x.Score) : 0.0f);

            if (sortDecisions) list.Sort((a, b) => - a.Score.CompareTo(b.Score));


            GUILayout.BeginVertical(EditorStyles.helpBox);

            for (int i = 0; i < list.Count; i++) {
                var decision = list[i];

                var isPositive = decision.Score > 0f;

                if (hideNegative && !isPositive) continue;

                GUI.color = isPositive ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                EditorGUILayout.LabelField($"[{decisionList[decision.DSEId]}] #{decision.TargetId.Index}         {(decision.Score*100f).ToString("F1")}%", (GUIStyle)"ProjectBrowserHeaderBgMiddle");
                GUI.color = Color.white;

                if (isPositive) {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    float scale = (decision.Score / maxScore);
                    Color color = new Color(1f - Mathf.Pow(scale, 20), scale, 0f, 0.5f);
                    EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width * scale, EditorGUIUtility.singleLineHeight + 2), color);
                }


                if (showDetails) {
                    var decisionConsiderationsBuffer = AIManager.Instance.mgr.GetBuffer<DebugConsideration>(ai.GetEntity());
                    var dse = decisionList[decision.DSEId];

                    if (!AIManager.Instance.mgr.Exists(decision.DecisionEntity)) {
                        EditorGUILayout.LabelField($"▷ (No longer exists)", GUILayout.ExpandWidth(true));
                        continue;
                    }

                    var weight = AIManager.Instance.mgr.GetComponentData<DecisionWeight>(decision.DecisionEntity);

                    EditorGUILayout.LabelField($"▷ Weight: {weight.Value.ToString("0.00")}", EditorStyles.label, GUILayout.ExpandWidth(true));

                    for (int j = 0; j < decisionConsiderationsBuffer.Length; j++) {
                        var consideration = decisionConsiderationsBuffer[j];
                        if (!decision.DecisionEntity.Equals(consideration.decisionEntity)) continue;

                        var ctype = AIManager.Instance.mgr.GetSharedComponentData<ConsiderationType>(consideration.considerationEntity).DataType;
                        var score = AIManager.Instance.mgr.GetComponentData<ConsiderationScore>(consideration.considerationEntity).Value;

                        EditorGUILayout.LabelField($"► {ctype.ToString().PadRight(50)}", $" ▸{score.ToString("0.00")}", EditorStyles.label, GUILayout.ExpandWidth(true));

                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            rect.width -= 150;
                            float scale = (score / 1.0f);
                            Color color = new Color(1f - Mathf.Pow(scale, 20), scale, 0f, 0.3f);
                            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width * scale, EditorGUIUtility.singleLineHeight + 0), color);
                        }
                        // EditorGUILayout.LabelField($"��� {dse.Considerations[consideration.considerationId].ToString().PadRight(50)}", $"{consideration.score.ToString("0.00")} ▸ {consideration.evalScore.ToString("0.0")} => {consideration.finalScore.ToString("0.00")}".PadLeft(30), EditorStyles.label, GUILayout.ExpandWidth(true));
                        // EditorGUILayout.LabelField($"Score: {dec2consid.Score.ToString("0.00")}    Bonus: { dec2consid.Bonus.ToString("0.00")}    Min: { (detailsList.Count !=0 ? detailsList[0].min.ToString("0.00") : null )}", (GUIStyle)"PR DisabledPrefabLabel");
                    }
                }
            }


            GUILayout.EndVertical();

            // foreach (var item in ai.DSE2Id) {
            //     HashSet<Entity> res;
            //     ai.DSEDecisions.TryGetValue(item.Key, out res);
            //     EditorGUILayout.LabelField($"{decisionList[item.Key]} => x{(res?.Count ?? -1)} ({(item.Value!=-1 ? ai.IdCounts[item.Value]: -1)})");
            // }

            /*var dbgHistoryLast = ai.DebugConsiderationList;

               var list = new List<DebugDecision>(ai.DebugDecisionList);
               if (sortDecisions) list.Sort((a, b) => - a.Score.CompareTo(b.Score));

               var maxScore = Mathf.Max(1.0f, list.Count != 0 ? list.Max(x => x.Score) : 0.0f);

               // var targetList = AIManager.Instance.GetSignals();

               foreach (var dec2consid in list) {
                if (dec2consid.DSEId == 0) continue;
                var isPositive = dec2consid.Score >= 0f;

                if (hideNegative && !isPositive) continue;

                GUI.color = isPositive ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                EditorGUILayout.LabelField($"[{decisionList[dec2consid.DSEId]}] #{dec2consid.Target.Index}         {(dec2consid.Score*100f).ToString("F1")}%", (GUIStyle)"ProjectBrowserHeaderBgMiddle");
                GUI.color = Color.white;

                if (isPositive) {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    float scale = (dec2consid.Score / maxScore);
                    Color color = new Color(1f - Mathf.Pow(scale, 20), scale, 0f, 0.5f);
                    EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width * scale, EditorGUIUtility.singleLineHeight + 2), color);
                }

                if (showDetails) {
                    var targetId = dec2consid.Target;
                    // Signal target;
                    //    targetList.TryGetValue(targetId, out target);
                    //    if (targetId != Entity.Null && (target == null || !target.Active)) {
                    //     EditorGUILayout.LabelField($"Target is not active {targetId}");
                    //     continue; // See if this causes any issues, are we getting rid of innactive signals?????
                    //    }

                    var hash = dec2consid.GetHashCode();
                    var detailsList = dbgHistoryLast.FindAll(x => x.history.GetHashCode().Equals(hash));

                    EditorGUILayout.LabelField($"Score: {dec2consid.Score.ToString("0.00")}    Bonus: { dec2consid.Bonus.ToString("0.00")}    Min: { (detailsList.Count !=0 ? detailsList[0].min.ToString("0.00") : null )}", (GUIStyle)"PR DisabledPrefabLabel");

                    var dse = decisionList[dec2consid.DSEId];

                    foreach (var dbgHist in detailsList) {
                        EditorGUILayout.LabelField($"▷ {dse.Considerations[dbgHist.considerationId].ToString().PadRight(50)}", $"{dbgHist.score.ToString("0.00")} ▸ {dbgHist.evalScore.ToString("0.0")} => {dbgHist.finalScore.ToString("0.00")}".PadLeft(30), EditorStyles.label, GUILayout.ExpandWidth(true));

                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            rect.width -= 150;
                            float scale = (dbgHist.finalScore / maxScore);
                            Color color = new Color(1f - Mathf.Pow(scale, 20), scale, 0f, 0.3f);
                            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width * scale, EditorGUIUtility.singleLineHeight + 0), color);
                        }
                    }
                }
               }*/

            // -----------------------------------------------------------------------------------------------
            // DrawSideHeader("Prefered Decisions", ref toggleBools[4]);

            /*if (toggleBools[4]) {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                var tempSortedPrefferedList = ai.DecisionsPreferrence.data.ToList();
                tempSortedPrefferedList.Sort((a, b) => - a.value.CompareTo(b.value));

                foreach (var dec in tempSortedPrefferedList) {
                    EditorGUILayout.LabelField($"❤ [{decisionList[dec.element.GetDSEId()]}] {dec.element.GetSignalId()}  -  Preference: {dec.value.ToString("F3")}");
                }
                GUILayout.EndVertical();
               }*/
            // -----------------------------------------------------------------------------------------------
            // DrawSideHeader("Failed Decisions", ref toggleBools[5]);

            /*if (toggleBools[5]) {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                var failList = ai.DecisionsFailed.data.GetList();
                foreach (var dec in failList) {
                    EditorGUILayout.LabelField($"‼ [{decisionList[dec.element.GetDSEId()]}] {dec.element.GetSignalId()}  -  Time Left: {(dec.value - ai.Time).ToString("F3")}s");
                }
                GUILayout.EndVertical();
               }*/
#else
            EditorGUILayout.HelpBox("NO_DEBUG flag is set", MessageType.Error);
#endif
        }

        // -----------------------------------------------------------------------------------------------
        // DrawHeaderToggle("Decision Targets", ref toggleBools[4]);

        // if (toggleBools[4]) {
        //     var bufferInternal = AIManager.Instance.mgr.GetBuffer<AddNewTargets>(ai.GetEntity());

        //     for (int i = 0; i < bufferInternal.Length; i++) {
        //         EditorGUILayout.LabelField($"{i}.  {bufferInternal[i].sender} {(DecisionTags)bufferInternal[i].decisionTags} {bufferInternal[i].decisionFlags}");
        //     }
        // }
        // -----------------------------------------------------------------------------------------------
        DrawHeaderToggle("Decision Internal", ref toggleBools[5]);

        if (toggleBools[5]) {
            var bufferInternal = AIManager.Instance.mgr.GetBuffer<DecisionInternal>(ai.GetEntity());

            for (int i = 0; i < bufferInternal.Length; i++) {
                EditorGUILayout.LabelField($"{i}.  {decisionList[bufferInternal[i].dseId]} {bufferInternal[i].target} = {bufferInternal[i].decisionEntity}");
            }
        }
        // -----------------------------------------------------------------------------------------------
        DrawHeaderToggle("Mindsets", ref toggleBools[7]);

        if (toggleBools[7]) {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var mindset in ai.Mindsets) {
                EditorGUILayout.LabelField(mindset.Name);
            }
            GUILayout.EndVertical();
        }

        // -----------------------------------------------------------------------------------------------
        // DrawHeaderToggle("Memory", ref toggleBools[8]);

        /*if (toggleBools[8]) {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            ai.MemorySet.ForEachType((dec, signal) => {
                    EditorGUILayout.LabelField(dec.ToString());

                    EditorGUILayout.SelectableLabel(String.Join(", ", signal.ToList()), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                });
            GUILayout.EndVertical();
           }*/
        // -----------------------------------------------------------------------------------------------

        EditorGUILayout.EndScrollView();
        Drag(dragRect);

        Repaint();
    }

    // void LoopThroughStates (Interaction interaction, ActionManager.TransitionState transition, short stateIndex, List<ActionManager.SubStateInstance> currentStates, Dictionary<short,
    //     Dictionary<ActionManager.TransitionState,
    //     short> > childrenLookup, int depth = 0)
    // {
    //     var state = currentStates.Find(x => x.Node.Id.Equals(stateIndex));

    //     GUIStyle richtext = new GUIStyle { richText = true };
    //     var label = $"[{interaction.Meta.Nodes[stateIndex].Name}]";

    //     if (state.HasEnded) {
    //         label = $"<color=brown>{label}</color>";
    //     }else if (state.HasStarted) {
    //         label = $"<b>{label}</b>";
    //     }
    //     if (state.HasAddedNext) {
    //         label = $"{label} - NEXT";
    //     }

    //     GUILayout.BeginVertical(EditorStyles.helpBox);

    //     EditorGUILayout.LabelField($"{transition}: {label}{(state.RequestExitState?" - WANTS TO EXIT":"")}", richtext);

    //     Dictionary<ActionManager.TransitionState, short> children;
    //     if (childrenLookup.TryGetValue(stateIndex, out children)) {
    //         GUILayout.BeginHorizontal();
    //         foreach (var child in children) {
    //             LoopThroughStates(interaction, child.Key, child.Value, currentStates, childrenLookup, depth + 1);
    //         }
    //         GUILayout.EndHorizontal();
    //     }
    //     GUILayout.EndVertical();
    // }

    private void DrawLinearTimeline (Rect container, int maxHeight, DynamicBuffer<DecisionHistoryRecord> buff, float startTime, float endTime, Action<short, Entity, Rect> action, bool drawBg = true)
    {
        float maxWidth = container.width - 10;
        // float maxHeight = container.height;
        var beginX = container.x + 5;
        var beginY = container.y - maxHeight - 5;

        var time = selectedAgent.GetActionManager().time;
        var starTimeReal = time - startTime;
        var endTimeReal = time - endTime;

        var timeRange = (endTime - startTime);

        if (drawBg) EditorGUI.DrawRect(new Rect(beginX, beginY, maxWidth, maxHeight), Color.gray);
        int counter = 0;
        int inlineHeight = 0;
        int inlineHeightMax = 0;

        for (int i = 0; i < buff.Length; i++) {
            var item = buff[i];
            if (item.StartTime > starTimeReal && item.StartTime < endTimeReal) continue;

            var start = ((starTimeReal - item.EndTime) / timeRange) * maxWidth;
            var end = ((starTimeReal - item.StartTime) / timeRange) * maxWidth;

            if (counter != 0 && item.Duration == 0.0f) {
                end += 0.5f / timeRange * maxWidth;
                inlineHeight++;
                if (inlineHeight > inlineHeightMax) inlineHeightMax = inlineHeight;
            }else{
                inlineHeight = 0;
            }

            var rect = new Rect(start, beginY + inlineHeight * maxHeight,
                    (end - start), maxHeight);

            action.Invoke(item.dseId, item.target, rect);

            counter++;
        }

        GUILayout.Space(maxHeight + inlineHeightMax * maxHeight);

        /*foreach (var item in histEnum) {
            EditorGUILayout.LabelField($"{item.Reference}  - START: {item.StartTime}, END: {item.EndTime}");
           }*/
    }

    protected Vector2 m_PositionDelta;
    private Vector2 m_PositionLast;
    private Vector2 m_DragStart;
    private bool m_Dragging;

    public void Drag (Rect draggingRect)
    {
        EditorGUIUtility.AddCursorRect(draggingRect, MouseCursor.Pan);

        if (Event.current.rawType == EventType.MouseUp) {
            m_Dragging = false;
        }else if (Event.current.type == EventType.MouseDown && draggingRect.Contains(Event.current.mousePosition)) {
            m_Dragging = true;
            m_DragStart = Event.current.mousePosition;
            m_PositionLast = m_DragStart;
            Event.current.Use();
        }

        if (m_Dragging) {
            m_PositionDelta = Event.current.mousePosition - m_PositionLast;
            m_PositionLast = Event.current.mousePosition;
        }
    }

    private bool SetTimeRangeAndClamp (float newTimeStart, float newTimeEnd, float width)
    {
        if ((newTimeStart >= 0f) && (newTimeStart) <= timelineMax && (newTimeEnd >= 0f) && (newTimeEnd) <= timelineMax && newTimeEnd != newTimeStart) {
            timelineStart = newTimeStart;
            timelineEnd = newTimeEnd;
            return true;
        }else if (newTimeStart < 0f) {
            timelineStart = 0;
            timelineEnd = width;
            return true;
        }else if (newTimeEnd > timelineMax) {
            timelineStart = timelineMax - width;
            timelineEnd = timelineMax;
            return true;
        }
        return false;
    }

    public static void DrawSplitter ()
    {
        var rect = GUILayoutUtility.GetRect(1f, 1f);

        // Splitter rect should be full-width
        rect.xMin = 0f;
        rect.width += 4f;

        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, !EditorGUIUtility.isProSkin
            ? new Color(0.6f, 0.6f, 0.6f, 1.333f)
            : new Color(0.12f, 0.12f, 0.12f, 1.333f));
    }

    public static bool DrawHeader (string title, bool state)
    {
        var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

        var labelRect = backgroundRect;

        labelRect.xMin += 16f;
        labelRect.xMax -= 20f;

        var foldoutRect = backgroundRect;
        foldoutRect.y += 1f;
        foldoutRect.width = 13f;
        foldoutRect.height = 13f;

        // Background rect should be full-width
        backgroundRect.xMin = 0f;
        backgroundRect.width += 4f;

        // Background
        float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
        EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

        // Title
        EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

        // Active checkbox
        state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

        var e = Event.current;
        if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0) {
            state = !state;
            e.Use();
        }

        return state;
    }

    private void DrawHeaderToggle (string title, ref bool value)
    {
        DrawSplitter();
        value = DrawHeader(title, value);
    }

    private void DrawSideHeader (string title, ref bool value)
    {
        value = GUILayout.Toggle(value, title, (GUIStyle)"Foldout", GUILayout.ExpandWidth(true));
    }
}