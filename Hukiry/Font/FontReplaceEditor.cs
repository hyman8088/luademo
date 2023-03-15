using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FontReplaceEditor : MonoBehaviour
{
	[MenuItem("Assets/Replace Font Of Prefab", false, 3)]
	private static void ExportSpriteAtlasTexture2D()
	{
		var ids = Selection.instanceIDs;
		int length = ids.Length;
		int i = 0;
		foreach (var item in ids)
		{
			i++;
			string assetPath = AssetDatabase.GetAssetPath(item);
			if (Path.GetExtension(assetPath) != ".meta")
			{
				if (File.Exists(assetPath))
				{
					var meshPrefab = PrefabUtility.LoadPrefabContents(assetPath);
					if (meshPrefab)
					{
						Font font = Hukiry.HukiryToolEditor.GetAssetObject<Font>("simhei");
						Text[] allText = meshPrefab.GetComponentsInChildren<Text>(true);
						foreach (var uitxt in allText)
						{
							uitxt.font = font;
						}
						PrefabUtility.SaveAsPrefabAsset(meshPrefab, assetPath);
						Debug.Log(assetPath, AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
					}
				}
				else if (Directory.Exists(assetPath))
				{
					string[] files = Directory.GetFiles(assetPath, "*.prefab", SearchOption.AllDirectories);
					int len = files.Length;
                    for (int k = 0; k < len; k++)
                    {
						string pathUrl = files[k].Replace('\\', '/').Replace(Application.dataPath, "Assets");
						var meshPrefab = PrefabUtility.LoadPrefabContents(pathUrl);
						if (meshPrefab)
						{
							Font font = Hukiry.HukiryToolEditor.GetAssetObject<Font>("simhei"); //Resources.GetBuiltinResource<Font>("Arial.ttf"); 
							Text[] allText = meshPrefab.GetComponentsInChildren<Text>(true);
							if (allText != null && allText.Length > 0)
							{
								foreach (var uitxt in allText)
								{
									uitxt.font = font;
								}
								PrefabUtility.SaveAsPrefabAsset(meshPrefab, pathUrl);
								Debug.Log(pathUrl, AssetDatabase.LoadAssetAtPath<GameObject>(pathUrl));
							}
						}

						if (k % 3 == 0)
						{
							EditorUtility.DisplayProgressBar("字体替换中...", pathUrl, k / (float)len);
						}
					}
				}
			}
			EditorUtility.DisplayProgressBar("字体替换中...", assetPath, i / (float)length);
		}
		EditorUtility.ClearProgressBar();
		
	}

	[MenuItem("GameObject/Replace Font Of Prefab", false, 3)]
	private static void SceneReplaceFont()
	{
		var tf = Selection.activeTransform;
		Font font = Hukiry.HukiryToolEditor.GetAssetObject<Font>("simhei"); //Resources.GetBuiltinResource<Font>("Arial.ttf"); //
		Text[] allText = tf.GetComponentsInChildren<Text>(true);
		foreach (var uitxt in allText)
		{
			uitxt.font = font;
		}
		AssetDatabase.SaveAssets();
	}
}
