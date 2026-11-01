using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Moonpop.Core;
using UnityEngine;
namespace Moonpop
{
                                                                                                       
    public sealed class VisualAuditDriver:MonoBehaviour
    {
        [Serializable] private sealed class Result
        {
            public string editorVersion;
            public bool completed;
            public List<string> screenshots=new List<string>();
            public List<string> errors=new List<string>();
        }
        private readonly Result result=new Result();private string output;private GameManager game;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
#if UNITY_EDITOR
            if(!UnityEditor.EditorPrefs.GetBool("Moonpop.RunVisualAudit",false))return;
            UnityEditor.EditorPrefs.DeleteKey("Moonpop.RunVisualAudit");
            new GameObject("Automatic visual review").AddComponent<VisualAuditDriver>().StartCoroutine("Run");
#endif
        }
        private IEnumerator Run()
        {
            result.editorVersion=Application.unityVersion;
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Review",DateTime.Now.ToString("yyyyMMdd-HHmmss")));Directory.CreateDirectory(output);
            Application.logMessageReceived+=OnLog;
            bool hadCamera=PlayerPrefs.HasKey("Moonpop.Camera");int previousCamera=PlayerPrefs.GetInt("Moonpop.Camera",0);
            try
            {
                game=GameManager.Instance;Require(game!=null&&game.Session!=null,"The game did not boot.");game.AutomationMode=true;
                game.ReturnToMenu();yield return new WaitForSecondsRealtime(2.6f);Require(game.UI.MenuVisible,"Main menu is not visible.");yield return Capture("01_main_menu.png");
                game.StartRun();yield return new WaitForSecondsRealtime(1.3f);game.Session.SetBoostHeld(true);yield return new WaitForSecondsRealtime(.35f);
                Require(game.Session.Boosting&&game.Session.Nitro<100,"Nitro did not activate.");yield return Capture("02_nitro_gameplay.png");game.Session.SetBoostHeld(false);
                game.CycleCamera();yield return new WaitForSecondsRealtime(.3f);yield return Capture("03_camera.png");
                game.TogglePause();float fuel=game.Session.Fuel;double distance=game.Session.Distance;yield return new WaitForSecondsRealtime(.3f);
                Require(game.Session.State==GameState.Paused&&game.Session.Fuel==fuel&&game.Session.Distance==distance,"Pause did not freeze fuel/distance.");yield return Capture("04_pause.png");game.TogglePause();
                game.StartRun();
                for(int i=0;i<3;i++){game.HitTraffic(Vector3.forward);if(i<2)game.Session.Tick(1.6f);}
                Require(game.Session.State==GameState.GameOver&&game.Session.Reason==EndReason.Collision&&game.UI.GameOverVisible,"Three collisions did not show Game Over.");yield return Capture("05_collision_gameover.png");
                game.StartRun();game.Session.Tick(1000);Require(game.Session.Reason==EndReason.FuelEmpty,"An empty tank did not end the run.");yield return Capture("06_fuel_gameover.png");
                Require(game.World.PoolMisses==0,"A prewarmed object pool was exhausted.");result.completed=result.errors.Count==0;
            }
            finally
            {
                if(hadCamera)PlayerPrefs.SetInt("Moonpop.Camera",previousCamera);else PlayerPrefs.DeleteKey("Moonpop.Camera");PlayerPrefs.Save();
                Application.logMessageReceived-=OnLog;File.WriteAllText(Path.Combine(output,"engine-review.json"),JsonUtility.ToJson(result,true));
                Debug.Log("MOONPOP VISUAL AUDIT: "+(result.completed?"PASSED":"CHECK ERRORS")+" - "+output);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying=false;
#endif
            }
        }
        private IEnumerator Capture(string filename)
        {
            string path=Path.Combine(output,filename);ScreenCapture.CaptureScreenshot(path);
            for(int i=0;i<240;i++){yield return null;if(File.Exists(path)&&new FileInfo(path).Length>100){result.screenshots.Add(filename);yield break;}}
            throw new InvalidOperationException("Screenshot was not written: "+filename);
        }
        private void Require(bool condition,string message){if(!condition){result.errors.Add(message);throw new InvalidOperationException(message);}}
        private void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)result.errors.Add(message+"\n"+stack);}
    }
}
