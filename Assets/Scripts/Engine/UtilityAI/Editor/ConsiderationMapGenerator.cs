using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UtilityAI
{
    public class ConsiderationMapGenerator : EditorWindow
    {
        const string considerationFile = "ConsiderationMap.cs";
        string considerationFilePath;
        List<FileTemp> allConsiderations = new List<FileTemp>(10);

        struct FileTemp
        {
            public string fullname;
            public string name;
            public string name2;
            public ParametersType parameters;
            public bool cachable;
            public int order;
        }

        [MenuItem("Tools/AI/Consideration Map Compiler")]
        public static void Init ()
        {
            // Get existing open window or if none, make a new one:
            ConsiderationMapGenerator window = (ConsiderationMapGenerator)EditorWindow.GetWindow(typeof(ConsiderationMapGenerator), true, "Consideration Map Compiler");

            window.Show();
        }

        private void OnEnable ()
        {
            // GenerateJob();

            considerationFilePath = new DirectoryInfo(Application.dataPath).GetFiles(considerationFile, SearchOption.AllDirectories).SingleOrDefault()?.FullName;

            var methods = System.Reflection.Assembly
                .GetAssembly(typeof(ConsiderationAttribute))
                .GetTypes()
                .Where(x => x.IsPublic && x.Name.EndsWith("Considerations"))
                .SelectMany(x => x.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                .Where(x => x.GetCustomAttributes(typeof(ConsiderationAttribute), true).Length == 1);

            allConsiderations.Clear();
            foreach (var item in methods) {
                var ca = (ConsiderationAttribute)item.GetCustomAttributes(true).FirstOrDefault();
                allConsiderations.Add(new FileTemp {
                        order = ca.Order,
                        fullname = item.DeclaringType.FullName,
                        name = item.Name,
                        name2 = ca.Name,
                        parameters = ca.Parameters,
                        cachable = ca.Cache
                    });
            }
            allConsiderations.Sort(delegate(FileTemp x, FileTemp y) {
                    return x.order.CompareTo(y.order);
                });
        }

        void OnGUI ()
        {
            GUILayout.Label("Consideration Map Compiler", EditorStyles.boldLabel);
            GUILayout.Label(considerationFilePath);


            if (considerationFilePath == null) {
                GUILayout.Label($"Could not find the {considerationFile} file");
                return;
            }

            if (GUILayout.Button("Recompile")) {
                File.WriteAllText(considerationFilePath, Create(allConsiderations));
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
            // foreach (var tt in AssetDatabase.FindAssets("ConsiderationMap.cs"))
            // UnityEditor.FileUtil.GetProjectRelativePath("/").GetFiles("*.*", SearchOption.AllDirectories);

            // System.Reflection.Assembly.GetAssembly(typeof(ConsiderationMap)).


            foreach (var item in allConsiderations) {
                GUILayout.Label(item.name2 + $"        ({item.fullname})");
            }
            /*foreach (var item in Enum.GetNames(typeof(ConsiderationMap.Types))) {
                GUILayout.Label(item);
               }*/
        }

        string Create (List<FileTemp> considerations)
        {
            var ret = "// Automatically generated, do not modify by hand\n\n";

            ret += "using System;namespace UtilityAI{\npublic static class ConsiderationMap {\n";

            ret += "public enum Types : byte{\n";
            foreach (var item in considerations) ret += item.name2 + ",\n";
            ret += "}\n\n";


            foreach (var item in considerations) ret += $"public static ConsiderationScoreDelegate {item.name2} = {item.fullname}.{item.name};\n";
            ret += "\n\n";

            ret += "public static bool IsCachable(Types name){\n";
            if (considerations.Where(x => x.cachable).Count() != 0) {
                ret += "switch (name){\n";
                foreach (var item in considerations) if (item.cachable) ret += "case Types." + item.name2 + ": return true;\n";
                ret += "}\n";
            }
            ret += "return false;\n}\n\n";

            ret += "public static ConsiderationScoreDelegate Get(Types name){\nswitch (name){\n";
            foreach (var item in considerations) ret += "case Types." + item.name2 + ": return " + item.name2 + ";\n";
            ret += "}\nreturn null;\n}\n\n";

            ret += "public static ParametersType GetParametersType(Types name){\nswitch (name){\n";
            foreach (var item in considerations) ret += "case Types." + item.name2 + ": return ParametersType." + item.parameters.ToString().Replace(", ", " | ParametersType.") + ";\n";
            ret += "}\nreturn " + nameof(ParametersType) + "." + nameof(ParametersType.None) + ";\n}\n";
            ret += "}}";
            return ret;
        }

        private Dictionary <ConsiderationMap.Types, object[]> typeToDataTypes;
        // private Dictionary <short, object[]> decisionToConsiderationData;

        void GenerateJob ()
        {
            // Setup all consideration to used data list
            var methods = System.Reflection.Assembly
                .GetAssembly(typeof(ConsiderationAttribute))
                .GetTypes()
                .SelectMany(x => x.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                .Where(x => x.GetCustomAttributes(typeof(ConsiderationAttribute), true).Length == 1)
                .Select(y => new {
                        name = ((ConsiderationAttribute)y.GetCustomAttributes(true).FirstOrDefault()).Name,
                        all = y.GetCustomAttributes(typeof(ConsiderationDataAttribute), false)
                    });

            typeToDataTypes = new Dictionary<ConsiderationMap.Types, object[]>();

            foreach (var item in methods) {
                var dataType = (ConsiderationMap.Types)Enum.Parse(typeof(ConsiderationMap.Types), item.name);
                // typeToDataTypes.Add(dataType, item.all);
                Debug.Log(dataType);
                Debug.Log("all: " + string.Join(", ", item.all));
                foreach (var attr in item.all) {
                    Debug.Log("data: " + (attr.GetType() == typeof(ConsiderationDataAttribute)));
                    // Debug.Log("sharedData: " + (attr.GetType() == typeof(ConsiderationSharedDataAttribute)));
                    // Debug.Log("targetData: " + (attr.GetType() == typeof(ConsiderationDataFromTargetAttribute)));
                    // Debug.Log("targetSharedData: " + (attr.GetType() == typeof(ConsiderationSharedDataFromTargetAttribute)));
                }
            }
        }

        static void Createjj ()
        {
            /*GameObject selected = Selection.activeObject as GameObject;
               if (selected == null || selected.name.Length & lt;= 0 )
               {
                Debug.Log("Selected object not Valid");
                return;
               }

               // remove whitespace and minus
               string name = selected.name.Replace(" ", "_");
               name = name.Replace("-", "_");
               string copyPath = "Assets/" + name + ".cs";
               Debug.Log("Creating Classfile: " + copyPath);
               if (File.Exists(copyPath) == false) { // do not overwrite
                using (StreamWriter outfile =
                    new StreamWriter(copyPath)) {
                    outfile.WriteLine("using UnityEngine;");
                    outfile.WriteLine("using System.Collections;");
                    outfile.WriteLine("");
                    outfile.WriteLine("public class " + name + " : MonoBehaviour {");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" // Use this for initialization");
                    outfile.WriteLine(" void Start () {");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" }");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" // Update is called once per frame");
                    outfile.WriteLine(" void Update () {");
                    outfile.WriteLine(" ");
                    outfile.WriteLine(" }");
                    outfile.WriteLine("}");
                }//File written
               }
               AssetDatabase.Refresh();
               selected.AddComponent(Type.GetType(name));*/
        }

        public static void Register ()
        {
            /*if (Initialized) return;
               Initialized = true;

               Debug.Log("REGISTER");

               var methods = typeof(Defaults)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.GetCustomAttributes(typeof(ConsiderationAttribute), true).Length == 1);

               foreach (var item in methods) {
                var ca = ((ConsiderationAttribute)item.GetCustomAttributes(true)[0]);
                Debug.Log(item.Name + " -> " + ca.Name + " : " + ca.Parameters);
               }*/
        }
    }
}