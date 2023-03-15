
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Hukiry.Editor
{
    [InitializeOnLoad]
    public class PackageAndroidAarEditor
    {
        static PackageAndroidAarEditor()
        {
#if USE_CNSDK
            if (!PackageConifgEditor.Instance.isCompiling)
            {
                var spAnim = GameObject.FindObjectOfType<SplashAnimation>();
                if (spAnim){ 
                    spAnim.enabled = true;
                    spAnim.SetEnable(PackageConifgEditor.Instance.isCompiling);
                }
            }
#else
            var spAnim = GameObject.FindObjectOfType<SplashAnimation>();
            if(spAnim)
            {
                spAnim.enabled = false;
                spAnim.SetEnable(false);
            }
#endif

            Hukiry.HukiryToolEditor.InvokeMethodStatic<PackageWindow>("SetSymbols");
        }
        // @"E:\Client\projectX\Android\ProjectX_Test_SDK\unity-android-resources\build.gradle";
        public static void GradlewBatBuild(string bundleVersion, int versionCode)
        {
            int versionCode1 = PackageConifgEditor.Instance.GetAppVersionCode();
            string buildPath = PackageConifgEditor.Instance.androidbuildgradle + "unity-android-resources/build.gradle";
            string outputFileNameARR = string.Empty;
            string workMode = PackageConifgEditor.Instance.workMode == WorkModeDef.Release ? PackageConifgEditor.Instance.workMode.ToString().ToLower() : "debug";
            if (PackageConifgEditor.Instance.isDelivery == false && PackageConifgEditor.Instance.workMode == WorkModeDef.Release)
            {
                Loger.Log($"<color=orange>正式包未使用分包</color>");
#if !USE_CNSDK
                versionCode1 = versionCode1 - 1;
#endif
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath.Replace("Assets", ""));
            string outAARShortPath = $"{workMode}/{directoryInfo.Name}-unity-{workMode}-{bundleVersion}.{versionCode1}";
            string outAARDirName = PackageConifgEditor.Instance.isUSE_CNSDK ? "arrCN" : "arr";
            string NewFileNameARR = $"                outputFileName = \"../../../{outAARDirName}/{outAARShortPath}.aar\""; 
            if (File.Exists(buildPath))
            {
                string[] lines = File.ReadAllLines(buildPath);
                foreach (var item in lines)
                {
                    string itemLine = item.Trim();
                    if (!string.IsNullOrEmpty(itemLine) && itemLine.StartsWith("outputFileName"))
                    {
                        outputFileNameARR = item;
                        break;
                    }
                }

                string text = File.ReadAllText(buildPath).Replace(outputFileNameARR, NewFileNameARR);
                File.WriteAllText(buildPath, text);
            }
            else
            {
                Loger.InfoErr("buildPath  路径不存在" + buildPath);
            }

            //查看task 命令帮助
            //gradlew tasks --all
            //输出aar
            //gradlew unity - android - resources:assembleDebug
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WorkingDirectory = PackageConifgEditor.Instance.androidbuildgradle;
            proc.StartInfo.FileName = "gradlew.bat";
            proc.StartInfo.Arguments = "unity-android-resources:assembleDebug";
            proc.Start();
            proc.WaitForExit();
            proc.Close();
            proc.Dispose();

            string outPath = PackageConifgEditor.Instance.androidbuildgradle + $"unity-android-resources/{outAARDirName}/{outAARShortPath}.aar";
            if (File.Exists(outPath))
            {
                Loger.Log($"打包上传路径：<color=yellow>{outPath}</color>");
                Application.OpenURL("https://gamecenter.jinkejoy.com/#/package/index");//打开打包系统出包
            }
        }

        /// <summary>
        /// 替换工程AndroidManifest包名
        /// </summary>
        /// <param name="gamePackageName"></param>
        //public static void ReplaceBuildgradlePackageName(string gamePackageName)
        //{
        //    if (PackageConifgEditor.Instance.isUSE_CNSDK && PackageConifgEditor.Instance.isUSE_SDK == false)
        //    {
        //        string readManifest = "JarTool/AndroidManifest.xml";
        //        if (!File.Exists(readManifest))
        //        {
        //            Debug.LogError("文件不存在，请更新工程！" + readManifest);
        //            return;
        //        }
        //        string text = File.ReadAllText(readManifest).Replace("com.jinke.mergeAnimal", gamePackageName);
        //        string writeManifest = PackageConifgEditor.Instance.androidbuildgradle + $"unity-android-resources/AndroidManifest.xml";
        //        File.WriteAllText(writeManifest, text);
        //        AssetDatabase.Refresh();
        //    }
        //}
    }
}
