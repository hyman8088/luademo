using Hukiry.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class CheckResourceWin : EditorWindow, IHasCustomMenu
{
    public enum CheckResourceType
    {
        其他类型, 图片, 材质, 预制件, 文件夹
    }

    CheckResourceType checkResource = CheckResourceType.其他类型;
    List<GameObject> m_gameObjects = null;
    private Object goObject;
    private List<string> dirGUIDList = null;
    private bool isFolder = true;
    private int fileNum = 0;
    private List<string> luaGuidList;

    [MenuItem("Assets/依赖检查窗口")]
    public static void ShowWindow()
    {
        CheckResourceWin resourceWin = GetWindow<CheckResourceWin>();
        resourceWin.ShowAuxWindow();
    }
    private void OnEnable()
    {
        isFolder = SeletctPickerConfig.Instance.isChechFolder;
        string[] dirsID = AssetDatabase.FindAssets("t:folder", SeletctPickerConfig.Instance.dirListPath.ToArray());
        dirGUIDList = dirsID.Where(p => !Path.GetExtension(AssetDatabase.GUIDToAssetPath(p)).EndsWith("meta")).ToList();
        if (goObject != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(goObject)))
            fileNum = Directory.GetFiles(AssetDatabase.GetAssetPath(goObject)).Where(p => !p.EndsWith(".meta")).Count();

        luaGuidList = AssetDatabase.FindAssets("t:defaultasset", new string[] { "Assets/LuaFw/Lua" }).
            Where(p => !AssetDatabase.GUIDToAssetPath(p).EndsWith(".meta")).
            Where(p => AssetDatabase.GUIDToAssetPath(p).EndsWith(".lua") && File.Exists(AssetDatabase.GUIDToAssetPath(p))).
            ToList();
    }
    private string GUIDToDirName(string guid) { return Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid)); }
    private void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                isFolder = EditorGUILayout.Toggle("使用文件夹查找", isFolder);
                SeletctPickerConfig.Instance.是否启动引用 = EditorGUILayout.Toggle("打印依赖文件", SeletctPickerConfig.Instance.是否启动引用);
            }
            SeletctPickerConfig.Instance.isChechFolder = isFolder;
            if (isFolder)
            {
                if (goObject && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(goObject)) == false) { goObject = null; }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField("", goObject, typeof(Object), true);
                    if (GUILayout.Button("选择文件夹"))
                    {
                        SeletctPickerView.ShowSeletctPicker(dirGUIDList, guid =>
                        {
                            goObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(guid));
                            fileNum = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(guid)).Where(p => !p.EndsWith(".meta")).Count();
                            this.Repaint();
                        });
                    }
                }
            }
            else
            {
                goObject = EditorGUILayout.ObjectField("检查依赖的对象", goObject, typeof(Object), true);
            }

            if (goObject != null)
            {
                if (goObject is GameObject)
                {
                    checkResource = CheckResourceType.预制件;
                }
                else if (goObject is Texture2D || goObject is Sprite)
                {
                    checkResource = CheckResourceType.图片;
                }
                else if (goObject is Material)
                {
                    checkResource = CheckResourceType.材质;
                }
                else if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(goObject)))
                {
                    checkResource = CheckResourceType.文件夹;
                    SeletctPickerConfig.Instance.selectFolderGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(goObject));
                }
                else
                {
                    checkResource = CheckResourceType.其他类型;
                }
            }

            if (!isFolder)
            {
                GUILayout.Label("查找对象的类型：   " + checkResource.ToString(), GUI.skin.textField);
            }
            else
            {
                GUILayout.Label("查找对象的类型：   " + checkResource.ToString() + ", 数量：" + fileNum, GUI.skin.textField);
            }

            if (GUILayout.Button("检查依赖"))
            {
                SeletctPickerConfig.Instance.lastSelectGuiId = goObject.GetInstanceID();
                Hukiry.HukiryToolEditor.ClearUnityConsole();
                if (goObject)
                {
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();
                    this.StartCheckFolder(checkResource);

                    stopwatch.Stop();
                    int TotalSeconds = (int)stopwatch.Elapsed.TotalSeconds;
                    Debug.Log($"查找使用时间：{TotalSeconds / 60}分{TotalSeconds % 60}秒");
                    EditorUtility.ClearProgressBar();
                }
                else
                    Debug.LogError("不能为空！");
            }
        }
    }

    private void StartCheckFolder(CheckResourceType checkResource)
    {
        List<string> outLine = new List<string>();
        List<string> unoutLine = new List<string>();
        string usingfileName = "输出文件依赖引用.txt";
        if (checkResource == CheckResourceType.文件夹)
        {
            string[] guidPng = AssetDatabase.FindAssets("t:Texture", new string[] { AssetDatabase.GetAssetPath(goObject) });
            guidPng = guidPng.Where(p => !AssetDatabase.GUIDToAssetPath(p).EndsWith(".meta")).ToArray();
            for (int i = 0; i < guidPng.Length; i++)
            {
                goObject = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guidPng[i]));
                checkResource = CheckResourceType.图片;
                this.StartCheckTexture(checkResource, ref outLine, ref unoutLine);
            }

            goObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(SeletctPickerConfig.Instance.selectFolderGUID));
        }
        else
            this.StartCheckTexture(checkResource, ref outLine, ref unoutLine);

        if (outLine.Count > 0 && SeletctPickerConfig.Instance.是否启动引用)
        {
            var temp = new List<string>();
            outLine.ToList().ForEach(p =>
            {
                var lines = p.Split('=');
                temp.Add((temp.Count + 1) + ", 图片: " + lines[0] + ", \t引用文件: " + lines[1] + "\n");
            });
            File.WriteAllLines(usingfileName, temp.ToArray());
        }
        else
        {
            usingfileName = "输出未引用的文件.txt";
            var temp = new List<string>();
            unoutLine.ToList().ForEach(p =>
            {
                temp.Add((temp.Count + 1) + ", " + p + "\n");
            });
            File.WriteAllLines(usingfileName, unoutLine.ToArray());
        }
        AssetDatabase.Refresh();
        Application.OpenURL(usingfileName);

    }
    private void StartCheckTexture(CheckResourceType checkResource, ref List<string> dic, ref List<string> Undic)
    {
        EditorUtility.DisplayProgressBar("准备中...", "", 0.05F);
        if (m_gameObjects == null || m_gameObjects.Count == 0)
        {
            m_gameObjects = Hukiry.HukiryToolEditor.GetAssetObjects<GameObject>(true);
        }

        int len = m_gameObjects.Count;
        bool isUsing = false;

        for (int i = 0; i < len; i++)
        {
            Transform[] roots = m_gameObjects[i].GetComponentsInChildren<Transform>(true);
            foreach (var item in roots)
            {
                string texture = null;
                texture = SearchComponent(checkResource, item, texture);
                if (texture == goObject.name)
                {
                    string assetPath = AssetDatabase.GetAssetPath(m_gameObjects[i]);
                    dic.Add(goObject.name + "=" + assetPath);
                    isUsing = true;
                    break;
                }
            }

            if (i % 30 == 0)
                EditorUtility.DisplayProgressBar($"查找中... {goObject.name}", m_gameObjects[i].name, i / (float)len);
        }

        EditorUtility.DisplayProgressBar($"Lua准备中...", goObject.name, 0.09f);
        int index = 0;
        foreach (var guid in luaGuidList)
        {
            index++;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var readText = File.ReadAllText(assetPath);
            string luaStr = "\"" + goObject.name + "\"";
            if (readText.IndexOf(goObject.name) >= 0 || readText.Contains(goObject.name) || readText.IndexOf(luaStr) >= 0 || readText.Contains(luaStr))
            {
                dic.Add(goObject.name + "=" + assetPath);
                isUsing = true;
            }

            foreach (var icon in SeletctPickerConfig.Instance.luaIconNameList)
            {
                if (readText.Contains(icon))
                {
                    dic.Add(goObject.name + "=" + assetPath);
                    isUsing = true;
                }
            }

            if (index % 30 == 0)
                EditorUtility.DisplayProgressBar($"Lua查找中... {goObject.name}", "lua文件搜索", index / (float)luaGuidList.Count);
        }

        if (SeletctPickerConfig.Instance.是否启动引用)
        {
            foreach (var item in dic)
            {
                string path = item.Split('=')[1];
                string fileName = Path.GetFileName(item.Split('=')[1]);
                Debug.Log($"<color=yellow>图片名：{goObject.name}= 依赖文件：{fileName} : {path}</color>", AssetDatabase.LoadAssetAtPath<Object>(path));
            }
        }

        if (dic.Count == 0 || isUsing == false)
        {
            this.ShowNotification(new GUIContent("未找到资源依赖"));
            if (!SeletctPickerConfig.Instance.是否启动引用)
            {
                var tex = goObject as Texture2D;
                Undic.Add($"图片名： {goObject.name}  , 图片Size=[ {tex.width} , {tex.height} ]， 预制件中 未引用：{AssetDatabase.GetAssetPath(goObject)}");
                Debug.Log($"<color=yellow>图片名：{goObject.name} ， 预制件中 未引用：{AssetDatabase.GetAssetPath(goObject)}，请查找lua代码和配置表中是否有引用</color>", AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(goObject)));
            }
        }
        else
        {
            this.ShowNotification(new GUIContent("存在依赖文件"));
        }
    }

    private string SearchComponent(CheckResourceType checkResource, Transform item, string texture)
    {
        try
        {
            if (checkResource == CheckResourceType.图片 || checkResource == CheckResourceType.其他类型)
            {
                texture = item?.gameObject?.GetComponent<AtlasImage>()?.spriteName;
                if (texture == null)
                    texture = item?.gameObject?.GetComponent<RawImage>()?.texture?.name;
                if (texture == null)
                    texture = item?.gameObject?.GetComponent<Image>()?.sprite?.name;
            }

            if (checkResource == CheckResourceType.预制件)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(item.gameObject) && item.name == goObject.name && !item.gameObject.Equals(goObject))
                {
                    texture = goObject.name;
                }
                goto PREFAB;
            }

            if (texture == null)
            {
                var ps = item?.gameObject?.GetComponent<ParticleSystem>();
                if (ps)
                    texture = ps?.shape.meshRenderer?.sharedMaterial?.mainTexture?.name;
            }

            if (texture == null)
                texture = item?.gameObject?.GetComponent<Renderer>()?.sharedMaterial?.mainTexture?.name;
        }
        catch { }
    PREFAB:
        return texture;
    }
    private void OnDisable() { m_gameObjects = null; }
    public void AddItemsToMenu(GenericMenu menu) { menu.AddItem(new GUIContent("Location Configuration"), false, () => { Hukiry.HukiryToolEditor.LocationObject<SeletctPickerConfig>("SeletctPickerConfig"); }); }
}
