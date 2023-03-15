using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Hukiry.UI;
using System;

namespace Hukiry.Prefab
{
    /*
     * 1,项目升级，替换预制件组件，只需要更换类型即可
     * 例如：Image -》AtlasImage
     */
    public class ReplacePrefabComponentEditor
    {
        [MenuItem("Assets/Replace Prefab To AtlasImage Component", false, -5)]
        public static void FixImage()
        {
            StartReplace(true);
        }

        [MenuItem("Assets/Replace Prefab To Image Component", false, -5)]
        public static void FixImage2()
        {
            StartReplace(true, true);
        }

        //[MenuItem("Assets/Replace Prefab Mask", false, -5)]
        public static void FixImageSprite()
        {
            StartReplace(false);
        }
        private static void StartReplace(bool isComponent,bool isImage=false)
        {
            var guids = Selection.instanceIDs;
            int index = 0;
            foreach (var id in guids)
            {
                index++;
                string assetPath = AssetDatabase.GetAssetPath(id);
                if (Directory.Exists(assetPath))
                {
                    string[] files = Directory.GetFiles(assetPath, "*.prefab", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string path = files[i].Replace('\\', '/').Replace(Application.dataPath, "Assets");
                        if (isImage)
                        {
                            ReplaceImageComponent(path);
                        }
                        else { 
                            if (isComponent)
                                ReplaceComponent(path);
                            else
                                ReplaceSpriteName(path);
                        }
                        EditorUtility.DisplayProgressBar("替换预制件组件", path, i / (float)files.Length);
                    }
                }
                else
                {
                    if (isImage)
                    {
                        ReplaceImageComponent(assetPath);
                    }
                    else
                    {
                        if (isComponent)
                            ReplaceComponent(assetPath);
                        else
                            ReplaceSpriteName(assetPath);
                    }
                    EditorUtility.DisplayProgressBar("替换预制件组件", assetPath, index / (float)guids.Length);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        private static void ReplaceSpriteName(string assetPath)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(assetPath);
            if (go)
            {
                Mask[] images = go.GetComponentsInChildren<Mask>(true);
                if (images != null && images.Length > 0)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        var img = images[i].GetComponent<AtlasImage>();
                        if (img&& img.spriteAtlas&& img.spriteAtlas.name == "Common")
                        {
                            images[i].enabled = false;
                            img.spriteName = "Common_miaosdk";
                            images[i].enabled = true;
                        }

                    }
                    PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                }
            }
        }

        private static void ReplaceImageComponent(string assetPath)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(assetPath);
            if (go)
            {
                AtlasImage[] images = go.GetComponentsInChildren<AtlasImage>(true);
                if (images != null && images.Length > 0)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (images[i].GetType().Name == typeof(Image).Name) continue;

                        var imgGo = images[i].gameObject;
                        int type = (int)images[i].type;
                        Color color = images[i].color;
                        bool isRaycast = images[i].raycastTarget;
                        Material material = images[i].material;
                        string spriteName = images[i].spriteName;
                        GameObject.DestroyImmediate(images[i]);

                        Image img = imgGo.AddComponent<Image>();
                        img.material = material;
                        img.raycastTarget = isRaycast;
                        img.color = color;
                        img.sprite = Hukiry.HukiryToolEditor.GetAssetObject<Sprite>(spriteName);
                        img.type = (Image.Type)type;
                        if (images[i].sprite == null)
                        {
                            img.color = new Color(1, 1, 1, 0);
                            continue;
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                }
            }
        }

        private static void ReplaceComponent(string assetPath)
        {
            GameObject go = PrefabUtility.LoadPrefabContents(assetPath);
            if (go)
            {
                Image[] images = go.GetComponentsInChildren<Image>(true);
                if (images != null && images.Length > 0)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        if (images[i].GetType().Name == typeof(AtlasImage).Name) continue;

                        var imgGo = images[i].gameObject;
                        int type = (int)images[i].type;
                        string spriteName = images[i].sprite?images[i].sprite.name: "";
                        Color color = images[i].color;
                        bool isRaycast = images[i].raycastTarget;
                        Material material = images[i].material;

                        GameObject.DestroyImmediate(images[i]);
   
                        AtlasImage img = imgGo.AddComponent<AtlasImage>();
                        img.material = material;
                        img.raycastTarget = isRaycast;
                        img.color = color;
                        img.spriteName = spriteName;
                        img.type = (Image.Type)type;
                        if (images[i].sprite == null)
                        {
                            img.color = new Color (1,1,1,0);
                            continue;
                        }
                        img.StartFindBySpriteName(images[i].sprite);
                    }
                    PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                }
            }
        }
    }
}