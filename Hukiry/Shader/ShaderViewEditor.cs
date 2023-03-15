using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.Rendering;
namespace UnityEditor
{
	public class ShaderMeshEditor : ShaderGUI
	{
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor, properties);

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("保存设置"))
				{
					float r = properties.First(p => p.name == "_Value1").floatValue;
					float g = properties.First(p => p.name == "_Value2").floatValue;
					Log("materal Name=" , materialEditor.target.name , ",r:" , r , ",g:" , g);
					var gridEditor = GameObject.FindObjectOfType<GridEdit>();
					if (gridEditor)
					{
						if (materialEditor.target.name == "Gray2")
						{
							gridEditor.grayColor2 = new Color(r, g, 1, 1);
						}
						else
						{
							gridEditor.grayColor1 = new Color(r, g, 1, 1);
						}

						EditorUtility.SetDirty(gridEditor);
						AssetDatabase.SaveAssets();
					}
					else
					{
						if (EditorUtility.DisplayDialog("系统提示！", "您当前不在编辑场景，是否要切换到编辑场景？", "确认", "取消"))
						{
							ShowMenu();
						}
					}
				}
				if (GUILayout.Button("切换场景"))
				{
					ShowMenu();
				}
			}
		}

		public static void ShowMenu()
		{
			var scenes = Directory.GetFiles("Assets/Scenes", "*.unity");
			GenericMenu genericMenu = new GenericMenu();
			foreach (var item in scenes)
			{
				string fileName = Path.GetFileNameWithoutExtension(item);
				genericMenu.AddItem(new GUIContent(fileName), false, OnClickToggle, $"Assets/Scenes/{fileName}.unity");
			}
			genericMenu.ShowAsContext();
		}

		private void Log(params object[] objs)
		{
			Func<string> action = () => {
				string s = "";
				for (int i = 0; i < objs.Length; i++)
				{
					s += objs[i].ToString();
				}
				return s;
			};

			Logger.Log($"<color=green>{action()}</color>");
		}

        private static void OnClickToggle(object userData)
        {
			string path = (string)userData;
			EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
		}
    }

	public class ShaderViewEditor : ShaderGUI
	{
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor, properties);

			Hukiry.HukiryToolEditor.DrawLine(Hukiry.UI.SpritePreview.START_COLOR, Hukiry.UI.SpritePreview.END__COLOR);
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("重置灰度值"))
				{
					properties.First(p => p.name == "_R").floatValue = 0.3f;
					properties.First(p => p.name == "_G").floatValue = 0.59f;
					properties.First(p => p.name == "_B").floatValue = 0.11f;
					properties.First(p => p.name == "_IsGray").floatValue = 1;
				}

				if (GUILayout.Button("重置饱和度"))
				{
					properties.First(p => p.name == "_R").floatValue = 0.3f;
					properties.First(p => p.name == "_G").floatValue = 0.59f;
					properties.First(p => p.name == "_B").floatValue = 0.11f;
					properties.First(p => p.name == "_IsGray").floatValue = 1;
					properties.First(p => p.name == "_saturation").floatValue = 0.1F;
					properties.First(p => p.name == "_brightness").floatValue = 1F;
				}
			}
		}
	}

	public class ShaderMeshTextureEditor : ShaderGUI
	{
		private static readonly string[] texNames ={ "一 ", "二 ", "四 ", "三 " };
		private const string btnName = "重"+ "新"+ "加"+"载";
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor,  properties);
			return;
			int texIndex = 0;
			bool isEnableButton = true;
			using (new EditorGUILayout.VerticalScope())
			{
				for (int i = 0; i < properties.Length; i++)
				{
					MaterialProperty prop = properties[i];
					if ((prop.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None)
					{
						if (prop.type == MaterialProperty.PropType.Texture)
						{
							if (prop.textureValue) isEnableButton = false;
							if (texIndex % 2 == 0) EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();
							using (new EditorGUILayout.VerticalScope())
							{
								string label = "贴图" + texNames[texIndex];
								string tip = prop.textureValue == null ? "未设置，如启动，需要添加纹理" : "已经设置";
								EditorGUILayout.LabelField(new GUIContent(label, $"当前{label}{tip}。"), GUILayout.Width(100));
								EditorGUILayout.Space();
								prop.textureValue = (Texture2D)EditorGUILayout.ObjectField(prop.textureValue, typeof(Texture2D), true, GUILayout.Height(100), GUILayout.Width(100));
								GUILayout.Space(5);
							}
							if (texIndex % 2 == 1) EditorGUILayout.EndHorizontal();
							texIndex++;
						}
						else
						{
                            if (prop.type == MaterialProperty.PropType.Float && prop.displayName.Contains("IsEnable"))
                            {
                                prop.floatValue = EditorGUILayout.Toggle(new GUIContent("绘制精灵", "如果启动，那么就绘制物品；否则绘制网格（mesh）渲染。"), prop.floatValue == 1) ? 1 : 0;
                            }
                            else
                            {
                                materialEditor.DefaultShaderProperty(prop, prop.displayName);
                            }
                        }
					}
				}
			}

			EditorGUILayout.Space();
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();

            if (isEnableButton&&GUILayout.Button(btnName))
			{
				if (texIndex > 0)
				{
					for (int i = 1; i <= texIndex; i++)
					{
						SetTexture(materialEditor.target as Material, "Texture", i);
					}
				}
			}
		}

		static void SetTexture(Material matt, string textureName, int index)
		{
			string texNameLocal = textureName + index;
			var tex = Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>(texNameLocal);
			if (tex && matt)
			{
				matt.SetTexture($"_MainTex{index}", tex);
			}
		}
	}

	public class ShaderCloudEditor : ShaderGUI
	{
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			base.OnGUI(materialEditor, properties);

			if (GUILayout.Button("Save Cloud Data"))
			{
				List<CloudMaterial> cmList = new List<CloudMaterial>();
				foreach (var item in properties)
				{
					switch (item.type)
					{
						case MaterialProperty.PropType.Color:
							cmList.Add(new CloudMaterial()
							{
								key = item.name,
								value = ColorToString(item.colorValue),
								type = item.type
							});
							break;
						case MaterialProperty.PropType.Vector:
							cmList.Add(new CloudMaterial()
							{
								key = item.name,
								value = Vector3ToString(item.vectorValue),
								type = item.type
							});
							break;
						case MaterialProperty.PropType.Float:
						case MaterialProperty.PropType.Range:
							cmList.Add(new CloudMaterial()
							{
								key = item.name,
								value = item.floatValue,
								type = item.type
							});
							break;
						case MaterialProperty.PropType.Texture:
							var id=item.textureValue.GetInstanceID();
							string assetPath=AssetDatabase.GetAssetPath(id);
							string valueName = assetPath.Substring(assetPath.LastIndexOf('/') + 1).Replace('-', '_');
							cmList.Add(new CloudMaterial()
							{
								key = item.name,
								value = valueName.Split('.')[0],
								type = item.type
							});
							break;
					}
				}


				//System.IO.File.WriteAllText(CommonPath.CloudMaterial, Json.Instance.ToJson(cmList), System.Text.Encoding.UTF8);
				AssetDatabase.Refresh();
			}
		}

		string ColorToString(Color col)
		{
			string s = col.r + "|" + col.g + "|" + col.b + "|" + col.a;
			return s;
		}

		string Vector4ToString(Vector4 col)
		{
			string s = string.Format("{0:f3}", col.x) + "|" + string.Format("{0:f3}", col.y) + "|" + string.Format("{0:f3}", col.z) + "|" + string.Format("{0:f3}", col.w);
			return s;
		}

		string Vector3ToString(Vector3 col)
		{
			string s = string.Format("{0:f3}", col.x) + "|" + string.Format("{0:f3}", col.y) + "|" + string.Format("{0:f3}", col.z);
			return s;
		}

		public class CloudMaterial
		{
			public string key;
			public object value;
			public MaterialProperty.PropType type;
		}
	}
}
