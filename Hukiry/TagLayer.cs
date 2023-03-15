using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hukiry.RichText;
using Hukiry.UI;

/// <summary>
/// 标签层
/// </summary>
public class TagLayer
{
	public static string Untagged = "Untagged";
	public static string UI_CSharp = "UI_CSharp";

	//系统 和子模板
	public static string UI_Lua = "UI_LuaFull";
	public static string UI_LuaTemplate = "UI_LuaItem";

	/// <summary>
	/// 对话框和提示框
	/// </summary>
	public static string UI_LuaView = "UI_LuaView";
	public static string UI_LuaPanel = "UI_LuaPanel";

	public static List<string> UI_Control = new List<string>()
	{

		typeof(AtlasImage).Name,
		typeof(Text).Name,
		typeof(RichText).Name,
		typeof(GameObject).Name,
		typeof(Transform).Name,
		typeof(RawImage).Name,
		typeof(Image).Name,
		typeof(InputField).Name,
		//typeof(CanvasGroup).Name,
		typeof(Toggle).Name,
		typeof(Slider).Name,
	};
}

/// <summary>
/// 相机渲染剔除层
/// </summary>
public class CameraRenderLayers
{
	public static string[] RenderLayers = {
		"Item",
		"Npc",
		"Model"
    };
}


/// <summary>
/// 对象层级排序
/// </summary>
public class ObjectSortingLayers
{
	public static List<string> SortingLayers = new List<string>()
	{
		
		"Map",
		"Cloud",
		"Ballon"
	};
}



