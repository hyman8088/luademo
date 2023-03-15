using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hukiry.Editor
{


    /// <summary>
    /// 配置或一键出包
    /// </summary>
    public class PackageWindow : EditorWindow, IHasCustomMenu
    {
        public bool lastUseSdk = false;
        private static Rect rect = new Rect(0, 0, 800, 500);
        private GUILayoutOption HeightLayoutOption;
        private string versionPackagePath = $"{Application.streamingAssetsPath}/unityVersion.txt";
        private const string workModeTooltip = "Release模式强制选择cdn服，正式服。其他模式有选择服，可选择cdn";
        private const string CompilingToolTip = "如果勾选编译打全包，否则只打包资源版本热更！";
        private WorkModeDef workMode;

        private static string GetPlatIco =>
#if UNITY_ANDROID
            "d_BuildSettings.Android.Small";
#elif UNITY_WEBGL
            "d_BuildSettings.WebGL";
#elif UNITY_STANDALONE || UNITY_WEBPLAYER
            "d_BuildSettings.Standalone";
#else
            "d_BuildSettings.iPhone.Small";
#endif

        private GUIContent ReWriteVersionGuiContent, ReWriteOldVersionGuiContent;
        private string gamePackageName;
        public static void ShowPackage()
        {
            PackageWindow packageWindow = GetWindowWithRect<PackageWindow>(rect,false);
            packageWindow.titleContent = new GUIContent("打包工具", Hukiry.HukiryToolEditor.GetTexture2D(GetPlatIco));
            packageWindow.Show();
        }

        private void OnEnable()
        {
            PackageConifgEditor.Instance.isEnableCDN = false;
            HeightLayoutOption = GUILayout.Height(25);
            PackageConifgEditor.Instance.InitVersion();
            //"d_BuildSettings.iPhone.Small"
            ReWriteVersionGuiContent = new GUIContent(@"Reset/出包版本号");
            ReWriteOldVersionGuiContent = new GUIContent(@"Reset/资源版本号");
            gamePackageName = PackageConifgEditor.Instance.selectPackageName;
            this.titleContent = new GUIContent("打包工具", Hukiry.HukiryToolEditor.GetTexture2D(GetPlatIco));
            //var t = (from r in PackageConifgEditor.Instance.androidPackageChannal where r.packageName!=null select r.position == Vector3.zero);
        }

        private void OnGUI()
        {
            Color colorDefault =
#if UNITY_2019_1_OR_NEWER
                
           new Color(255, 255, 255, 1F);
            Color colorContent = Color.black;
#else
           new Color(255, 255, 255, 255);
             Color colorContent = Color.white;
#endif
            GUI.skin.label.fontSize = 18;
            
            GUILayout.Label("资源配置");
            GUI.color = colorDefault;
           
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                GUI.contentColor = colorContent;
                PackageConifgEditor.Instance.isPackageResource = EditorGUILayout.ToggleLeft("打包资源", PackageConifgEditor.Instance.isPackageResource, HeightLayoutOption);

                if (GUILayout.Button("ab文件大小检查", HeightLayoutOption))
                {
                    PackageConifgEditor.CheckABResource();
                }

                if (GUILayout.Button("清除游戏缓存", HeightLayoutOption))
                {
                    ClearCache.GetFoodConfi();
                    PlayerPrefs.SetString("Localizer", PackageConifgEditor.Instance.isUSE_CNSDK ? "zh_CN" : "en");
                }

                if (PackageConifgEditor.Instance.isPackageResource)
                {
#if UNITY_ANDROID||UNITY_WEBGL
                    GUI.color = Color.green;
                    if (GUILayout.Button("一键资源打包", HeightLayoutOption))
                    {
                        PackMakerEditor.MarkAndPackageALLOfResource();
                    }
                    GUI.color = colorDefault;
#endif
                }
                else
                {
                    if (GUILayout.Button("依赖资源检查", HeightLayoutOption))
                    {
                        CheckResourceWin.ShowWindow();
                    }
                }
#if UNITY_ANDROID||UNITY_WEBGL
                if (GUILayout.Button("Mark Window", HeightLayoutOption))
                {
                    PackMakerEditor.MarkResource();
                }
#endif

                if (GUILayout.Button("定位配置", HeightLayoutOption))
                {
                    Hukiry.HukiryToolEditor.LocationObject<PackageConifgEditor>("PackageConifgEditor");
                }
                GUI.contentColor = Color.white;
            }
           
            EditorGUILayout.Space();
            GUI.color = Color.white;
            GUILayout.Label("打包配置");
            GUI.color = colorDefault;
            EditorGUILayout.BeginVertical(GUI.skin.box);

            //-----------------------水平分割线---------------------------------
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUI.contentColor = colorContent;
                //SDK环境切换部分
                using (new EditorGUILayout.HorizontalScope())
                {
                    var alignment = GUI.skin.label.alignment;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUIContent gUIContent = new GUIContent(PackageConifgEditor.Instance.isUSE_CNSDK ? "国内-工程出包环境" : "海外-工程出包环境");
                    gUIContent.tooltip = "注意：区分国内和海外的语言切换、SDK切换、CDN切换、默认货币符号切换、包名切换，用户协议切换等等。";
                    if (GUILayout.Button(gUIContent, GUI.skin.label, GUILayout.Height(20)))
                    {
                        GenericMenu genericMenu = new GenericMenu();
                        genericMenu.AddItem(new GUIContent("国内-工程出包环境"), PackageConifgEditor.Instance.isUSE_CNSDK, () =>
                        {
                            PackageConifgEditor.Instance.isUSE_CNSDK = true;
                            PlayerPrefs.SetString("Localizer", PackageConifgEditor.Instance.isUSE_CNSDK ? "zh_CN" : "en");
                            SetSymbols();
                        });
                        genericMenu.AddItem(new GUIContent("海外-工程出包环境"), !PackageConifgEditor.Instance.isUSE_CNSDK, () =>
                        {
                            PackageConifgEditor.Instance.isUSE_CNSDK = false;
                            PlayerPrefs.SetString("Localizer", PackageConifgEditor.Instance.isUSE_CNSDK ? "zh_CN" : "en");
                            SetSymbols();
                        });
                        genericMenu.ShowAsContext();
                    }
                    GUI.skin.label.alignment = alignment;
                }
                EditorGUILayout.Space();

                //出包模式选择
                using (new EditorGUILayout.HorizontalScope())
                {
                    workMode = (WorkModeDef)EditorGUILayout.EnumPopup(new GUIContent("WorkMode:", workModeTooltip), PackageConifgEditor.Instance.workMode);
                    if (PackageConifgEditor.Instance.workMode != workMode)
                    {
                        PackageConifgEditor.Instance.workMode = workMode;
                        if (PackageConifgEditor.Instance.workMode == WorkModeDef.Release)
                        {
                            PackageConifgEditor.Instance.isEnableCDN = true;
                        }
                        PackageConifgEditor.Instance.InitVersion();
                    }
                    if (workMode == WorkModeDef.Develop)
                    {
                        GUILayout.Space(20);
                        var isPreReleaseMode = EditorGUILayout.ToggleLeft("预发布包", PackageConifgEditor.Instance.isPreReleaseMode, GUILayout.Width(80));
                        if (PackageConifgEditor.Instance.isPreReleaseMode != isPreReleaseMode)
                        {
                            PackageConifgEditor.Instance.isPreReleaseMode = isPreReleaseMode;
                            PackageConifgEditor.Instance.InitVersion();
                        }
                    }
                    else
                    {
                        PackageConifgEditor.Instance.isPreReleaseMode = false;
                    }
                }
            }EditorGUILayout.Space();

            //显示清单 //-----------------------水平分割线---------------------------------
            using (new EditorGUILayout.HorizontalScope())
            {
#if UNITY_ANDROID
#if !USE_CNSDK
                gamePackageName = "com.jinke.mergeAnimal";
#endif
#elif UNITY_IOS || UNITY_IPHONE
                gamePackageName = "com.jk.ios.merge";
#if USE_CNSDK
                gamePackageName = "com.jk.ios.merge.cn";//国内Ios包名
#endif
#else
                gamePackageName = PlayerSettings.applicationIdentifier;
#endif
#if USE_CNSDK
                var conBTN = new GUIContent("包名：" + gamePackageName, PackageConifgEditor.Instance.androidPackageChannal?.FirstOrDefault(p=>p.packageName== gamePackageName).tooltip);
                if (GUILayout.Button(conBTN, GUI.skin.label, HeightLayoutOption))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    PackageConifgEditor.Instance.androidPackageChannal.ForEach(p => {genericMenu.AddItem(new GUIContent(p.ToString(), p.tooltip), p.packageName == gamePackageName, packString =>{gamePackageName = (string)packString;PackageConifgEditor.Instance.selectPackageName = (string)packString;}, p.packageName);});
                    genericMenu.ShowAsContext();
                }
#else
                GUILayout.Label("包名：" + gamePackageName, HeightLayoutOption);
#endif
                if (GUILayout.Button("更新版本")) PackageConifgEditor.Instance.EnableUpdateVersion();
            }

            GUI.contentColor = Color.white;
            //-----------------------水平分割线---------------------------------
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("包版本：" + PackageConifgEditor.Instance.bundleVersion, HeightLayoutOption);
                GUI.color = Color.cyan;
                if (PackageConifgEditor.Instance.isChangePackage)
                {
                    GUILayout.Label("强更资源版本：" + PackageConifgEditor.Instance.GetGameVersionName(), HeightLayoutOption);
                }
                else
                {
                    GUILayout.Label("资源版本：" + PackageConifgEditor.Instance.GetGameOldVersionName(), HeightLayoutOption);
                }
                GUI.color = colorDefault;
            }
            GUILayout.Label("包版本号：" + PackageConifgEditor.Instance.versionCode, HeightLayoutOption);
            if(PackageConifgEditor.Instance.isCompiling)
            GUILayout.Label("出包：" + PackageConifgEditor.Instance.GetOutFileName(), HeightLayoutOption);

            PackageConifgEditor.Instance.UpdateVersion();
            //-----------------------水平分割线---------------------------------
            using (new EditorGUILayout.HorizontalScope())
            {
                PackageConifgEditor.Instance.isUSE_SDK = EditorGUILayout.ToggleLeft("USE SDK", PackageConifgEditor.Instance.isUSE_SDK);
                GUI.color = workMode == WorkModeDef.Release ? Color.green * 255 : Color.white*0.8F;
                PackageConifgEditor.Instance.isDebug = EditorGUILayout.ToggleLeft(new GUIContent("出审核包", "仅正式服使用"), PackageConifgEditor.Instance.isDebug);
                PackageConifgEditor.Instance.isEnableCDN = EditorGUILayout.ToggleLeft(new GUIContent("使用CDN", "仅正式服使用"), PackageConifgEditor.Instance.isEnableCDN);
                PackageConifgEditor.Instance.isChangePackage = EditorGUILayout.ToggleLeft(new GUIContent("强更包", "仅正式服使用"), PackageConifgEditor.Instance.isChangePackage);
                GUI.color = colorDefault;
            }

            //是否打包SDK
            if (lastUseSdk != PackageConifgEditor.Instance.isUSE_SDK)
            {
                lastUseSdk = PackageConifgEditor.Instance.isUSE_SDK;
                SetSymbols();  
            }

            //-----------------------水平分割线---------------------------------
            using (new EditorGUILayout.VerticalScope())
            {
                GUI.color = Color.white;
                if (!PackageConifgEditor.Instance.isCompiling) EditorGUILayout.HelpBox(workMode == WorkModeDef.Release ? workModeTooltip : CompilingToolTip, MessageType.Info);
                else
                {
                    Hukiry.HukiryToolEditor.DrawLine(Color.white * 0.6F, Color.cyan * 0.6F);
                    Hukiry.HukiryToolEditor.DrawLine(Color.cyan * 0.6F, Color.green * 0.6F);
                }
                GUI.color = colorDefault;
            }
            using (new GUILayout.HorizontalScope())
            {
                PackageConifgEditor.Instance.isCompiling = EditorGUILayout.Toggle("是否编译包", PackageConifgEditor.Instance.isCompiling, HeightLayoutOption);
                if (PackageConifgEditor.Instance.workMode == WorkModeDef.Editor)
                {
                    PackageConifgEditor.Instance.workMode = WorkModeDef.Debug;
                }

#if UNITY_ANDROID
#if USE_CNSDK
                PackageConifgEditor.Instance.isDelivery = false;
#else

                //海外包
                if (PackageConifgEditor.Instance.workMode == WorkModeDef.Release)
                {
                    PackageConifgEditor.Instance.isDelivery = EditorGUILayout.Toggle("是否分包", PackageConifgEditor.Instance.isDelivery, HeightLayoutOption);
                    PackageConifgEditor.Instance.isPreReleaseMode = false;
                }
                else
                {
                    PackageConifgEditor.Instance.isDelivery = false;
                }
#endif
#endif
                string btnTxtCon = PackageConifgEditor.Instance.isOpenGuide ? "埋点" : "未配置";
                if (GUILayout.Button(btnTxtCon, GUI.skin.box, GUILayout.Width(120), GUILayout.Height(22)))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("埋点"), PackageConifgEditor.Instance.isOpenGuide, () => {
                        PackageConifgEditor.Instance.isOpenGuide = !PackageConifgEditor.Instance.isOpenGuide;
                    });
                    genericMenu.ShowAsContext();
                }
                GUILayout.Space(10);
                //PackageConifgEditor.Instance.isOpenGuide = EditorGUILayout.Toggle("开启埋点", PackageConifgEditor.Instance.isOpenGuide, HeightLayoutOption);
                if (GUILayout.Button("Player Settings", GUI.skin.box, GUILayout.Width(120), GUILayout.Height(22)))
                {
                    BuildPlayerWindow.ShowBuildPlayerWindow();
                }
                GUI.color = Color.black;
                GUILayout.Space(10);
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUI.color = Color.white;
                    if (GUILayout.Button("备份streamingAssets", GUI.skin.box, GUILayout.Width(120), GUILayout.Height(22)))
                    {
                        PackMakerResource.CopystreamingAssetsPath(true);
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("还原streamingAssets", GUI.skin.box, GUILayout.Width(120), GUILayout.Height(22)))
                    {
                        PackMakerResource.CopystreamingAssetsPath(false);
                    }
                }
            }

            //打包配置环节
            if (PackageConifgEditor.Instance.isCompiling)
            {
                GUI.contentColor = colorContent;
#if UNITY_WEBGL
                PackageConifgEditor.Instance.ScriptingBackend = ScriptingBackend.IL2CPP;
#else
                PackageConifgEditor.Instance.ScriptingBackend = (ScriptingBackend)EditorGUILayout.EnumPopup("编译类型Script Backend:", PackageConifgEditor.Instance.ScriptingBackend);
#endif
                if (PackageConifgEditor.Instance.ScriptingBackend == ScriptingBackend.IL2CPP)
                    PackageConifgEditor.Instance.AndroidBuildType = (AndroidBuildType)EditorGUILayout.EnumPopup("编译模式:", PackageConifgEditor.Instance.AndroidBuildType);


#if UNITY_ANDROID
                using (new GUILayout.HorizontalScope(GUI.skin.label))
                {
                    PackageConifgEditor.Instance.isArmv7 = EditorGUILayout.ToggleLeft("ARMv7", PackageConifgEditor.Instance.isArmv7);
                    switch (PackageConifgEditor.Instance.ScriptingBackend)
                    {
                        case ScriptingBackend.Mono:
                            PackageConifgEditor.Instance.isX86 = EditorGUILayout.ToggleLeft("x86", PackageConifgEditor.Instance.isX86);
                            break;
                        case ScriptingBackend.IL2CPP:
                            PackageConifgEditor.Instance.isArm64 = EditorGUILayout.ToggleLeft("ARM64", PackageConifgEditor.Instance.isArm64);
                            break;
                    }
                }
                EditorGUILayout.Space();
                PackageConifgEditor.Instance.PackageType = (PackageType)EditorGUILayout.EnumPopup("出包类型:", PackageConifgEditor.Instance.PackageType);
#elif UNITY_WEBGL
                PackageConifgEditor.Instance.PackageType = PackageType.webGL;
#else
                PackageConifgEditor.Instance.PackageType = PackageType.exportProject;
#endif
                GUI.contentColor = Color.white;
            }
#if USE_CNSDK
            if (GUI.changed)
            {
                FindObjectOfType<SplashAnimation>()?.SetEnable(PackageConifgEditor.Instance.isCompiling);
            }
#endif

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (EditorApplication.isCompiling)
            { EditorGUILayout.HelpBox("正在编译...", MessageType.Warning); }
            string buildBtn = PackageConifgEditor.Instance.PackageType == PackageType.webGL || PackageConifgEditor.Instance.PackageType == PackageType.webGL ? "Export" : "Build";

            GUI.color = Color.green;
            //开始打包按钮
            if (GUILayout.Button(buildBtn, HeightLayoutOption))
            {
                if (PackageConifgEditor.Instance.isCompiling == false)
                {
                    if (File.Exists(Application.streamingAssetsPath + "/files.byte") == false)
                    {
                      if(EditorUtility.DisplayDialog("系统提示", "先还原SteaminAsset才可以进行资源打包", "确认", "取消"))
                        {
                            PackMakerResource.CopystreamingAssetsPath(false);
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            return;
                        }
                       
                    }
                }
#if UNITY_STANDALONE_WIN
                if (EditorUtility.DisplayDialog("系统提示", "当前是windows 请切换到Android平台！", "确认"))
                { EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android); }
                return;
#endif

#if USE_CNSDK
                if (FindObjectOfType<SplashAnimation>() == null)
                {
                    EditorUtility.DisplayDialog("系统提示", "请切换到游戏场景，或当前组件不存在", "确认");
                }
                FindObjectOfType<SplashAnimation>().SetEnable(true);
#endif
                if (File.Exists(versionPackagePath))
                {
                    File.Delete(versionPackagePath);
                }

                if (GameObject.FindObjectOfType<StartLogic>() == null)
                {
                    EditorUtility.DisplayDialog("系统提示", @"请切换到指定的场景出包" +
                        "菜单栏：工具->Scene选择->MainScene", "确认");
                    return;
                }
                GameObject.FindObjectOfType<StartLogic>().ShowWorkMode = PackageConifgEditor.Instance.workMode;
                GameObject.FindObjectOfType<StartLogic>().isPreReleaseMode = PackageConifgEditor.Instance.isPreReleaseMode;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                //记录上次打包的记录
               
                
                PackageConifgEditor.Instance.SaveAssets();
                if (PackageConifgEditor.Instance.isPackageResource)
                {
                    PackMakerEditor.PackageResourcesFunction();
                    AssetDatabase.Refresh();
                }

                if (!PackageConifgEditor.Instance.isCompiling)
                {
                    //写入热更资源包版本
                    PackageConifgEditor.Instance.PackageFinish();
                    int TotalSeconds1 = (int)stopwatch.Elapsed.TotalSeconds;
                    string useTime1 = $"{TotalSeconds1 / 60}分{TotalSeconds1 % 60}秒";
                    stopwatch.Stop();
                    if (EditorUtility.DisplayDialog("打包成功", "打包资源使用时间：" + useTime1, "确认"))
                    {
                        string pathURL = PackageConifgEditor.Instance.GetOutDirPath();
                        Application.OpenURL(pathURL.Replace('/','\\'));
                    }
                    return;
                }

                EditorUtility.DisplayProgressBar("系统提示", "正在打包中...", 0.8f);
                //打包设置
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
                switch (PackageConifgEditor.Instance.PackageType)
                {
                    case PackageType.aab:
                        EditorUserBuildSettings.buildAppBundle = true;
                        break;
                    case PackageType.exportProject:
                        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                        break;
                    case PackageType.webGL:
                        //EditorUserBuildSettings.development = true;
                        break;
                }

                //配置脚本编译
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, (ScriptingImplementation)PackageConifgEditor.Instance.ScriptingBackend);

                //设置包名
                PlayerSettings.applicationIdentifier = gamePackageName;
                PlayerSettings.bundleVersion = PackageConifgEditor.Instance.bundleVersion;
#if UNITY_ANDROID
 
                PlayerSettings.Android.renderOutsideSafeArea = true;

                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.None;
                if (PackageConifgEditor.Instance.isArmv7)
                {
                    PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARMv7;
                }
                switch (PackageConifgEditor.Instance.ScriptingBackend)
                {
                    case ScriptingBackend.Mono:
                        if (PackageConifgEditor.Instance.isX86)
                        {
                            PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.X86;
                        }
                        break;
                    case ScriptingBackend.IL2CPP:
                        if (PackageConifgEditor.Instance.isArm64)
                        {
                            PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARM64;
                        }
                        EditorUserBuildSettings.androidBuildType = PackageConifgEditor.Instance.AndroidBuildType;
                        break;

                }

               
                PlayerSettings.Android.bundleVersionCode = PackageConifgEditor.Instance.versionCode;
#elif UNITY_WEBGL
#else
                PlayerSettings.iOS.buildNumber = PackageConifgEditor.Instance.versionCode.ToString();
#endif
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                File.WriteAllText(versionPackagePath, PlayerSettings.bundleVersion);//打包完成后，保存本地版本文件
                this.WritePackageVersion();//写入包资源版本
 
                string outPath = PackageConifgEditor.Instance.outPackageDirPath + "/" + PackageConifgEditor.Instance.GetOutFileName();
                BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, outPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.CompressWithLz4);

                //写入审核包版本
                PackageConifgEditor.Instance.PackageFinish();
                AssetDatabase.Refresh();

#if UNITY_ANDROID
                //分包部分
                if (PackageConifgEditor.Instance.isDelivery)
                {
                    PackMakerEditor.BuildZip();
                    Debug.Log("压缩ab文件完成！");
                }

                //sdk部分
                if (PackageConifgEditor.Instance.isUSE_SDK)
                {
                    CopyAndroidFile(outPath, PlayerSettings.bundleVersion);
                }
#elif UNITY_WEBGL

#endif

                Debug.Log("包：" + Path.GetFullPath(outPath));
                int TotalSeconds = (int)stopwatch.Elapsed.TotalSeconds;
                string useTime = $"{TotalSeconds / 60}分{TotalSeconds % 60}秒";
                stopwatch.Stop();
                if (EditorUtility.DisplayDialog("打包成功", "打包时间一共：" + useTime + "\n是否定位到包文件所在路径", "确认"))
                {
#if UNITY_ANDROID
                    Application.OpenURL(PackageConifgEditor.Instance.isUSE_SDK ? PackageConifgEditor.Instance.androidbuildgradle + "\\unity-android-resources" : PackageConifgEditor.Instance.outPackageDirPath);
#elif UNITY_IOS || UNITY_IPHONE
                    string curPath = "file://" + Path.GetFullPath(PackageConifgEditor.Instance.outPackageDirPath);
                    Application.OpenURL(curPath);
#elif UNITY_WEBGL
                    Application.OpenURL(Path.GetFullPath(PackageConifgEditor.Instance.outPackageDirPath));
#endif
                }
            }
            GUI.skin.label.fontSize = 12;
            GUI.color = Color.white;
        }
        private static void CopyAndroidFile(string dirPath, string bundleVersion)
        {
            string[] arrayDirName =
            {
                "assets",
                "jniLibs",
            };

            string androidProject = PackageConifgEditor.Instance.androidbuildgradle + "unity-android-resources";
            EditorUtility.DisplayProgressBar("Unity Moving ... ", "", 0.1f);
            foreach (var item in arrayDirName)
            {
                string dirshortPath = Path.Combine(androidProject, item);
                if (Directory.Exists(dirshortPath))
                {
                    Directory.Delete(dirshortPath, true);
                    Debug.Log($"删除成功：{dirshortPath}");
                    EditorUtility.DisplayProgressBar("Unity Moving ... ", "", 0.5f);
                }
            }

            AssetDatabase.Refresh();
            //拷贝
            string targetProject = Path.Combine(dirPath, $"{PlayerSettings.productName}/src/main");

            if (!Directory.Exists(androidProject)) { Directory.CreateDirectory(androidProject); }
            foreach (var item in arrayDirName)
            {
                string sourceDirName = Path.Combine(targetProject, item).Replace('\\','/');
                string destDirName = Path.Combine(androidProject, item).Replace('\\', '/');
                Debug.Log($"移动目录：{sourceDirName} 到 {destDirName}");
                if (PackageConifgEditor.Instance.isDelivery)
                {
                    if (item == "assets")
                    {

                        if (!Directory.Exists(destDirName)) { Directory.CreateDirectory(destDirName); }
                        //移动Unity库
                        string[] files = Directory.GetFiles(sourceDirName);
                        foreach (var filePath in files)
                        {
                            File.Copy(sourceDirName + "/" + Path.GetFileName(filePath), destDirName + "/" + Path.GetFileName(filePath), true);
                        }
                        CopyOldLabFilesToNewLab(sourceDirName+"/bin", destDirName + "/bin");
                        continue;
                    }
                }
            
                CopyOldLabFilesToNewLab(sourceDirName, destDirName);
                EditorUtility.DisplayProgressBar("Unity Moving ... ", "", 0.7f);

            }
            EditorUtility.DisplayProgressBar("Unity Moving ... ", "", 0.8f);
            AssetDatabase.Refresh();

            PackageAndroidAarEditor.GradlewBatBuild(bundleVersion, PackageConifgEditor.Instance.versionCode);
            EditorUtility.DisplayProgressBar("Unity export arr ... ", "", 0.9f);
            EditorUtility.ClearProgressBar();
        }

        //文件夹拷贝
        public static bool CopyOldLabFilesToNewLab(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

#region //拷贝labs文件夹到savePath下
            try
            {
                string[] labDirs = Directory.GetDirectories(sourcePath);//目录
                string[] labFiles = Directory.GetFiles(sourcePath);//文件
                if (labFiles.Length > 0)
                {
                    for (int i = 0; i < labFiles.Length; i++)
                    {
                        //Loger.Log(sourcePath + "          =====labFiles[i]=====" + labFiles[i] + "       =====  Path.GetFileName=====" + Path.GetFileName(labFiles[i]));
                        File.Copy(sourcePath + "/" + Path.GetFileName(labFiles[i]), savePath + "/" + Path.GetFileName(labFiles[i]), true);
                    }
                }
                else
                {
                }
                if (labDirs.Length > 0)
                {
                    for (int j = 0; j < labDirs.Length; j++)
                    {
                        Directory.GetDirectories(sourcePath + "/" + Path.GetFileName(labDirs[j]));
                        //递归调用
                        CopyOldLabFilesToNewLab(sourcePath + "/" + Path.GetFileName(labDirs[j]), savePath + "/" + Path.GetFileName(labDirs[j]));
                    }
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Loger.Error("==文件路径出错==" + e.ToString());
                return false;
            }
#endregion
            return true;
        }
        
        //修改中设置宏定义
        [InitializeOnLoadMethod]
        private static void SetSymbols()
        {
            //设置脚本宏定义符号
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            Func<string, bool, string> funcSymbol = (symbolName, isAdd) =>
            {
                List<string> temp = symbols.Split(';').ToList();
                if (isAdd ? !temp.Contains(symbolName) : temp.Contains(symbolName))
                    (isAdd ? temp.Add : (Action<string>)(strItem => { temp.Remove(strItem); }))(symbolName);
                return string.Join(";", temp);
            };
            symbols = funcSymbol(PackageConifgEditor.USE_CNSDK, PackageConifgEditor.Instance.isUSE_CNSDK);
            symbols = funcSymbol(PackageConifgEditor.USE_SDK, PackageConifgEditor.Instance.isUSE_SDK);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }

        //写入包版本
        private void WritePackageVersion()
        {
            string versionPath = Application.streamingAssetsPath + "/version.json";
            if (File.Exists(versionPath))
            {
                Vesion version = LitJson.JsonMapper.ToObject<Vesion>(File.ReadAllText(versionPath));
                if (PackageConifgEditor.Instance.isChangePackage)
                {
                    version.version = PackageConifgEditor.Instance.GetGameVersionName();
                }
                else
                {
                    version.version = PackageConifgEditor.Instance.GetGameOldVersionName();
                }
                version.WorkMode = PackageConifgEditor.Instance.workMode.ToString();
                version.isCDN = PackageConifgEditor.Instance.isEnableCDN;
                version.isDebug = PackageConifgEditor.Instance.isDebug;

#if UNITY_ANDROID
                version.downUrlforAnd = "https://play.google.com/store/apps/details?id=com.jinke.mergeAnimal";
#elif UNITY_IOS || UNITY_IPHONE
                version.downUrlforAnd = "https://apps.apple.com/us/app/merge-animal-land/id1601595486";
#endif

                //国内更新地址变化
#if USE_CNSDK
                 version.downUrlforAnd = "https://xddmx-cdn.jinkejoy.com/web/xddmx_release.apk";
#endif
                if (PackageConifgEditor.Instance.workMode == WorkModeDef.Release)
                {
#if USE_CNSDK
                    version.gameip = "xddmx-online.jinkejoy.com";
                    version.jsonUrl = "https://xddmx-platform.jinkejoy.com/game/";//正式服充值平台域名
                    version.dotLink = "https://xddmx-platform.jinkejoy.com/game/";
                    version.phpUrl = "https://xddmx-cdn.jinkejoy.com/query/checkServer.json";//桶名：xddmx-cdn
#else
                    version.gameip = "miracle-online.jinkeglobal.com";
                    version.jsonUrl = "http://miracle-platform.jinkeglobal.com:9418/game/";
                    version.dotLink = "http://miracle-platform.jinkeglobal.com:9418/game/";
                    version.phpUrl = "https://mergeanimalland-cdn.jinkejoy.com/query/checkServer.json";
#endif
                }
                else
                {
                    if (PackageConifgEditor.Instance.isPreReleaseMode)
                    {
#if USE_CNSDK
                        version.gameip = "119.3.26.74";
#else
                        version.gameip = "miracle-test.jinkeglobal.com";
#endif
                        version.jsonUrl = "http://miracle-test.jinkeglobal.com:9418/game/";
                        version.dotLink = "http://miracle-test.jinkeglobal.com:9418/game/";
                        version.phpUrl = "https://mergeanimalland-cdn.jinkejoy.com/querytest/checkServer.json";
                    }
                    else
                    {
#if USE_CNSDK
                        //测试sdk 国内
                        version.gameport = 80;
                        version.gameip = "miracle-api-test.jinkejoy.com";
                        version.jsonUrl = "https://miracle-res-test.jinkejoy.com/game/";
                        version.dotLink = "https://miracle-res-test.jinkejoy.com/game/";
                        version.phpUrl = "https://xddmx-cdn.jinkejoy.com/querydebug/checkServer.json";
#else
                        //海外 google测试服地址
                        version.gameport = 9011;
                        version.jsonUrl = "http://miracle-res-test.jinkejoy.com:9419/game/";
                        version.dotLink = "http://miracle-res-test.jinkejoy.com:9419/game/";
                        version.phpUrl = "https://mergeanimalland-cdn.jinkejoy.com/querydebug/checkServer.json";
#endif
                    }
                }
                version.isDebug = PackageConifgEditor.Instance.isOpenGuide;
                string sversion = "{\n" +
                    $"  \"version\": \"{ version.version}\",\n" +
                    $"  \"downUrlforAnd\": \"{ version.downUrlforAnd}\",\n" +
                    $"  \"webUrl\": \"{ version.webUrl}\",\n" +
                    $"  \"jsonUrl\": \"{ version.jsonUrl}\",\n" +
                    $"  \"gameip\": \"{ version.gameip}\",\n" +
                    $"  \"gameport\": { version.gameport},\n" +
                    $"  \"phpUrl\": \"{ version.phpUrl}\",\n" +
                    $"  \"dotLink\": \"{ version.dotLink}\",\n" +
                    $"  \"WorkMode\": \"{ version.WorkMode}\",\n" +
                    $"  \"isCDN\": { version.isCDN.ToString().ToLower()},\n" +
                    $"  \"isOpenGuide\": { version.isOpenGuide.ToString().ToLower()},\n" +
                    $"  \"isDebug\": { version.isDebug.ToString().ToLower()},\n" +
                    $"  \"timestamp\": \"{ version.timestamp}\""
                    + "\n}";
                File.WriteAllText(versionPath, sversion);
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(ReWriteOldVersionGuiContent, false, () => {
                Hukiry.HukiryToolEditor.InvokeMethodInstance(PackageConifgEditor.Instance, "ReWriteOldVersion");
            });

            menu.AddItem(ReWriteVersionGuiContent, false, ()=> {
                Hukiry.HukiryToolEditor.InvokeMethodInstance(PackageConifgEditor.Instance, "ReWriteVersion");
            });
        }
    }
}
