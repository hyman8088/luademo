using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;

public class Texture2DExportEditor
{
	public static readonly string[] MaterialNames = {
		"Sprites","UIDefault","CloudMesh","MapMesh"
	};

	[MenuItem("Assets/Export Texture2D Of SpriteAtlas", false, 2)]
	private static void ExportSpriteAtlasTexture2D()
	{
		var ids = Selection.instanceIDs;
		foreach (var item in ids)
		{
			string assetPath = AssetDatabase.GetAssetPath(item);
			if (Path.GetExtension(assetPath) == ".spriteatlas")
			{
				string fileName = Path.GetFileNameWithoutExtension(assetPath);
				var texName = fileName.TrimEnd('1', '2', '3', '4');
				for (int i = 1; i <= 4; i++)
				{
					string atlasPath = assetPath.Replace(fileName, texName + i.ToString());
					var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
					if (spriteAtlas)
					{
						string path = GeneratePngFromSpriteAtlas(spriteAtlas);
						Texture2DExportEditor.ExportSpriteAtlas(spriteAtlas, i);
						SplitTexture(path, i);
						SpriteAtlasAssetManager.Instance.ClearData();
						EditorUtility.DisplayProgressBar("导出图鉴", spriteAtlas.name, i / 4.0F);
					}
					else break;
				}
				//给材质球设置贴图纹理
				SetMaterialSphere(texName);
				Debug.Log("<color=green>导出图集完成</color>");
				break;
			}
		}
		EditorUtility.ClearProgressBar();

		if (Application.isPlaying)
		{
			Hukiry.Mesh.MeshUVRefreshEditor.instance.RefreshMesh();
		}
		else
		{
			Debug.LogError("Please on playing to refresh all of mesh !");
		}
	}
	private static void SetMaterialSphere(string texName)
	{
		void SetTexture(Material matt, string textureName,int index)
		{
			string texNameLocal = textureName + index;
			var tex = Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>(texNameLocal);
			if (tex && matt)
			{
				matt.SetTexture($"_MainTex{index}", tex);
			}
		}
		int len = MaterialNames.Length;
		for (int i = 0; i < len; i++)
		{
			Material mat = Hukiry.HukiryToolEditor.GetAssetObject<Material>(MaterialNames[i]);
            for (int k = 1; k <= 4; k++)
            {
				SetTexture(mat, texName , k);
			}
		}
		AssetDatabase.SaveAssets();
	}

	/// <summary>
	/// 对应贴图分割
	/// </summary>
	/// <param name="assetPath"></param>
	/// <param name="indexTex"></param>
	private static void SplitTexture(string assetPath, int indexTex)
	{
		AssetDatabase.Refresh();
		TextureImporter importer = TextureImporter.GetAtPath(assetPath) as TextureImporter;
		
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Multiple;
		importer.wrapMode = TextureWrapMode.Repeat;
        var settings = new TextureImporterPlatformSettings();
		settings.overridden = true;
#if UNITY_EDITOR
		settings.name = "Android";
		settings.format = TextureImporterFormat.ETC2_RGBA8;
#else
		settings.name = "iOS";
		settings.format = TextureImporterFormat.ASTC_RGBA_5x5;
#endif

		settings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        settings.textureCompression = TextureImporterCompression.Compressed;
		settings.compressionQuality = 50;
		settings.allowsAlphaSplitting = false;
		importer.SetPlatformTextureSettings(settings);
        SpriteAtlasAsset spriteAtlasAsset = SpriteAtlasAssetManager.Instance.GetSpriteAtlasInfo("Texture");
		List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
		for (int index = 0; index < spriteAtlasAsset.spriteDatas.Count; ++index)
		{
			SpriteData data = spriteAtlasAsset.spriteDatas[index];
			if (indexTex == data.index)
			{
				SpriteMetaData spriteMetaData = new SpriteMetaData();
				spriteMetaData.name = data.spriteName;
				spriteMetaData.rect = data.GetRect();
				spriteMetaData.pivot = new Vector2(0.5f, 0.5f);
				spritesheet.Add(spriteMetaData);
			}
		}

		importer.spritesheet = spritesheet.ToArray();
		EditorUtility.SetDirty(importer);
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
	}

	/// <summary>
	/// 生成图片
	/// </summary>
	/// <param name="spriteAtlas"></param>
	/// <returns></returns>
	private static string GeneratePngFromSpriteAtlas(SpriteAtlas spriteAtlas)
	{
		string texturePath = Path.ChangeExtension(AssetDatabase.GetAssetPath(spriteAtlas), ".png");
		if (spriteAtlas == null)
			return null;

		Texture2D[] tempTexture = AccessPackedTextureEditor(spriteAtlas);
		if (tempTexture == null)
			return null;

		byte[] bytes = null;
		for (int i = 0; i < tempTexture.Length; i++)
		{
			try
			{
				bytes = tempTexture[i].EncodeToPNG();
			}
			catch (Exception)
			{
				// handled below
			}
			if (bytes == null || bytes.Length == 0)
			{
				Debug.LogError("不能读取压缩过的（SpriteAtlas）图集，需要启动可读可写并在Inspector里确保压缩设置成 None"+"Could not read Compressed SpriteAtlas. Please enable 'Read/Write Enabled' and ensure 'Compression' is set to 'None' in Inspector."+i+spriteAtlas.ToString());
				continue;
			}
			if (i > 0)
			{
				File.WriteAllBytes(texturePath.Replace(".png", i + ".png"), bytes);
			}
			else
			{
				File.WriteAllBytes(texturePath, bytes);
			}
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return texturePath;
	}
	private static Texture2D[] AccessPackedTextureEditor(SpriteAtlas spriteAtlas)
	{
		SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);
		Type T = Type.GetType("UnityEditor.U2D.SpriteAtlasExtensions,UnityEditor");
		MethodInfo GetPreviewTexturesMethod = T.GetMethod("GetPreviewTextures", BindingFlags.NonPublic | BindingFlags.Static);
		if (GetPreviewTexturesMethod != null)
		{
			object retval = GetPreviewTexturesMethod.Invoke(null, new object[] { spriteAtlas });
			var textures = retval as Texture2D[];
			if (textures.Length > 0)
				return textures;
		}
		return null;
	}

	//保存图集中精灵所有的数据
	private static void ExportSpriteAtlas(SpriteAtlas spriteAtlas,  int index = 1)
    {
        if (EditorApplication.isPlaying)
        {
            string texturePath = Path.ChangeExtension(AssetDatabase.GetAssetPath(spriteAtlas).Replace(index.ToString(),""), ".asset");
            Sprite[] sprites = GetSprites(spriteAtlas);
            SpriteAtlasAsset spriteAtlasAsset = AssetDatabase.LoadAssetAtPath<SpriteAtlasAsset>(texturePath);
			bool isNotExit = spriteAtlasAsset == null;

			Dictionary<string, SpriteData> dicData = new Dictionary<string, SpriteData>();
			if(spriteAtlasAsset) dicData = spriteAtlasAsset.spriteDatas.ToDictionary(p => p.spriteName);
			if (isNotExit) spriteAtlasAsset = new SpriteAtlasAsset();
			spriteAtlasAsset.spriteAtlasName = spriteAtlas.name.TrimEnd('1','2','3','4');
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                SpriteData spriteData = new SpriteData();
                spriteData.spriteName = sprite.name.Replace("(Clone)","");
				spriteData.x = sprite.textureRect.x;
				spriteData.y = sprite.textureRect.y;
				spriteData.height = sprite.textureRect.height;
                spriteData.width = sprite.textureRect.width;
				spriteData.innerUV = UnityEngine.Sprites.DataUtility.GetInnerUV(sprite);
				spriteData.SetUVs(UnityEngine.Sprites.DataUtility.GetOuterUV(sprite));
				spriteData.index = index;
				dicData[spriteData.spriteName] = spriteData;
            }
			spriteAtlasAsset.spriteDatas = dicData.Values.ToList();
			if (isNotExit)
				UnityEditor.AssetDatabase.CreateAsset(spriteAtlasAsset, texturePath);
			else
				UnityEditor.EditorUtility.SetDirty(spriteAtlasAsset);
		}
    }

    private static Sprite[] GetSprites(SpriteAtlas spriteAtlas)
    {
        Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(sprites);
        return sprites;
    }
}