using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;


public enum SeletctPickerType
{
    FolderPicker,
    Texture2D
}
public class SeletctPickerView : EditorWindow
{
    private static Action<string> callSelect;
    private static List<string> dirGUIDList = new List<string> ();
    private static SeletctPickerType m_seletctPicker;

    private static List<string> dirLastGUIDList=new List<string> ();

    private List<string> findGUIDList = new List<string>();
    private bool isDraw = false;
    private const int WIDTH = 100;
    private const int HEIGTH = 100;
    private const int SPACE = 10;
    private int cellNum, rowNum;
    private string mSearchText;
    private string selectGUID;

    public static void ShowSeletctPicker(List<string> dirGUIDListParam, Action<string> callSelectParam, SeletctPickerType seletctPickerType= SeletctPickerType.FolderPicker)
    {
        callSelect = callSelectParam;
        dirLastGUIDList = new List<string>(dirGUIDList);
        dirGUIDList = dirGUIDListParam;
        m_seletctPicker = seletctPickerType;
        var win = GetWindow<SeletctPickerView>();
        win.titleContent = new GUIContent("文件夹选择器", Hukiry.HukiryToolEditor.GetTexture2D("Folder Icon"));
        win.ShowUtility();
    }
    private void ChangeCaculate()
    {
        cellNum = Mathf.FloorToInt(position.width / WIDTH);
        cellNum -= Mathf.CeilToInt((cellNum + 1) * 10 / 100);
        rowNum = Mathf.CeilToInt(dirGUIDList.Count / cellNum);
    }

    private void OnEnable()
    {
        isDraw = dirGUIDList != null && dirGUIDList.Count > 0;
        if (isDraw)
        {
            findGUIDList = dirGUIDList;
            this.ChangeCaculate();
            mSearchText = SeletctPickerConfig.Instance.searchFolder;
            selectGUID = SeletctPickerConfig.Instance.selectFolderGUID;
            StartSearch(mSearchText);
        }
    }

    private string lastSearchText="";
    private Vector2 m_scrollPosition;
    private long delayTime = 0;
    private bool StartSearch(string SearchText)
    {
        if (lastSearchText != SearchText)
        {
            SeletctPickerConfig.Instance.searchFolder = SearchText;
            lastSearchText = SearchText.Trim();
            findGUIDList = dirGUIDList.Where(p => GUIDToDirName(p).ToLower().Contains(lastSearchText.ToLower())).ToList();
            return true;
        }

        if (string.IsNullOrEmpty(SearchText)&& findGUIDList.Count!= dirGUIDList.Count)
        {
            findGUIDList = dirGUIDList;
            return true;
        }
        return false;
    }
    private void OnGUI()
    {
        if (isDraw)
        {
            this.ChangeCaculate();
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(5f);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(WIDTH);
                    mSearchText = EditorGUILayout.TextField("", mSearchText, "SearchTextField");
                    if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                    {
                        mSearchText = "";
                        GUIUtility.keyboardControl = 0;
                    }

                    if (this.StartSearch(mSearchText))
                    {
                        this.Repaint();
                        return;
                    }

                    if (m_seletctPicker == SeletctPickerType.Texture2D)
                    {
                        GUILayout.Space(WIDTH/2);
                        if (GUILayout.Button("", "ArrowNavigationLeft", GUILayout.Width(WIDTH / 2)))
                        {
                            this.OnBack();
                        }
                    }
                    else
                    {
                        GUILayout.Space(WIDTH);
                    }
                }

                GUILayout.Space(SPACE);
                m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
                {

                    for (int i = 0; i < rowNum; i++)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(SPACE);
                            for (int j = 0; j < cellNum; j++)
                            {
                                int index = i * cellNum + j;
                                if (index < findGUIDList.Count)
                                {
                                    using (new GUILayout.VerticalScope())
                                    {
                                        bool isSelected = selectGUID == findGUIDList[index];
                                        if (isSelected) GUI.color = new Color(255, 255, 255, 255);

                                        var tex = m_seletctPicker == SeletctPickerType.Texture2D ? AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(findGUIDList[index])) : Hukiry.HukiryToolEditor.GetTexture2D("Folder Icon");
                                        if (GUILayout.Button(tex, isSelected? "flow node 3 on" : "flow node 0 on", GUILayout.Width(WIDTH), GUILayout.Height(HEIGTH)))
                                        {
                                            var contenttip = m_seletctPicker == SeletctPickerType.Texture2D ? "图片" : "目录";
                                            this.ShowNotification(new GUIContent($"您选择的{contenttip}：" + AssetDatabase.GUIDToAssetPath(findGUIDList[index])));
                                            callSelect?.Invoke(findGUIDList[index]);
                                            selectGUID = findGUIDList[index];
                                            SeletctPickerConfig.Instance.selectFolderGUID = selectGUID;
                                            var TotalMilliseconds = (long)System.DateTime.Now.TimeOfDay.TotalMilliseconds;
                                            if (m_seletctPicker != SeletctPickerType.Texture2D&&TotalMilliseconds - delayTime <= 210)
                                            {
                                                //显示下一级目录
                                                var select = AssetDatabase.FindAssets("t:texture", new string[] { AssetDatabase.GUIDToAssetPath(findGUIDList[index]) }).
                                                    Where(p=> !AssetDatabase.GUIDToAssetPath(p).EndsWith(".meta")).ToList();
                                                this.Close();
                                                SeletctPickerView.ShowSeletctPicker(select, null, SeletctPickerType.Texture2D);
                                                return;
                                            }
                                            delayTime = TotalMilliseconds;
                                        }
                                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                        GUILayout.Label(GUIDToDirName(findGUIDList[index]), GUILayout.Width(WIDTH), GUILayout.Height(20));
                                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                                        GUI.color = Color.white;
                                    }
                                }
                            }
                        }
                    }
                }GUILayout.EndScrollView();
            }
        }
        else
        {
            this.Close();
        }
    }
    private string GUIDToDirName(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        return path.Substring(path.LastIndexOf('/')+1);
    }

    private void OnBack()
    {
        if (m_seletctPicker == SeletctPickerType.Texture2D && dirLastGUIDList.Count > 0)
        {
            this.Close();
            SeletctPickerView.ShowSeletctPicker(dirLastGUIDList, callSelect, SeletctPickerType.FolderPicker);
        }
    }
}
