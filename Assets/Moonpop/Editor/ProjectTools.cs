using System;
using System.IO;
using System.Text.RegularExpressions;
using Moonpop.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Moonpop.Editor
{
    public static class ProjectTools
    {
        public const string RequiredEditorVersion="6000.6.5f1";
        public const string ScenePath="Assets/Moonpop/Scenes/Moonpop.unity";

        [InitializeOnLoadMethod]
        private static void OnImport()
        {
            EditorApplication.delayCall+=()=>
            {
                if(Application.isBatchMode||EditorApplication.isPlayingOrWillChangePlaymode)return;
                if(SceneManager.sceneCount==1 && string.IsNullOrEmpty(SceneManager.GetActiveScene().path) && !SceneManager.GetActiveScene().isDirty)
                    EditorSceneManager.OpenScene(ScenePath);
            };
        }
        [MenuItem("Moonpop/Open game scene")]
        public static void OpenScene()
        {
            if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Moonpop/Validate project")]
        public static void ValidateProject()
        {
            if(Application.unityVersion!=RequiredEditorVersion)
                throw new BuildFailedException("Open and build this project with Unity "+RequiredEditorVersion+". Current Editor: "+Application.unityVersion);
            if(!File.Exists(ScenePath))throw new BuildFailedException("The game scene is missing.");
            var settings=Resources.Load<GameSettings>("GameSettings");
            if(!settings)throw new BuildFailedException("GameSettings.asset is missing.");
            settings.rules.Validate();
            if(settings.rules.StartingLives!=3)throw new BuildFailedException("HW2 requires exactly 3 starting lives.");
            if(settings.roadSegments*settings.segmentLength<settings.spawnHorizon+100)
                throw new BuildFailedException("Road coverage is shorter than the visible horizon.");
            var controls=Resources.Load<InputActionAsset>("Input/DrivingControls");
            if(!controls)throw new BuildFailedException("Input Actions asset is missing.");
            foreach(string name in new[]{"Gameplay/Left","Gameplay/Right","Gameplay/Boost","System/Camera","System/Pause","System/Restart","System/Menu","System/Mute","UI/Point","UI/Click","UI/Navigate","UI/Submit","UI/Cancel","UI/Scroll"})
                if(controls.FindAction(name)==null)throw new BuildFailedException("Missing action "+name);
            if(!Regex.IsMatch(File.ReadAllText("ProjectSettings/ProjectSettings.asset"),@"activeInputHandler:\s*1\b"))
                throw new BuildFailedException("Set Active Input Handling to Input System Package (New), then restart the Editor.");
            foreach(string shader in new[]{"NeonSurface","NeonParticle","NightSky"})
            {
                Shader resource=Resources.Load<Shader>("Rendering/"+shader);
                if(!resource)throw new BuildFailedException("Missing shader "+shader);
#if UNITY_6000_0_OR_NEWER
                if(ShaderUtil.ShaderHasError(resource))throw new BuildFailedException("Shader compilation error: "+shader+". Check the Console.");
#endif
            }
            if(!Resources.Load<PostProcessResources>("Rendering/PostProcessResources"))throw new BuildFailedException("Post-processing resources are missing.");
            foreach(string clip in new[]{"MidnightCircuit","Engine","Coin","Fuel","Crash","Lane","Click","Firework","Nitro","NearMiss","Badge"})
                if(!Resources.Load<AudioClip>("Audio/"+clip))throw new BuildFailedException("Missing audio "+clip);
            foreach(string font in new[]{"Regular","Bold","Mono"})
                if(!Resources.Load<Font>("Fonts/"+font))throw new BuildFailedException("Missing font "+font);
            if(!typeof(Interactable).IsAbstract || !typeof(Interactable).IsAssignableFrom(typeof(MovingObstacle)) || !typeof(Interactable).IsAssignableFrom(typeof(FuelCan)))
                throw new BuildFailedException("Invalid Interactable inheritance.");
            var planner=new TrafficPlanner(10);
            for(int i=0;i<10000;i++)
            {
                TrafficRow row=planner.Next(i/10000f);
                if(row.IsBlocked(row.SafeLane)||row.BlockedMask==7)throw new BuildFailedException("Invalid traffic plan.");
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.companyName="Moonpop Studio";PlayerSettings.productName="Moonpop - Night Parade";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=false;
            AssetDatabase.SaveAssets();Debug.Log("MOONPOP: project validation passed.");
        }
        [MenuItem("Moonpop/Build/Windows x64")]
        public static void BuildWindows(){Build(BuildTarget.StandaloneWindows64,"Builds/Windows/Moonpop.exe");}
        [MenuItem("Moonpop/Build/Linux x64")]
        public static void BuildLinux(){Build(BuildTarget.StandaloneLinux64,"Builds/Linux/Moonpop.x86_64");}
        [MenuItem("Moonpop/Review/Run visual audit and capture screens")]
        public static void RunVisualAudit()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new BuildFailedException("Stop Play Mode before starting the visual audit.");
            ValidateProject();
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(ScenePath);
            Type view=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if(view!=null){var window=EditorWindow.GetWindow(view);window.Show();window.Focus();}
            EditorPrefs.SetBool("Moonpop.RunVisualAudit",true);EditorApplication.isPlaying=true;
        }
        private static void Build(BuildTarget target,string output)
        {
            ValidateProject();
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,target))
                throw new BuildFailedException("Install the "+target+" Build Support module in Unity Hub.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options=new BuildPlayerOptions{scenes=new[]{ScenePath},target=target,locationPathName=output,options=BuildOptions.None};
            BuildReport report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("Build failed: "+report.summary.result);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output),"RUN.txt"),
                "MOONPOP\nRun the executable beside its _Data folder and UnityPlayer library.\n"+
                "A/D or arrows: lanes | Esc/P: pause | R: restart | Backspace: menu | M: sound\n");
            Debug.Log("MOONPOP: build succeeded at "+Path.GetFullPath(output));
        }
    }
}
