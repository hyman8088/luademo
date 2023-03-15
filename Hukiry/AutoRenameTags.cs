using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AutoRenameTags
{
	static AutoRenameTags()
	{
		//AddTag();
		//AddLayer();
		//AddSortingLayer();
	}

	#region 添加标签
	static void AddTag(string tag)
	{
		if (!isHasTag(tag))
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "tags")
				{
					int index = it.arraySize;
					it.InsertArrayElementAtIndex(index);
					SerializedProperty dataPoint = it.GetArrayElementAtIndex(index);
					dataPoint.stringValue = tag;
					tagManager.ApplyModifiedProperties();
				}
			}
		}
	}
	static void AddLayer(string layer)
	{
		if (!isHasLayer(layer))
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name.StartsWith("layers"))
				{
					for (int i = 0; i < it.arraySize; i++)
					{
						if (i == 3 || i == 6 || i == 7) continue;
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						if (string.IsNullOrEmpty(dataPoint.stringValue))
						{
							dataPoint.stringValue = layer;
							tagManager.ApplyModifiedProperties();
							return;
						}
					}
				}
			}
		}
	}
	static void AddSortingLayer(string sortingLayer)
	{
		if (!isHasSortingLayer(sortingLayer))
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "m_SortingLayers")
				{
					int index = it.arraySize;
					it.InsertArrayElementAtIndex(index);
					SerializedProperty dataPoint = it.GetArrayElementAtIndex(index);
					while (dataPoint.NextVisible(true))
					{
						if (dataPoint.name == "name")
						{
							dataPoint.stringValue = sortingLayer;
							tagManager.ApplyModifiedProperties();
							return;
						}
					}
				}
			}
		}
	}
	#endregion
	#region MenuItem
	//[MenuItem("Assets/Hukiry/Add/Tags")]
	static void AddTag()
	{
		AddTag(TagLayer.Untagged);
		AddTag(TagLayer.UI_CSharp);
		for (int i = 0; i < TagLayer.UI_Control.Count; i++)
		{
			AddTag(TagLayer.UI_Control[i]);
		}

		
		AddTag(TagLayer.UI_Lua);
		AddTag(TagLayer.UI_LuaTemplate);
		AddTag(TagLayer.UI_LuaView);
		AddTag(TagLayer.UI_LuaPanel);
	}

	//[MenuItem("Assets/Hukiry/Add/Layers")]
	static void AddLayer()
	{
		for (int i = 0; i < CameraRenderLayers.RenderLayers.Length; i++)
		{
			AddLayer(CameraRenderLayers.RenderLayers[i]);
		}
	}

	//[MenuItem("Assets/Hukiry/Add/SortingLayers")]
	static void AddSortingLayer()
	{
		for (int i = 0; i < ObjectSortingLayers.SortingLayers.Count; i++)
		{
			Hukiry.HukiryLayer.AddSortingLayer(ObjectSortingLayers.SortingLayers[i]);
		}
	}


	//[MenuItem("Assets/Hukiry/Clear/Tags")]
	static void ClearTags()
	{
		SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty it = tagManager.GetIterator();
		while (it.NextVisible(true))
		{
			if (it.name == "tags")
			{
				it.ClearArray();
				tagManager.ApplyModifiedProperties();
				return;
			}
		}
	}




	//[MenuItem("Assets/Hukiry/Clear/SortingLayers")]
	static void ClearSortingLayers()
	{
		SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty it = tagManager.GetIterator();
		while (it.NextVisible(true))
		{
			if (it.name == "m_SortingLayers")
			{
				it.ClearArray();
				tagManager.ApplyModifiedProperties();
				return;
			}
		}
	}

	//[MenuItem("Assets/Hukiry/Clear/Layers")]
	static void ClearLayers()
	{
		SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty it = tagManager.GetIterator();
		while (it.NextVisible(true))
		{
			if (it.name == "layers")
			{
				for (int i = 0; i < it.arraySize; i++)
				{
					if (i == 3 || i == 6 || i == 7) continue;
					SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
					dataPoint.stringValue = string.Empty;
				}
				tagManager.ApplyModifiedProperties();
				return;
			}
		}
	}

	#endregion
	static bool isHasTag(string tag)
	{
		for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
		{

			if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))
				return true;
		}

		return false;
	}

	static bool isHasLayer(string layer)
	{
		for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
		{
			if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
				return true;
		}
		return false;
	}
	static bool isHasSortingLayer(string sortingLayer)
	{
		SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty it = tagManager.GetIterator();
		while (it.NextVisible(true))
		{
			if (it.name == "m_SortingLayers")
			{
				for (int i = 0; i < it.arraySize; i++)
				{
					SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
					while (dataPoint.NextVisible(true))
					{
						if (dataPoint.name == "name")
						{
							if (dataPoint.stringValue == sortingLayer) return true;
						}
					}
				}
			}
		}

		return false;
	}

}