using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Hukiry.Mesh
{
    public class FreshMeshConfig
    {
        public const string PrefabDirPath = "Assets/ArtResources/map";
    }
    public class MeshUVRefreshEditor
    {
        public static MeshUVRefreshEditor instance { get; set; } = new MeshUVRefreshEditor();
        //批量刷新网格：仅纹理发生改变时
        public void RefreshMesh()
        {
            AssetDatabase.Refresh();

            string[] meshPrefabList = Directory.GetFiles(FreshMeshConfig.PrefabDirPath, "*.prefab", SearchOption.AllDirectories);
            if (meshPrefabList != null && meshPrefabList.Length > 0)
            {
                int length = meshPrefabList.Length;
                for (int i = 0; i < length; i++)
                {
                    string assetPath = meshPrefabList[i].Replace('\\', '/');
                    var meshPrefab = PrefabUtility.LoadPrefabContents(assetPath);
                    this.StartRepair(EMeshUVType.Grid, assetPath, meshPrefab.transform.Find("MapMesh"), meshPrefab.transform.Find("decoration"));
                    this.StartRepair(EMeshUVType.Cloud, assetPath, meshPrefab.transform.Find("CloudMesh"));
                    this.StartRepair(EMeshUVType.Sea, assetPath, meshPrefab.transform.Find("SeaMesh"));
                    this.StartRepair(EMeshUVType.ExpandMesh, assetPath, meshPrefab.transform.Find("ExpandMesh"));
                    PrefabUtility.SaveAsPrefabAsset(meshPrefab, assetPath);
                    EditorUtility.DisplayProgressBar("Fixing Mesh UV...", meshPrefab.name, i / (float)length);
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        //收集网格UV数据
        public void CollectUvData()
        {
            string[] meshPrefabList = Directory.GetFiles(FreshMeshConfig.PrefabDirPath, "*.prefab", SearchOption.AllDirectories);
            if (meshPrefabList != null && meshPrefabList.Length > 0)
            {
                int length = meshPrefabList.Length;
                for (int i = 0; i < length; i++)
                {
                    string assetPath = meshPrefabList[i].Replace('\\', '/');
                    var meshPrefab = PrefabUtility.LoadPrefabContents(assetPath);
                    this.StartCollectUvInformation(EMeshUVType.Grid, assetPath, meshPrefab.transform.Find("MapMesh"), meshPrefab.transform.Find("decoration"));
                    this.StartCollectUvInformation(EMeshUVType.Cloud, assetPath, meshPrefab.transform.Find("CloudMesh"));
                    this.StartCollectUvInformation(EMeshUVType.Sea, assetPath, meshPrefab.transform.Find("SeaMesh"));
                    this.StartCollectUvInformation(EMeshUVType.ExpandMesh, assetPath, meshPrefab.transform.Find("ExpandMesh"));
                    PrefabUtility.SaveAsPrefabAsset(meshPrefab, assetPath);
                    if (i % 2 == 0)
                    {
                        EditorUtility.DisplayProgressBar("Collecting Mesh ...", meshPrefab.name, i / (float)length);
                    }
                }
                Debug.Log($"收集数据完成，共收集{length}个！");
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 修改网格
        /// </summary>
        /// <param name="tf"></param>
        private void StartRepair(EMeshUVType eMeshUVType, string assetPath, Transform tf, Transform tfTwo=null)
        {
            if (tf)
            {
                var tex = Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>("Texture1");
                switch (eMeshUVType)
                {
                    case EMeshUVType.Grid:
                        tf.GetComponent<MeshRenderer>()?.sharedMaterial?.SetTexture("_MainTex", tex);
                        break;
                    case EMeshUVType.Decoration:
                        tf.GetComponent<MeshRenderer>().sharedMaterial?.SetTexture("_MainTex", tex);
                        break;
                    case EMeshUVType.Cloud:
                        tf.GetComponent<MeshRenderer>().sharedMaterial?.SetTexture("_MainTex", tex);
                        break;
                    case EMeshUVType.Sea:
                        if (tf.GetComponent<MeshRenderer>().sharedMaterial == null)
                            tf.GetComponent<MeshRenderer>().sharedMaterial = Hukiry.HukiryToolEditor.GetAssetObject<Material>("SeaMesh");
                        tf.GetComponent<MeshRenderer>().sharedMaterial?.SetTexture("_MainTex", Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>("Texture2"));
                        break;
                    case EMeshUVType.ExpandMesh:
                        if (tf.GetComponent<MeshRenderer>().sharedMaterial == null)
                            tf.GetComponent<MeshRenderer>().sharedMaterial = Hukiry.HukiryToolEditor.GetAssetObject<Material>("ExpandMesh");
                        tf.GetComponent<MeshRenderer>().sharedMaterial?.SetTexture("_MainTex", Hukiry.HukiryToolEditor.GetAssetObject<Texture2D>("Texture3"));
                        break;
                }
                var sharedMesh = tf.GetComponent<MeshFilter>()?.sharedMesh;
                MeshUVInformationMgr.instance.RefreshMeshUV(assetPath, sharedMesh, eMeshUVType);
                if (tfTwo)
                {
                    tfTwo.GetComponent<MeshRenderer>().sharedMaterial?.SetTexture("_MainTex", tex);
                    sharedMesh = tfTwo.GetComponent<MeshFilter>()?.sharedMesh;
                    MeshUVInformationMgr.instance.RefreshMeshUV(assetPath, sharedMesh, EMeshUVType.Decoration);
                }
            }
        }

        /// <summary>
        /// 开始收集数据
        /// </summary>
        /// <param name="tf"></param>
        private void StartCollectUvInformation(EMeshUVType eMeshUVType, string assetPath,Transform tf, Transform tfTwo = null)
        {
            if (tf)
            {
                var sharedMesh = tf.GetComponent<MeshFilter>()?.sharedMesh;
                MeshUVInformationMgr.instance.CollectUvInformation(assetPath, sharedMesh, eMeshUVType);
                if (tfTwo)
                {
                    sharedMesh = tfTwo.GetComponent<MeshFilter>()?.sharedMesh;
                    MeshUVInformationMgr.instance.CollectUvInformation(assetPath, sharedMesh, EMeshUVType.Decoration);
                }
            }
        }
    }
}
