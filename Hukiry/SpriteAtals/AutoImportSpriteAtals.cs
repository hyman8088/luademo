using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Linq;

public class AutoImportSpriteAtals : AssetPostprocessor
{

    private static List<string> ablist = new List<string>()
    {
       "lua/common",
       "lua/config",
       "lua/data",
       "lua/game",
       "lua/logic",
       "lua/m_d",
       "lua/panel",
       "lua/pb",
       "lua/tolua",
       "lua/uiv",
       "lua/util"
    };
    //所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAsset != null && importedAsset.Length > 0)
        {
            foreach (var item in importedAsset)
            {
                AutoMarkLua(item);
            }
        }
    }

    public static void AutoMarkLua(string assetPath)
    {
        string startHead = "Assets/tempStreamingAssets/lua";
        if (assetPath.StartsWith(startHead))
        {
            string shortPath = assetPath.Replace(startHead + "/", "");
            string abName = null;
            if (shortPath.StartsWith("tolua"))
            {
                abName = "lua/tolua" + AppConst.ExtName;
            }
            else
            {
                foreach (var item in ablist)
                {
                    if (shortPath.StartsWith(item))
                    {
                        abName = item + AppConst.ExtName;
                        break;
                    }
                }
            }
            if (abName != null && Path.GetExtension(assetPath) == ".bytes")
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                importer.assetBundleName = abName;
            }
        }
    }

#if UNITY_IOS || UNITY_IPHONE
    [MenuItem("GameObject/资源处理/iOS 重新导入SpriteAtlas", false, -10)]
#elif UNITY_WEBGL
    [MenuItem("GameObject/资源处理/WebGL 重新导入SpriteAtlas", false, -10)]
#else
    [MenuItem("工具/资源处理/Android 重新导入SpriteAtlas", false, -10)]

#endif
    public static void ImportSpriteAtlas()
    {
        List<string> linePath = Hukiry.HukiryToolEditor.GetAssetsPath<SpriteAtlas>();
        StartImportAssets(linePath.ToArray());
    }

    [MenuItem("Assets/资源处理/WebGL 重新导入SpriteAtlas", false, -10)]
    public static void ImportAssetSpriteAtlas()
    {
        var guids = Selection.instanceIDs;
        int index = 0;
        foreach (var id in guids)
        {
            index++;
            string assetPath = AssetDatabase.GetAssetPath(id);
            if (Directory.Exists(assetPath))
            {
                string[] files = Directory.GetFiles(assetPath, "*.spriteatlas", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string path = files[i].Replace('\\', '/').Replace(Application.dataPath, "Assets");
                    StartImportAssets(path);
                    EditorUtility.DisplayProgressBar("重新导入SpriteAtlas", path, i / (float)files.Length);
                }
            }
            else
            {
                if (Path.GetExtension(assetPath) == ".spriteatlas")
                {
                    StartImportAssets(assetPath);
                }
                EditorUtility.DisplayProgressBar("重新导入SpriteAtlas", assetPath, index / (float)guids.Length);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/资源处理/WebGL 重新导入Texture2D", false, -10)]
    public static void ImportAssetTexture2D()
    {
        var guids = Selection.instanceIDs;
        int index = 0;
        foreach (var id in guids)
        {
            index++;
            string assetPath = AssetDatabase.GetAssetPath(id);
            if (Directory.Exists(assetPath))
            {
                string[] files = Directory.GetFiles(assetPath, "*.png", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string path = files[i].Replace('\\', '/').Replace(Application.dataPath, "Assets");
                    StartImportAssets(path);
                    EditorUtility.DisplayProgressBar("重新导入Texture2D", path, i / (float)files.Length);
                }
            }
            else
            {
                if (Path.GetExtension(assetPath) == ".png")
                {
                    StartImportAssets(assetPath);
                }
                EditorUtility.DisplayProgressBar("重新导入Texture2D", assetPath, index / (float)guids.Length);
            }
        }
        EditorUtility.ClearProgressBar();
    }

#if UNITY_IOS || UNITY_IPHONE
    [MenuItem("GameObject/资源处理/iOS 重新导入Texture2D", false, -10)]
#elif UNITY_WEBGL
    [MenuItem("GameObject/资源处理/WebGL 重新导入Texture2D", false, -10)]
#else
    [MenuItem("工具/资源处理/Android 重新导入Texture2D", false, -10)]
#endif
    public static void ImportTexture2D()
    {
        List<string> linePath = Hukiry.HukiryToolEditor.GetAssetsPath<Texture2D>();
        StartImportAssets(linePath.ToArray());
    }

#if UNITY_IOS || UNITY_IPHONE
    [MenuItem("GameObject/资源处理/iOS 重新导入Audio", false, -10)]

#else
    [MenuItem("工具/资源处理/Android 重新导入Audio", false, -10)]
#endif
    public static void ImportAudio()
    {
        List<string> linePath = Hukiry.HukiryToolEditor.GetAssetsPath<AudioClip>();
        StartImportAssets(linePath.ToArray());
        AssetDatabase.SaveAssets();
    }
#if UNITY_IOS || UNITY_IPHONE
    [MenuItem("GameObject/资源处理/iOS 一键资源打包", false, -10)]
#endif
    private static void IOSMarkResource()
    {
        PackMakerEditor.MarkAndPackageALLOfResource();
    }
    private static void StartImportAssets(params string[] importedAsset)
    {
        bool isProgressbar = true;
#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE || UNITY_WEBGL
        int len = importedAsset.Length;
        int index = 0;
        foreach (string str in importedAsset)
        {
            index++;
            if (Path.GetExtension(str) == ".spriteatlas")
            {
                SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(str);
                if (str.Contains("tp")) continue;
                bool is1024 = IsThan1024(spriteAtlas, str);
#if UNITY_ANDROID
                TextureImporterPlatformSettings settings = spriteAtlas.GetPlatformSettings("Android");
                settings.format = TextureImporterFormat.ETC2_RGBA8;
#elif UNITY_IOS || UNITY_IPHONE
                TextureImporterPlatformSettings settings = spriteAtlas.GetPlatformSettings("iPhone");
                settings.format = TextureImporterFormat.ASTC_RGBA_5x5;
#elif UNITY_WEBGL
                TextureImporterPlatformSettings settings = spriteAtlas.GetPlatformSettings("WebGL");
                settings.format = TextureImporterFormat.ASTC_8x8;
#endif
                settings.overridden = true;
                settings.textureCompression = TextureImporterCompression.Compressed;
                settings.maxTextureSize = is1024 ? 1024 : 2048;
                spriteAtlas.SetPlatformSettings(settings);
                EditorUtility.SetDirty(spriteAtlas);
                SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);
            }

            else if (Path.GetExtension(str) == ".png")
            {
                TextureImporter importer = TextureImporter.GetAtPath(str) as TextureImporter;
                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
#if UNITY_ANDROID
                if (importer.textureType != TextureImporterType.Sprite) return;
                importer.name = "Android";
                importer.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
                platformSettings.format = TextureImporterFormat.ETC2_RGBA8;
#elif UNITY_IOS || UNITY_IPHONE
                importer.name = "iPhone";
                platformSettings.format =  TextureImporterFormat.ASTC_RGBA_6x6;
#elif UNITY_WEBGL
                importer.name = "WebGL";
                platformSettings.name = "WebGL";
                platformSettings.format = TextureImporterFormat.ASTC_8x8;
#endif
                importer.textureCompression = TextureImporterCompression.Compressed;
                platformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                platformSettings.overridden = true;
                importer.sRGBTexture = true;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.compressionQuality = 50;
                importer.SetPlatformTextureSettings(platformSettings);
                importer.SaveAndReimport();
            }
#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
            else if (Path.GetExtension(str) == ".wav" || Path.GetExtension(str) == ".mp3"|| Path.GetExtension(str) == ".ogg")
            {
                AudioImporter importer = AudioImporter.GetAtPath(str) as AudioImporter;
                AudioImporterSampleSettings sampleSettings = importer.defaultSampleSettings;
                sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                sampleSettings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                sampleSettings.quality = 0.3F;
                sampleSettings.conversionMode = 0;
#if UNITY_ANDROID
                string platform = "Android";
#elif UNITY_IOS || UNITY_IPHONE
                string platform = "iPhone";
#endif
                importer.name = platform;
                importer.forceToMono = false;
                importer.preloadAudioData = true;
                importer.loadInBackground = false;
                importer.ambisonic = false;
                importer.SetOverrideSampleSettings(platform, sampleSettings);
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
#endif

            if (isProgressbar)
            {
                EditorUtility.DisplayProgressBar("导入中", str, index / (float)len);
            }
        }

        if (isProgressbar)
        {
            EditorUtility.ClearProgressBar();
        }
#endif
    }

    private static bool IsThan1024(SpriteAtlas spriteAtlas, string str)
    {
        Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(sprites);
        List<string> temp = new List<string>();
        var all = sprites.Where(p =>
        {
            bool isOk = p.texture.width > 1024 || p.texture.height > 1024;
            var spriteName = p.name.Replace("(Clone)", "");
            if (isOk && !temp.Contains(spriteName))
            {
                temp.Add(spriteName);
                var objSprite = Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>(spriteName);
                Debug.Log($"此图集 {spriteAtlas.name} 需要提取出来的精灵： <color=yellow> {spriteName}  </color>", objSprite);
            }
            return isOk;
        });

        if (all.Count()>0)
        {
            Debug.Log("单个贴图宽或高大于1024*1024: <color=#f00>" + str + "</color>", spriteAtlas);
            Debug.Log($"----------------------------------------共计：{all.Count()}----------------------------------------------");
        }
        return all.Count() == 0;
    }
}


