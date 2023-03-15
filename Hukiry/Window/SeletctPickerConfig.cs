using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new SeletctPickerConfig", menuName ="Assets/Create/SeletctPickerConfig Assets")]
public class SeletctPickerConfig : CommonAssets<SeletctPickerConfig>
{
    public int lastSelectGuiId;
    public List<string> dirListPath;
    public bool isChechFolder;
    [Description("查找的字符串")]
    public string searchFolder;
    [Description("选择的文件夹ID")]
    public string selectFolderGUID;
    public bool 是否启动引用;
    [Header("代码中可能引用的图片集合")]
    public List<string> luaIconNameList;
}
