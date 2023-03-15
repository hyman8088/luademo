using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Hukiry.Editor
{
    [System.Serializable]
    public class PackageConifgEditor : CommonAssets<PackageConifgEditor>
    {
        public const string USE_SDK = "USE_SDK";//使用SDK
        public const string USE_CNSDK = "USE_CNSDK";//国内SDK宏定义

        public string removeDirName = "";
        public bool isUSE_SDK = false;
        //切换国内环境SDK
        public bool isUSE_CNSDK = true;

        /// <summary>
        /// 出包版本
        /// </summary>
        public string bundleVersion = "1.0.0";

        /// <summary>
        /// 出包编号
        /// </summary>
        public int versionCode = 1;

        public int lastVersionCode = 1;
        /// <summary>
        /// 出包文件名
        /// </summary>
        public string dateTimePackage;
        /// <summary>
        /// 出包模式
        /// </summary>
        public WorkModeDef workMode = WorkModeDef.Debug;

        //仅 IL2CCPP 下显示
        public AndroidBuildType AndroidBuildType = AndroidBuildType.Release;
        public ScriptingBackend ScriptingBackend = ScriptingBackend.Mono;
        public PackageType PackageType = PackageType.apk;
        public BuildSystem buildSystem = BuildSystem.Internal;

        public bool isArmv7 = true;
        public bool isArm64 = true;
        public bool isX86 = false;

        /// <summary>
        /// 新包，增加打版本号，否则热更打包资源
        /// </summary>
        public bool isCompiling = false;

        public string _outPackageDirPath = "Android";

        /// <summary>
        /// 是否分包；分包aab
        /// </summary>
        public bool isDelivery = false;

        /// <summary>
        /// 是否使用cdn
        /// </summary>
        public bool isEnableCDN = true;
        /// <summary>
        /// 出审核包
        /// </summary>
        public bool isDebug = false;

        [SerializeField]
        public bool isChangePackage = false;

        /// <summary>
        /// 是否打包资源
        /// </summary>
        public bool isPackageResource = true;

        /// <summary>
        /// 是否开启打点
        /// </summary>
        public bool isOpenGuide = false;

        [SerializeField]
        private version_data version_Data;

        [Header("【资源版本号】")]
        [SerializeField]
        private version_data oldVersion_Data;
        [Header("【重置出包版本号】")]
        [SerializeField]
        private version_data lastversion_Data;

        private bool isUpdateVersion = false;

        public string androidbuildgradle =>
#if USE_CNSDK
            @"D:\Android_Project\ProjectX_Google_SDK\";
#else
            @"D:\Android_Project\ProjectX_Google_SDK_hw\";
#endif

        public bool isPreReleaseMode = false;

        [Header("包名选择部分")]
        [SerializeField]
        public List<ChannalData> androidPackageChannal = new List<ChannalData>();
        [SerializeField]
        public string selectPackageName =
#if USE_CNSDK
            "com.jinke.mcxxl";
#else
            "com.jinke.mergeAnimal";
#endif
        /// <summary>
        /// 出包目录路径，打包打开指定的路径
        /// </summary>
        public string outPackageDirPath
        {
            get
            {
#if UNITY_ANDROID
                _outPackageDirPath = "Android";
#elif UNITY_IOS || UNITY_PHONE
                _outPackageDirPath = "IOS";
#elif UNITY_WEBGL
                _outPackageDirPath = "WebGL";
#else
                _outPackageDirPath = "Window";

#endif
                if (!Directory.Exists(_outPackageDirPath)) Directory.CreateDirectory(_outPackageDirPath);
                return _outPackageDirPath;
            }
        }
        public string GetOutFileName()
        {
            dateTimePackage = System.DateTime.Now.ToString("yyyy年MM月dd日_HH时mm分");
            string extension = this.PackageType == PackageType.exportProject|| this.PackageType == PackageType.webGL ? string.Empty : "." + this.PackageType.ToString();
            string fileName = this.GetWorkModeDirName() + "_" + this.bundleVersion + extension;
#if UNITY_ANDROID
            return dateTimePackage + "_" + fileName;
#elif UNITY_IOS || UNITY_PHONE
            dateTimePackage = System.DateTime.Now.ToString("MM月dd日");
            return dateTimePackage + "_" + fileName;
#elif UNITY_WEBGL
            return this.GetWorkModeDirName() ;
#else
            return dateTimePackage + "_" + fileName;
#endif
        }

        public void InitVersion()
        {
            string bundleVersionInfo = File.Exists(VersionPath) ? File.ReadAllText(VersionPath) : "1.0.0";
            version_Data.Init(bundleVersionInfo);
            lastversion_Data.Init(bundleVersionInfo);

            string bundleOldVersionInfo = File.Exists(OldVersionPath) ? File.ReadAllText(OldVersionPath) : "1.0.0";
            this.oldVersion_Data.Init(bundleOldVersionInfo);
            versionCode = version_Data.GetVersionCode();
            bundleVersion = version_Data.ToString();
            isUpdateVersion = false;

            androidPackageChannal.Clear();
#if USE_CNSDK
            this.Add(new ChannalData("com.jinke.xddmx", "雪地大冒险  采用金科sdk界面"));
#else
            this.Add(new ChannalData("com.jinke.mergeAnimal", "海外Google 金科官网" ));
#endif
            this.Add(new ChannalData("com.jinke.mergeanimalland.huawei", "海外合并动物 金科官网"));
            this.Add(new ChannalData("com.jinke.xddmx.nearme.gamecenter", "OPPO-单机  第三方界面",6));
            this.Add(new ChannalData("com.jinke.xddmx.vivo", "VIVO-单机  第三方界面", 8));
            this.Add(new ChannalData("com.jinke.xddmx.huawei", "华为-单机  第三方界面", 9));
            this.Add(new ChannalData("com.jinke.xddmx.mi", "小米-单机  第三方界面", 13));
            this.Add(new ChannalData("com.jinke.xddmx.m4399", "4399-单机  第三方界面", 4));
            this.Add(new ChannalData("com.jinke.xddmx.guanwnag", "好游快爆-单机  采用金科sdk界面", 57));
            this.Add(new ChannalData("com.jinke.xddmx.guanwnag", "TAPTAP-单机  采用金科sdk界面", 58));
            this.Add(new ChannalData("com.jinke.xddmx.uc", "UC-单机  第三方界面", 16));
            this.Add(new ChannalData("com.jinke.xddmx.yyb", "应用宝  第三方界面", 3 ));
        }

        public void Add(ChannalData channalData)
        {
            if (androidPackageChannal.FindIndex(p=>p.ToString()==channalData.ToString()) < 0)
            {
                androidPackageChannal.Add(channalData);
            }
        }
        public void EnableUpdateVersion()
        {
            isUpdateVersion = true;
        }
        public void UpdateVersion()
        {
            if (isUpdateVersion)
            {
                if (isCompiling)//编译更新包
                {
                    version_Data.small = 0;
                    version_Data.middle = lastversion_Data.middle + 1;
                    if (version_Data.middle >= 100)
                    {
                        version_Data.large = lastversion_Data.large + 1;
                        version_Data.middle = 0;
                    }

                }
                else//更新资源
                {
                    version_Data.middle = lastversion_Data.middle;
                    version_Data.small = lastversion_Data.small + 1;
                }

                versionCode = version_Data.GetVersionCode();
                bundleVersion = version_Data.ToString();
            }
        }

        public string GetGameVersionName()
        {
            return version_Data.GetGameVersionName();
        }

        public string GetGameOldVersionName()
        {
            return this.oldVersion_Data.GetGameVersionName();
        }

        /// <summary>
        /// 获取热更版本号
        /// </summary>
        public string GetHotVersionNumber()
        {
            return this.version_Data.ToString();
        }
        public void PackageFinish()
        {
            this.WriteVersion();

            this.InitVersion();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        public string GetUpdateData(int fileCount, FilesCfg updateFile)
        {
            var items = updateFile.items;
            long fileSize = 0;
            string abFileStr = "";
            try
            {
                int index = 0;
                foreach (var abpath in items)
                {
                    index++;
                    int size = int.Parse(abpath.size);
                    string fileName = abpath.respath.Substring(1);
                    float mSize = size / 1024F / 1024F;
                    if (mSize > 0.5F)
                        abFileStr += $" {index}, " + fileName + string.Format("------{0:f2}M", mSize) + "\n";
                    else
                        abFileStr += $" {index}, " + fileName + "\n";
                    fileSize += size;
                }
            }
            catch (Exception ex)
            {

            }

            return
                $"---------------------------更新信息-------------------------------\n" +
                $"更新资源版本时间：{System.DateTime.Now.ToLongDateString()} {System.DateTime.Now.ToLongTimeString()}\n" +
                $"包名={this.GetOutFileName()}\n" +
                $"版本={this.version_Data}\n" +
                $"模式={this.workMode.ToString()}\n" +
                $"更新资源总大小={string.Format("{0:f2}M", fileSize / 1024F / 1024F)}\n" +
                $"更新资源文件数量={fileCount}\n" +
                $"{abFileStr}" +
                $"------------------------------------------------------------------";
        }

        public string GetWorkModeDirName()
        {
            string workModeLocal = this.workMode.ToString();
            if (isPreReleaseMode)
            {
                workModeLocal = "develop_release";
#if UNITY_IOS || UNITY_IPHONE
                workModeLocal = "ios_develop_release";
#elif UNITY_WEBGL 
                workModeLocal = "web_develop_release";
#endif
            }

            if (PackageConifgEditor.Instance.isUSE_SDK)
            {
                return workModeLocal + "_SDK";
            }
            return workModeLocal;
        }

        public string GetOutDirPath()
        {
            string paltDir = "ios_HotUpdate";
#if UNITY_ANDROID
            paltDir = "android_HotUpdate";
#elif UNITY_WEBGL
            paltDir = "WebGL_HotUpdate";
#endif
            string outDirPath = "ab/" + paltDir + "/" + this.GetWorkModeDirName();
            if (!Directory.Exists(outDirPath))
            {
                Directory.CreateDirectory(outDirPath);
            }
            return outDirPath;
        }
        public int GetAppVersionCode()
        {
            return this.version_Data.GetOutAppVersionCode();
        }
        public string GetPackageVersion() => this.oldVersion_Data.GetGamePackVersion();
        private string VersionPath => Path.Combine(GetOutDirPath(), "version.txt");
        private string OldVersionPath => Path.Combine(GetOutDirPath(), "OldVersion.txt");
        private void WriteVersion()
        {
            if (Directory.Exists(GetOutDirPath()))
            {
                Directory.CreateDirectory(GetOutDirPath());
            }

            File.WriteAllText(VersionPath, this.version_Data.ToString(), System.Text.Encoding.UTF8);
            if (this.isChangePackage)
            {
                File.WriteAllText(OldVersionPath, this.version_Data.ToString(), System.Text.Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(OldVersionPath, this.oldVersion_Data.ToString(), System.Text.Encoding.UTF8);
            }
        }

        [ContextMenu("重置出包版本号")]
        private void ReWriteVersion()
        {
            if (Directory.Exists(GetOutDirPath()))
            {
                Directory.CreateDirectory(GetOutDirPath());
            }
            File.WriteAllText(VersionPath, this.lastversion_Data.ToString(), System.Text.Encoding.UTF8);
            this.InitVersion();
            AssetDatabase.SaveAssets();
        }

        [ContextMenu("重置资源版本号")]
        private void ReWriteOldVersion()
        {
            if (Directory.Exists(GetOutDirPath()))
            {
                Directory.CreateDirectory(GetOutDirPath());
            }
            File.WriteAllText(OldVersionPath, this.oldVersion_Data.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// cdn热歌路径
        /// </summary>
        public static string cdnURL =>
#if UNITY_ANDROID
#if USE_CNSDK
       "https://xddmx-cdn.jinkejoy.com/web/online/";//国内android
#else
       "https://mergeanimalland-cdn.jinkejoy.com/web/online/";//海外google android
#endif
#else
        "https://mergeanimalland-cdn.jinkejoy.com/web/ios_online/";//海外IOS
#endif
        public static void CollectPackageABurl()
        {
            string urlPath = cdnURL;
            string paltDir = "ab/ios";
#if UNITY_ANDROID
            paltDir = "ab/android";
#elif UNITY_WEBGL
            paltDir = "ab/webGL";
#endif
            List<string> lines = new List<string>();
            string packVersion = PackageConifgEditor.Instance.GetPackageVersion();
            var allFiels = Directory.GetFiles(paltDir + "/StreamingAssets", "*.*", SearchOption.AllDirectories);
            int length = allFiels.Length;
            for (int i = 0; i < length; i++)
            {
                string extand = Path.GetExtension(allFiels[i]);
                if (extand == ".manifest") continue;
                string url = allFiels[i].Replace('\\', '/').Replace(Application.dataPath.Replace("Assets/", ""), "");
                string shortPath = url.Replace(paltDir + "/", "");
                string outUrl = urlPath + packVersion + "/" + shortPath;
                lines.Add(outUrl);
            }

            lines.Add(urlPath + packVersion + "/StreamingAssets/files.byte");//清单文件
            lines.Add(urlPath + packVersion + "/version.json");//版本文件

            string writePath = Path.Combine(paltDir, $"cdn_PackFiles.json");
            File.WriteAllLines(writePath, lines.ToArray());
        }

        /// <summary>
        /// 检查超过2M的资源文件
        /// </summary>
        public static void CheckABResource()
        {
            Hukiry.HukiryToolEditor.ClearUnityConsole();
            string assetPath = Application.streamingAssetsPath;
            if (Directory.Exists(assetPath))
            {
                string[] files = Directory.GetFiles(assetPath, "*.unity3d", SearchOption.AllDirectories);

                if (files != null)
                {
                    int Length = files.Length;
                    for (int i = 0; i < Length; i++)
                    {
                        FileInfo fi = new FileInfo(files[i]);
                        float MFloat = fi.Length / 1024.0F / 1024.0F;
                        
                        if (MFloat >= 2)
                        {
                            string M = string.Format("{0:F2}", MFloat);
                            Loger.Log($"<color=red>路径:{files[i].Replace(Application.streamingAssetsPath, "StreamingAssets")},文件名：{fi.Name},文件 Size：{M}M, 超出2M需要拆分</color>");
                        }
                        else if (MFloat >= 1.5F)
                        {
                            string M = string.Format("{0:F2}", MFloat);
                            Loger.Log($"<color=orange>路径:{files[i].Replace(Application.streamingAssetsPath, "StreamingAssets")},文件名：{fi.Name},文件 Size：{M}M, 需要注意文件大小</color>");
                        }

                        if (i % 2 == 0)
                            EditorUtility.DisplayProgressBar("文件检查", fi.Name, i / (float)Length);
                    }

                }
                else
                {

                }
            }
            else
            {
                Loger.Error($"文件不存在:{assetPath}");
            }
            EditorUtility.ClearProgressBar();
        }
    }

    [Serializable]
    public struct ChannalData
    {
        public string packageName;
        public string tooltip;
        public int channelID;
        public ChannalData(string packageName, string tooltip, int channelID = 0)
        {
            this.packageName = packageName;
            this.tooltip = tooltip;
            this.channelID = channelID;
        }

        public override string ToString()
        {
            return $"{this.packageName}     {this.tooltip},{this.channelID}";
        }
    }

    [Serializable]
    public struct version_data
    {
        public int large;

        public int middle;

        public int small;

        public void Init(string version)
        {
            var array = version.Split('.');
            if (array != null && array.Length == 3)
            {
                int.TryParse(array[0], out large);
                int.TryParse(array[1], out middle);
                int.TryParse(array[2], out small);
            }
            else
            {
                large = 1;
                middle = 0;
                small = 0;
            }
        }

        public int GetVersionCode()
        {
            return large * 1000 + middle * 10 + small;
        }
        public string GetGameVersionName()
        {
            return $"{large}.{middle}.{small}";
        }

        public string GetGamePackVersion()
        {
            return $"v{large}.{middle}";
        }

        public int GetOutAppVersionCode()
        {
            string s = $"{large}{middle}";
            int.TryParse(s, out int result);
            return result > 0 ? result : 0;
        }

        public override string ToString()
        { 
           return $"{large}.{middle}.{small}";
        }
    }

    public enum BuildSystem
    {
        //
        // 摘要:
        //     Build APK using internal build system.
        Internal = 0,
        //
        // 摘要:
        //     Build APK using Gradle or export Gradle project.
        Gradle = 1,
    }

    /// <summary>
    /// 脚本编译模式
    /// </summary>
    public enum ScriptingBackend
    {
        /// <summary>
        /// ARMv7
        /// </summary>
        Mono = 0,
        /// <summary>
        /// ARMv7 & ARM64
        /// </summary>
        IL2CPP = 1,
    }

    /// <summary>
    /// 出包类型
    /// </summary>
    public enum PackageType
    {
        /// <summary>
        /// 测试包
        /// </summary>
        apk = 0,
        /// <summary>
        /// Google包 Gradle
        /// </summary>
        aab = 1,
        /// <summary>
        /// 导出android工程
        /// </summary>
        exportProject = 2,
        webGL = 3,
    }
}
