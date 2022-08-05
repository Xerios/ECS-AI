using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UtilityAI;

public class MindsetsEditorWindow : EditorWindow
{
    public int selectedDMPackageIndex = 0;
    public Mindset dmPackage = null;

    public List<string> DMPackageKeys = new List<string>();
    public List<Mindset> DMPackageValues = new List<Mindset>();

    Vector2 scrollPos;

    Color bgColor = new Color(0.9f, 0.9f, 0.9f);
    Color colorGreen = new Color(0.3f, 1f, 0.4f);
    Color colorGray = new Color(1f, 1f, 1f, 0.4f);

    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/AI/Decision Maker Package Editor")]
    static void Init ()
    {
        // Get existing open window or if none, make a new one:
        MindsetsEditorWindow window = (MindsetsEditorWindow)EditorWindow.GetWindow(typeof(MindsetsEditorWindow), true, "Decision Maker Package Editor");

        window.Show();
    }

    private void OnEnable ()
    {
        RefreshDMPackages();
    }

    private void RefreshDMPackages ()
    {
        DMPackageKeys.Clear();
        DMPackageValues.Clear();

        var fps = Resources.FindObjectsOfTypeAll(typeof(Mindset));
        foreach (Mindset decision in fps) {
            DMPackageKeys.Add(decision.Name);
            DMPackageValues.Add(decision);
        }

        selectedDMPackageIndex = Mathf.Min(DMPackageKeys.Count - 1, selectedDMPackageIndex);
    }
    void OnGUI ()
    {
        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal("MeBlendBackground");

        GUILayout.Label("Decision Maker Package Editor", (GUIStyle)"LODLevelNotifyText");

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Revert All", (GUIStyle)"ButtonLeft", GUILayout.Width(70), GUILayout.Height(30))) {
            GUI.SetNextControlName("");
            GUI.FocusControl("");
            RefreshDMPackages();
        }
        if (GUILayout.Button("Save", (GUIStyle)"ButtonRight", GUILayout.Width(150), GUILayout.Height(30))) {
            GUI.SetNextControlName("");
            GUI.FocusControl("");

            var dmPackage = DMPackageValues[selectedDMPackageIndex];
            AssetDatabase.ForceReserializeAssets(new[] { AssetDatabase.GetAssetPath(dmPackage.GetInstanceID()) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshDMPackages();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        SideBar();
        DecisionView();

        GUILayout.EndHorizontal();

        GUI.backgroundColor = Color.white;
    }

    private void DrawSideHeader (string title)
    {
        GUILayout.Label(title, (GUIStyle)"PreButton", GUILayout.ExpandWidth(true));
    }

    private void SideBar ()
    {
        GUILayout.BeginVertical(GUILayout.Width(200));


        DrawSideHeader("Packages");
        GUILayout.BeginVertical((GUIStyle)"HelpBox");
        selectedDMPackageIndex = GUILayout.SelectionGrid(selectedDMPackageIndex, DMPackageKeys.ToArray(), 1, (GUIStyle)"Tag MenuItem", GUILayout.ExpandWidth(true));
        dmPackage = DMPackageValues[selectedDMPackageIndex];
        GUILayout.EndVertical();

        GUILayout.Space(10);
        if (GUILayout.Button("+ New Package", (GUIStyle)"Button")) {
            var name = "UntitledDM";
            DMPackageKeys.Add(name);
            DMPackageValues.Add(Mindset.New(name));
            selectedDMPackageIndex = DMPackageKeys.Count - 1;
            dmPackage = DMPackageValues[selectedDMPackageIndex];
            AssetDatabase.CreateAsset(dmPackage, "Assets/Resources/Mindsets/" + dmPackage.Name + ".asset");
        }

        GUILayout.Space(40);

        DrawSideHeader("Property Map");
        // GUILayout.Box(String.Join(", ", Enum.GetNames(typeof(PropertyType))));
        if (GUILayout.Button("Open Property Map File", (GUIStyle)"minibutton")) {
            var filePath = new DirectoryInfo(Application.dataPath).GetFiles("PropertyMap.cs", SearchOption.AllDirectories).SingleOrDefault()?.FullName;
            Application.OpenURL("file://" + filePath);
        }
        GUILayout.Space(10);

        DrawSideHeader("Tag Map");
        // GUILayout.Box(String.Join(", ", Enum.GetNames(typeof(DecisionTags))));
        if (GUILayout.Button("Open Signal Tags File", (GUIStyle)"minibutton")) {
            var filePath = new DirectoryInfo(Application.dataPath).GetFiles("DecisionTagMap.cs", SearchOption.AllDirectories).SingleOrDefault()?.FullName;
            Application.OpenURL("file://" + filePath);
        }
        GUILayout.Space(10);


        DrawSideHeader("Consideration Map");
        // foreach (ConsiderationMap.Types considerationType in Enum.GetValues(typeof(ConsiderationMap.Types))) {
        //    GUILayout.Label(considerationType.ToString(), EditorStyles.centeredGreyMiniLabel);
        // }
        if (GUILayout.Button("Open Compiler", (GUIStyle)"minibutton")) ConsiderationMapGenerator.Init();


        GUILayout.EndVertical();
    }

    private void DecisionView ()
    {
        if (dmPackage == null) return;

        GUILayout.BeginVertical();

        GUILayout.BeginVertical();
        {
            GUILayout.Label("Decision Package", EditorStyles.centeredGreyMiniLabel);
            GUILayout.BeginHorizontal();
            dmPackage.Name = EditorGUILayout.DelayedTextField("Name:", dmPackage.Name);

            if (GUILayout.Button("+ Decision", GUILayout.Width(120))) {
                ArrayUtils.Add(ref dmPackage.DSEs, Decision.New("UntitledDecision"));
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        GUILayout.EndVertical();

        GUIStyle myStyle = new GUIStyle((GUIStyle)"AnimationKeyframeBackground") {
            padding = new RectOffset(0, 0, 5, 5),
        };

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, myStyle);

        GUILayout.BeginHorizontal();
        {
            for (int i = 0; i < dmPackage.DSEs.Length; i++) {
                Decision decision = dmPackage.DSEs[i];
                DrawDecision(decision, i, bgColor);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    public void DrawDecision (Decision decision, int index, Color bgColor)
    {
        GUI.backgroundColor = bgColor;

        GUIStyle myStyle = new GUIStyle((GUIStyle)"ShurikenModuleBg") {
            margin = new RectOffset(5, 5, 0, 5)
        };

        GUIStyle myStyleConsiderations = new GUIStyle((GUIStyle)"ShurikenModuleBg") {
            margin = new RectOffset(5, 5, 0, 0)
        };


        GUILayout.BeginVertical(GUILayout.MinWidth(250));
        GUILayout.BeginVertical(myStyle);

        var name = string.IsNullOrEmpty(decision.Name) ? "<Empty String>" : decision.Name;

        GUILayout.BeginHorizontal(new GUIContent(name), (GUIStyle)"ProjectBrowserHeaderBgTop");
        {
            if (GUILayout.Button("‹", EditorStyles.label, GUILayout.Width(17)) && index > 0) {
                ArrayUtils.Swap(dmPackage.DSEs, index, index - 1);
                this.Repaint();
                return;
            }

            if (GUILayout.Button("›", EditorStyles.label, GUILayout.Width(17)) && index < dmPackage.DSEs.Length - 1) {
                ArrayUtils.Swap(dmPackage.DSEs, index, index + 1);
                this.Repaint();
                return;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture("winbtn_win_close"), EditorStyles.label, GUILayout.Width(17)) &&
                EditorUtility.DisplayDialog("DELETE", string.Format("Are you sure you want to delete {0} ?", name), "DELETE", "Nope")) {
                ArrayUtils.RemoveAt(ref dmPackage.DSEs, index);
                this.Repaint();
                return;
            }
        }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        GUILayout.BeginHorizontal();
        {
            decision.Name = EditorGUILayout.DelayedTextField(decision.Name);
            decision.Weight = EditorGUILayout.DelayedFloatField(decision.Weight, GUILayout.Width(40));
        }
        GUILayout.EndHorizontal();

        decision.Tags = (uint)(DecisionTags)EditorGUILayout.EnumFlagsField((DecisionTags)decision.Tags);

        // var dict = StateScriptInteractionsHelper.GetMethods();
        // EditorGUILayout.Popup(0, dict.Keys.Prepend("-None-").ToArray());

        GUILayout.EndVertical();

        GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        GUILayout.BeginVertical(myStyleConsiderations);

        GUILayout.BeginVertical();
        {
            for (int j = 0; j < decision.Considerations.Length; j++) {
                DrawConsideration(decision, ref decision.Considerations[j], j);
            }

            GUI.backgroundColor = Color.white;
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add new consideration", (GUIStyle)"MiniPopup")) {
                    GenericMenu menu = new GenericMenu();
                    foreach (var item in Enum.GetNames(typeof(ConsiderationMap.Types))) {
                        menu.AddItem(new GUIContent(item), false, () => {
                                var newConsideration = ConsiderationParams.New((ConsiderationMap.Types)Enum.Parse(typeof(ConsiderationMap.Types), item));
                                ArrayUtils.Add(ref decision.Considerations, newConsideration);
                            });
                    }
                    menu.ShowAsContext();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();


        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    private void DrawConsideration (Decision decision, ref ConsiderationParams consideration, int index)
    {
        var propType = ConsiderationMap.GetParametersType(consideration.DataType);

        // var propS = consideration as SelfConsideration;

        GUI.backgroundColor = colorGreen;

        if ((propType & ParametersType.Range) == ParametersType.Range) GUI.backgroundColor = new Color(0.9f, 0.5f, 0.1f);
        else if ((propType & ParametersType.None) == ParametersType.None) GUI.backgroundColor = new Color(0.9f, 0.9f, 0.1f);


        GUIStyle myStyle = new GUIStyle((GUIStyle)"ChannelStripBg") {
            margin = new RectOffset(3, 3, 2, 5),
            padding = new RectOffset(1, 1, 1, 5)
        };

        GUIStyle myStyleBar = new GUIStyle();
        myStyleBar.alignment = TextAnchor.MiddleCenter;
        myStyleBar.normal.textColor = GUI.backgroundColor.ChangeBrightness(1f);

        GUILayout.BeginVertical(myStyle);
        {
            GUI.color = Color.white;
            GUILayout.BeginHorizontal(new GUIContent(consideration.DataType.ToString()), myStyleBar);
            {
                if (index > 0 && GUILayout.Button("▲", EditorStyles.label, GUILayout.Width(15))) {
                    ArrayUtils.Swap(decision.Considerations, index, index - 1);
                    this.Repaint();
                    return;
                }

                if (index < decision.Considerations.Length - 1 && GUILayout.Button("▼", EditorStyles.label, GUILayout.Width(15))) {
                    ArrayUtils.Swap(decision.Considerations, index, index + 1);
                    this.Repaint();
                    return;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(EditorGUIUtility.FindTexture("winbtn_win_close"), EditorStyles.label, GUILayout.Width(17)) &&
                    EditorUtility.DisplayDialog("DELETE", string.Format("Are you sure you want to delete {0} ?", consideration.DataType.ToString()), "DELETE", "Nope")) {
                    ArrayUtils.RemoveAt(ref decision.Considerations, index);
                    this.Repaint();
                    return;
                }
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            GUILayout.BeginVertical();

            EditorGUILayout.BeginVertical();
            if ((propType & ParametersType.Property) == ParametersType.Property) {
                consideration.Value.Property = (byte)(PropertyType)EditorGUILayout.EnumPopup((PropertyType)consideration.Value.Property);
            }

            if ((propType & ParametersType.Ability) == ParametersType.Ability) {
                consideration.Value.Property = (byte)(AbilityTags)EditorGUILayout.EnumPopup((AbilityTags)consideration.Value.Property);
            }

            if ((propType & ParametersType.Value) == ParametersType.Value) {
                EditorGUILayout.PrefixLabel("Value / Time ( seconds ) / Repeats:");
                consideration.Value.Min = EditorGUILayout.DelayedFloatField(consideration.Value.Min);
            }

            if ((propType & ParametersType.Range) == ParametersType.Range) {
                GUILayout.BeginHorizontal();
                consideration.Value.Min = EditorGUILayout.DelayedFloatField(consideration.Value.Min);
                consideration.Value.Max = EditorGUILayout.DelayedFloatField(consideration.Value.Max);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();

            if ((propType & ParametersType.Ability) == ParametersType.Ability) {
                // Show nothing
            }else if ((propType & ParametersType.Boolean) == ParametersType.Boolean) {
                var val = EditorGUILayout.Toggle("Is FALSE", consideration.Value.Min == 1f) ? 1f : 0f;
                if (consideration.Value.Min != val) {
                    consideration.UtilCurve = ConsiderationParams.GetBoolean(val == 0f);
                    consideration.Value.Min = val;
                }
            }else{
                EditorGUILayout.BeginVertical();
                var rect = new Rect(lastRect.x, lastRect.yMax + 2, lastRect.width, 50);
                EditorGUI.DrawRect(rect, Color.black);
                GUILayout.Space(50 + 2);

                consideration.UtilCurve.range = EditorGUILayout.Slider(consideration.UtilCurve.range, 0f, 1f);
                consideration.UtilCurve.offset = EditorGUILayout.Slider(consideration.UtilCurve.offset, 0f, 1f);
                consideration.UtilCurve.value0 = EditorGUILayout.Slider(consideration.UtilCurve.value0, 0f, 1f);
                consideration.UtilCurve.value1 = EditorGUILayout.Slider(consideration.UtilCurve.value1, 0f, 1f);
                GUILayout.BeginHorizontal();
                consideration.UtilCurve.tangent0 = EditorGUILayout.FloatField(consideration.UtilCurve.tangent0);
                consideration.UtilCurve.tangent1 = EditorGUILayout.FloatField(consideration.UtilCurve.tangent1);
                GUILayout.EndHorizontal();


                float yMin = 0;
                float yMax = 1;
                float step = 1f / rect.width;

                UnityEditor.Handles.color = GUI.backgroundColor;
                Vector2 prevPos = new Vector2(0, Mathf.Clamp01(consideration.UtilCurve.Evaluate(0)));

                for (float t = step; t < 1; t += step) {
                    Vector2 pos = new Vector2(t, Mathf.Clamp01(consideration.UtilCurve.Evaluate(t)));
                    UnityEditor.Handles.DrawLine(
                        new Vector2(rect.xMin + (prevPos.x) * rect.width, rect.yMax - ((prevPos.y - yMin) / (yMax - yMin)) * rect.height),
                        new Vector2(rect.xMin + (pos.x) * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height));
                    prevPos = pos;
                }

                EditorGUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
    }

    // void OnGUI ()
    // {
    //     EditorGUILayout.BeginHorizontal();
    //     {
    //         EditorGUILayout.BeginVertical();
    //         for (int i = 0; i < AllMindsets.Count; i++) {
    //             var item = AllMindsets[i];
    //             if (GUILayout.Button(item.name)) {
    //                 SelectedMindset = i;
    //             }
    //         }
    //         DrawMenu();
    //         EditorGUILayout.EndVertical();
    //     }
    //     {
    //         EditorGUILayout.BeginVertical();
    //         EditorGUILayout.LabelField("Hello");
    //         EditorGUILayout.EndVertical();
    //     }
    //     EditorGUILayout.EndHorizontal();
    // }

    // private void DrawMenu ()
    // {
    //     if (GUILayout.Button(new GUIContent("Open Property Map File"))) {
    //         var filePath = new DirectoryInfo(Application.dataPath).GetFiles("PropertyMap.cs", SearchOption.AllDirectories).SingleOrDefault()?.FullName;
    //         Application.OpenURL("file://" + filePath);
    //     }
    //     if (GUILayout.Button(new GUIContent("Open Signal Tags File"))) {
    //         var filePath = new DirectoryInfo(Application.dataPath).GetFiles("DecisionTagMap.cs", SearchOption.AllDirectories).SingleOrDefault()?.FullName;
    //         Application.OpenURL("file://" + filePath);
    //     }
    //     if (GUILayout.Button(new GUIContent("Open Compiler"))) ConsiderationMapGenerator.Init();

    //     // if (selected != null) {
    //     //     GUILayout.Label(selected.Name);
    //     // }

    //     /*if (GUILayout.Button(new GUIContent("SAVE")))
    //        {
    //         AssetDatabase.CreateAsset(dmPackage, "Assets/_Data/Mindsets/" + dmPackage.Name + ".asset");
    //         AssetDatabase.SaveAssets();
    //         AssetDatabase.Refresh();

    //         RefreshDMPackages();
    //        }*/
    // }

    // private void DrawSideHeader (string title)
    // {
    //     GUILayout.Label(title, (GUIStyle)"PreButton", GUILayout.ExpandWidth(true));
    // }

    // private void SideBar ()
    // {
    //     GUILayout.BeginVertical(GUILayout.Width(200));


    //     DrawSideHeader("Packages");
    //     GUILayout.BeginVertical((GUIStyle)"HelpBox");
    //     SelectedMindset = GUILayout.SelectionGrid(SelectedMindset, DMPackageKeys.ToArray(), 1, (GUIStyle)"Tag MenuItem", GUILayout.ExpandWidth(true));
    //     dmPackage = DMPackageValues[SelectedMindset];
    //     GUILayout.EndVertical();

    //     GUILayout.Space(10);
    //     if (GUILayout.Button("+ New Package", (GUIStyle)"Button")) {
    //         var name = "UntitledDM";
    //         DMPackageKeys.Add(name);
    //         DMPackageValues.Add(Mindset.New(name));
    //         SelectedMindset = DMPackageKeys.Count - 1;
    //         dmPackage = DMPackageValues[SelectedMindset];
    //     }

    //     GUILayout.Space(40);

    //     DrawSideHeader("Property Map");
    //     // GUILayout.Box(String.Join(", ", Enum.GetNames(typeof(PropertyType))));
    //     GUILayout.Space(10);

    //     DrawSideHeader("Tag Map");
    //     // GUILayout.Box(String.Join(", ", Enum.GetNames(typeof(DecisionTags))));
    //     GUILayout.Space(10);


    //     DrawSideHeader("Consideration Map");
    //     // foreach (ConsiderationMap.Types considerationType in Enum.GetValues(typeof(ConsiderationMap.Types))) {
    //     //    GUILayout.Label(considerationType.ToString(), EditorStyles.centeredGreyMiniLabel);
    //     // }


    //     GUILayout.EndVertical();
    // }

    // private void DecisionView ()
    // {
    //     if (dmPackage == null) return;

    //     GUILayout.BeginVertical();

    //     GUILayout.BeginVertical();
    //     {
    //         GUILayout.Label("Decision Package", EditorStyles.centeredGreyMiniLabel);
    //         GUILayout.BeginHorizontal();
    //         dmPackage.Name = EditorGUILayout.DelayedTextField("Name:", dmPackage.Name);

    //         if (GUILayout.Button("+ Decision", GUILayout.Width(120))) {
    //             ArrayUtils.Add(ref dmPackage.DSEs, Decision.New("UntitledDecision"));
    //         }
    //         GUILayout.EndHorizontal();
    //         EditorGUILayout.Space();
    //     }
    //     GUILayout.EndVertical();

    //     GUIStyle myStyle = new GUIStyle((GUIStyle)"AnimationKeyframeBackground"){
    //         padding = new RectOffset(0, 0, 5, 5),
    //     };

    //     scrollPos = EditorGUILayout.BeginScrollView(scrollPos, myStyle);

    //     GUILayout.BeginHorizontal();
    //     {
    //         for (int i = 0; i < dmPackage.DSEs.Length; i++) {
    //             Decision decision = dmPackage.DSEs[i];
    //             DrawDecision(decision, i, bgColor);
    //         }
    //     }
    //     GUILayout.EndHorizontal();

    //     GUILayout.FlexibleSpace();

    //     GUILayout.EndScrollView();
    //     GUILayout.EndVertical();
    // }

    // public void DrawDecision (Decision decision, int index, Color bgColor)
    // {
    //     GUI.backgroundColor = bgColor;

    //     GUIStyle myStyle = new GUIStyle((GUIStyle)"ShurikenModuleBg"){
    //         margin = new RectOffset(5, 5, 0, 5)
    //     };

    //     GUIStyle myStyleConsiderations = new GUIStyle((GUIStyle)"ShurikenModuleBg"){
    //         margin = new RectOffset(5, 5, 0, 0)
    //     };


    //     GUILayout.BeginVertical(GUILayout.MinWidth(250));
    //     GUILayout.BeginVertical(myStyle);

    //     var name = string.IsNullOrEmpty(decision.Name) ? "<Empty String>" : decision.Name;

    //     GUILayout.BeginHorizontal(new GUIContent(name), (GUIStyle)"ProjectBrowserHeaderBgTop");
    //     {
    //         if (GUILayout.Button("��", EditorStyles.label, GUILayout.Width(17)) && index > 0) {
    //             ArrayUtils.Swap(dmPackage.DSEs, index, index - 1);
    //             this.Repaint();
    //             return;
    //         }

    //         if (GUILayout.Button("›", EditorStyles.label, GUILayout.Width(17)) && index < dmPackage.DSEs.Length - 1) {
    //             ArrayUtils.Swap(dmPackage.DSEs, index, index + 1);
    //             this.Repaint();
    //             return;
    //         }

    //         GUILayout.FlexibleSpace();
    //         if (GUILayout.Button(EditorGUIUtility.FindTexture("winbtn_win_close"), EditorStyles.label, GUILayout.Width(17)) &&
    //             EditorUtility.DisplayDialog("DELETE", string.Format("Are you sure you want to delete {0} ?", name), "DELETE", "Nope")) {
    //             ArrayUtils.RemoveAt(ref dmPackage.DSEs, index);
    //             this.Repaint();
    //             return;
    //         }
    //     }
    //     GUILayout.EndHorizontal();
    //     GUI.backgroundColor = Color.white;
    //     GUILayout.BeginHorizontal();
    //     {
    //         decision.Name = EditorGUILayout.DelayedTextField(decision.Name);
    //         decision.Weight = EditorGUILayout.DelayedFloatField(decision.Weight, GUILayout.Width(40));
    //     }
    //     GUILayout.EndHorizontal();

    //     decision.Tags = (DecisionTags)EditorGUILayout.EnumFlagsField(decision.Tags);

    //     GUILayout.EndVertical();

    //     GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
    //     GUILayout.BeginVertical(myStyleConsiderations);

    //     GUILayout.BeginVertical();
    //     {
    //         for (int j = 0; j < decision.Considerations.Length; j++) {
    //             // DrawConsideration(decision, ref decision.Considerations[j], j);
    //         }

    //         GUI.backgroundColor = Color.white;
    //         GUILayout.BeginHorizontal();
    //         {
    //             GUILayout.FlexibleSpace();
    //             if (GUILayout.Button("Add new consideration", (GUIStyle)"MiniPopup")) {
    //                 GenericMenu menu = new GenericMenu();
    //                 foreach (var item in Enum.GetNames(typeof(ConsiderationMap.Types))) {
    //                     menu.AddItem(new GUIContent(item), false, () =>
    //                         {
    //                             var newConsideration = ConsiderationParams.New((ConsiderationMap.Types)Enum.Parse(typeof(ConsiderationMap.Types), item));
    //                             ArrayUtils.Add(ref decision.Considerations, newConsideration);
    //                         });
    //                 }
    //                 menu.ShowAsContext();
    //             }
    //         }
    //         GUILayout.EndHorizontal();
    //     }
    //     GUILayout.EndVertical();


    //     GUILayout.EndVertical();
    //     GUILayout.FlexibleSpace();
    //     GUILayout.EndVertical();
    // }

    // public class DecisionScoreEvaluatorDrawer : OdinValueDrawer<Decision[]>
    // {
    //     private Vector2 scrollPos;

    //     protected override void DrawPropertyLayout (GUIContent label)
    //     {
    //         var children = this.ValueEntry.Property.Children;

    //         Decision[] items = this.ValueEntry.SmartValue;

    //         GUIStyle myStyleConsiderations = new GUIStyle(){
    //             margin = new RectOffset(5, 5, 0, 0)
    //         };


    //         scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
    //         GUILayout.BeginHorizontal();

    //         for (int i = 0; i < children.Count; i++) {
    //             var item = children[i];
    //             var itemValue = item.ValueEntry.WeakSmartValue as Decision;
    //             EditorGUILayout.BeginVertical(myStyleConsiderations, GUILayout.MinWidth(250));
    //             GUIHelper.PushLabelWidth(100);

    //             var rect = EditorGUILayout.BeginHorizontal();
    //             EditorIcons.SpeechBubbleRound.Draw(GUILayoutUtility.GetRect(30, 30).Padding(10, 4));
    //             EditorGUILayout.BeginVertical();
    //             SirenixEditorGUI.Title(itemValue.Name, "Decision", TextAlignment.Left, false);
    //             EditorGUILayout.EndVertical();

    //             if (GUILayout.Button(EditorGUIUtility.FindTexture("winbtn_win_close"), EditorStyles.label, GUILayout.Width(17)) &&
    //                 EditorUtility.DisplayDialog("DELETE", string.Format("Are you sure you want to delete {0} ?", itemValue.Name), "DELETE", "Nope")) {
    //                 ArrayUtils.RemoveAt(ref items, i);
    //                 this.ValueEntry.SmartValue = items;
    //             }
    //             EditorGUILayout.EndHorizontal();

    //             item.Draw(null);


    //             var newValue = DragAndDropUtilities.DragAndDropZone(rect, itemValue, typeof(Decision), false, false);
    //             if (newValue != itemValue) {
    //                 var newValueIndex = Array.IndexOf(items, newValue);
    //                 items[i] = newValue as Decision;
    //                 items[newValueIndex] = itemValue as Decision;

    //                 this.ValueEntry.SmartValue = items;
    //             }

    //             GUIHelper.PopLabelWidth();
    //             EditorGUILayout.EndVertical();
    //         }

    //         if (GUILayout.Button("+ Decision", GUILayout.Width(120))) {
    //             ArrayUtils.Add(ref items, Decision.New("UntitledDecision"));
    //             this.ValueEntry.SmartValue = items;
    //         }

    //         GUILayout.EndHorizontal();
    //         EditorGUILayout.EndScrollView();
    //     }
    // }

    // public class ConsiderationParamsDrawer : OdinValueDrawer<ConsiderationParams>
    // {
    //     Color bgColor = new Color(0.9f, 0.9f, 0.9f);
    //     Color colorGreen = new Color(0.3f, 1f, 0.4f);
    //     Color colorGray = new Color(1f, 1f, 1f, 0.4f);

    //     protected override void DrawPropertyLayout (GUIContent label)
    //     {
    //         ConsiderationParams value = this.ValueEntry.SmartValue;

    //         var propType = ConsiderationMap.GetParametersType(value.DataType);


    //         GUI.backgroundColor = colorGreen;

    //         if ((propType & ParametersType.Range) == ParametersType.Range) GUI.backgroundColor = new Color(0.9f, 0.5f, 0.1f);
    //         else if ((propType & ParametersType.None) == ParametersType.None) GUI.backgroundColor = new Color(0.9f, 0.9f, 0.1f);

    //         SirenixEditorGUI.BeginBox();

    //         SirenixEditorGUI.Title(value.ToString(), "Consideration", TextAlignment.Left, false);

    //         if ((propType & ParametersType.Property) == ParametersType.Property) {
    //             // Property
    //             value.Value.Property = (UtilityAI.PropertyType)EditorGUILayout.EnumPopup("Property", value.Value.Property);
    //             // -------
    //             // }else if ((propType & ParametersType.AccessTag) == ParametersType.AccessTag) {
    //             // AccessTag
    //             // value.Value.AccessTags = (UtilityAI.AccessTags)EditorGUILayout.EnumFlagsField("Tags", value.Value.AccessTags);
    //             // -------
    //         }else if ((propType & ParametersType.Value) == ParametersType.Value) {
    //             // Value
    //             this.ValueEntry.Property.Children["Value"].Children["Min"].Draw(new GUIContent("Value / Time ( seconds ) / Repeats:"));
    //             // -------
    //         }else if ((propType & ParametersType.Range) == ParametersType.Range) {
    //             // Range
    //             GUILayout.BeginHorizontal();
    //             this.ValueEntry.Property.Children["Value"].Children["Min"].Draw();
    //             this.ValueEntry.Property.Children["Value"].Children["Max"].Draw();
    //             GUILayout.EndHorizontal();
    //             // -------
    //         }

    //         var lastRect = GUILayoutUtility.GetLastRect();

    //         if ((propType & ParametersType.Boolean) == ParametersType.Boolean) {
    //             var val = EditorGUILayout.Toggle("Is FALSE", value.Value.Min == 1f) ? 1f : 0f;
    //             if (value.Value.Min != val) {
    //                 value.UtilCurve = ConsiderationParams.GetBoolean(val == 0f);
    //                 value.Value.Min = val;
    //                 this.ValueEntry.SmartValue = value;
    //             }
    //         }else{
    //             EditorGUILayout.BeginVertical();
    //             var rect = new Rect(lastRect.x, lastRect.yMax + 2, lastRect.width, 50);
    //             EditorGUI.DrawRect(rect, Color.black);
    //             GUILayout.Space(50 + 2);

    //             this.ValueEntry.Property.Children["UtilCurve"].Draw(new GUIContent("Curve Settings"));
    //             /*value.UtilCurve.type = (Curve.CurveType)EditorGUILayout.EnumPopup(value.UtilCurve.type);

    //                GUILayout.BeginHorizontal();
    //                value.UtilCurve.m = EditorGUILayout.FloatField(value.UtilCurve.m);
    //                value.UtilCurve.k = EditorGUILayout.FloatField(value.UtilCurve.k);
    //                value.UtilCurve.b = EditorGUILayout.FloatField(value.UtilCurve.b);
    //                value.UtilCurve.c = EditorGUILayout.FloatField(value.UtilCurve.c);
    //                GUILayout.EndHorizontal();*/

    //             float yMin = 0;
    //             float yMax = 1;
    //             float step = 1f / rect.width;

    //             UnityEditor.Handles.color = GUI.backgroundColor;
    //             Vector2 prevPos = new Vector2(0, value.UtilCurve.Evaluate(0));

    //             for (float t = step; t < 1; t += step) {
    //                 Vector2 pos = new Vector2(t, Mathf.Clamp01(value.UtilCurve.Evaluate(t)));
    //                 UnityEditor.Handles.DrawLine(
    //                     new Vector2(rect.xMin + prevPos.x * rect.width, rect.yMax - ((prevPos.y - yMin) / (yMax - yMin)) * rect.height),
    //                     new Vector2(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height));
    //                 prevPos = pos;
    //             }

    //             EditorGUILayout.EndVertical();
    //         }

    //         // GUILayout.EndVertical();
    //         // GUILayout.EndHorizontal();
    //         SirenixEditorGUI.EndBox();


    //         GUI.backgroundColor = Color.white;
    //     }
    //     float curveFunc (float t)
    //     {
    //         return Mathf.Sin(t * 2 * Mathf.PI);
    //     }
    // }
}