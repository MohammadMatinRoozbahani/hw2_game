using System.Collections;
using Moonpop.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Moonpop.Tests
{
    public sealed class RuntimeTests
    {
        private GameObject bootstrap;
        private bool hadBest;private int oldBest;
        [UnitySetUp] public IEnumerator Setup()
        {
            hadBest=PlayerPrefs.HasKey("Moonpop.HighScore");oldBest=PlayerPrefs.GetInt("Moonpop.HighScore");
            if(GameManager.Instance)Object.Destroy(GameManager.Instance.gameObject);
            yield return null;
            bootstrap=new GameObject("Test bootstrap");bootstrap.AddComponent<NeonBootstrap>();
            yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            Time.timeScale=1;
            if(GameManager.Instance)Object.Destroy(GameManager.Instance.gameObject);
            if(bootstrap)Object.Destroy(bootstrap);
            yield return null;
            if(hadBest)PlayerPrefs.SetInt("Moonpop.HighScore",oldBest);else PlayerPrefs.DeleteKey("Moonpop.HighScore");
            PlayerPrefs.Save();
        }
        [UnityTest] public IEnumerator BootCreatesAllSystemsAndMainMenu()
        {
            var g=GameManager.Instance;
            Assert.That(g,Is.Not.Null);Assert.That(g.Session.State,Is.EqualTo(GameState.Menu));
            Assert.That(g.Player,Is.Not.Null);Assert.That(g.UI,Is.Not.Null);Assert.That(g.CameraRig,Is.Not.Null);
            Assert.That(g.Input,Is.Not.Null);Assert.That(g.Audio,Is.Not.Null);Assert.That(g.Effects,Is.Not.Null);
            Assert.That(g.UI.MenuVisible,Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator PausedRunFreezesFuelAndDistanceThenResumes()
        {
            var g=GameManager.Instance;g.StartRun();yield return new WaitForSeconds(.15f);
            g.TogglePause();float fuel=g.Session.Fuel;double distance=g.Session.Distance;
            for(int i=0;i<10;i++)yield return null;
            Assert.That(g.Session.Fuel,Is.EqualTo(fuel));Assert.That(g.Session.Distance,Is.EqualTo(distance));
            g.TogglePause();yield return new WaitForSeconds(.15f);
            Assert.That(g.Session.Distance,Is.GreaterThan(distance));
        }
        [UnityTest] public IEnumerator LaneInputClampsAtRoadEdges()
        {
            var g=GameManager.Instance;g.StartRun();g.Player.ChangeLane(-1);g.Player.ChangeLane(-1);g.Player.ChangeLane(-1);
            yield return new WaitForSeconds(.3f);
            Assert.That(g.Player.Lane,Is.EqualTo(0));Assert.That(g.Player.transform.position.x,Is.EqualTo(-3.2f).Within(.02));
            for(int i=0;i<5;i++)g.Player.ChangeLane(1);
            yield return new WaitForSeconds(.3f);
            Assert.That(g.Player.Lane,Is.EqualTo(2));Assert.That(g.Player.transform.position.x,Is.EqualTo(3.2f).Within(.02));
        }
        [UnityTest] public IEnumerator EmptyTankShowsGameOverAndRestartResetsRun()
        {
            var g=GameManager.Instance;g.StartRun();g.Session.Tick(1000);
            Assert.That(g.UI.GameOverVisible,Is.True);
            Assert.That(g.Session.Reason,Is.EqualTo(EndReason.FuelEmpty));
            g.StartRun();Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
            Assert.That(g.Session.Lives,Is.EqualTo(3));Assert.That(g.Session.Fuel,Is.EqualTo(100));
            yield return null;
        }
        [UnityTest] public IEnumerator NewInputSystemActionsSteerAndDisableWhenPaused()
        {
            var g=GameManager.Instance;g.StartRun();
            Keyboard keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));yield return null;
                Assert.That(g.Player.Lane,Is.EqualTo(0));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                g.TogglePause();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return null;
                Assert.That(g.Player.Lane,Is.EqualTo(0));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
            }
            finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator GameplayAndMenuNavigationHaveExclusiveInput()
        {
            var g=GameManager.Instance;g.AutomationMode=true;
            var module=g.Input.GetComponentInChildren<InputSystemUIInputModule>();
            var actions=module.actionsAsset;
            Assert.That(actions.FindAction("UI/Navigate").enabled,Is.True);
            Assert.That(actions.FindAction("Gameplay/Left").enabled,Is.False);
            g.StartRun();
            foreach(string name in new[]{"Navigate","Submit","Cancel"})
                Assert.That(actions.FindAction("UI/"+name).enabled,Is.False,name);
            foreach(string name in new[]{"Point","Click","Scroll"})
                Assert.That(actions.FindAction("UI/"+name).enabled,Is.True,name);
            Assert.That(EventSystem.current.sendNavigationEvents,Is.False);
            Assert.That(actions.FindAction("Gameplay/Left").enabled,Is.True);
            Assert.That(actions.FindAction("System/Pause").enabled,Is.True);
            g.TogglePause();
            Assert.That(actions.FindAction("Gameplay/Left").enabled,Is.False);
            Assert.That(actions.FindAction("UI/Navigate").enabled,Is.True);
            Assert.That(actions.FindAction("UI/Submit").enabled,Is.True);
            Assert.That(EventSystem.current.sendNavigationEvents,Is.True);
            g.TogglePause();
            Assert.That(actions.FindAction("UI/Navigate").enabled,Is.False);
            Assert.That(actions.FindAction("Gameplay/Left").enabled,Is.True);
            g.ReturnToMenu();
            Assert.That(actions.FindAction("UI/Submit").enabled,Is.True);
            Assert.That(actions.FindAction("Gameplay/Left").enabled,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator ArrowKeysSteerAfterHudButtonIsSelected()
        {
            var g=GameManager.Instance;g.AutomationMode=true;g.StartRun();
            foreach(var button in g.UI.GetComponentsInChildren<Button>())
                if(button.name=="CAM")EventSystem.current.SetSelectedGameObject(button.gameObject);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.RightArrow));yield return null;
                Assert.That(g.Player.Lane,Is.EqualTo(2));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftArrow));yield return null;
                Assert.That(g.Player.Lane,Is.EqualTo(1));
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            }
            finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator SpaceBoostDoesNotPressSelectedHudPauseButton()
        {
            var g=GameManager.Instance;g.AutomationMode=true;g.StartRun();
            foreach(var button in g.UI.GetComponentsInChildren<Button>())
                if(button.name=="II")EventSystem.current.SetSelectedGameObject(button.gameObject);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                Assert.That(g.Session.Boosting,Is.True);
                Assert.That(g.Session.Nitro,Is.LessThan(100));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Assert.That(g.Session.Boosting,Is.False);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Paused));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            }
            finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator EnterCanStartAndResumeFromMenus()
        {
            var g=GameManager.Instance;g.AutomationMode=true;
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                g.TogglePause();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            }
            finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator FocusLossPausesAndKeyboardResumeClearsBoost()
        {
            var g=GameManager.Instance;g.AutomationMode=true;g.StartRun();
            g.Session.SetBoostHeld(true);Assert.That(g.Session.Boosting,Is.True);
            g.AutomationMode=false;
            g.SendMessage("OnApplicationFocus",false,SendMessageOptions.RequireReceiver);
            g.AutomationMode=true;
            Assert.That(g.Session.State,Is.EqualTo(GameState.Paused));
            bool hasHint=false;
            foreach(var text in g.UI.GetComponentsInChildren<Text>())
                if(text.text.Contains("Window lost focus."))hasHint=true;
            Assert.That(hasHint,Is.True);
            g.SendMessage("OnApplicationFocus",true,SendMessageOptions.RequireReceiver);
            Assert.That(g.Session.State,Is.EqualTo(GameState.Paused));
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;
                Assert.That(g.Session.State,Is.EqualTo(GameState.Playing));
                Assert.That(g.Session.Boosting,Is.False);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));yield return null;
                Assert.That(g.Player.Lane,Is.EqualTo(0));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            }
            finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator ThreeTrafficHitsEndRunAndPersistBest()
        {
            var g=GameManager.Instance;g.StartRun();g.Session.CollectCoin();
            for(int i=0;i<3;i++){g.HitTraffic(Vector3.forward);if(i<2)g.Session.Tick(1.6f);}
            Assert.That(g.Session.State,Is.EqualTo(GameState.GameOver));
            Assert.That(g.Session.Reason,Is.EqualTo(EndReason.Collision));
            Assert.That(PlayerPrefs.GetInt("Moonpop.HighScore"),Is.GreaterThanOrEqualTo(25));
            yield return null;
        }
        [UnityTest] public IEnumerator RestartReusesPooledGameObjects()
        {
            var g=GameManager.Instance;g.StartRun();yield return new WaitForSeconds(.2f);
            int before=Resources.FindObjectsOfTypeAll<GameObject>().Length;
            for(int i=0;i<10;i++){g.StartRun();yield return null;}
            int after=Resources.FindObjectsOfTypeAll<GameObject>().Length;
            Assert.That(after,Is.EqualTo(before));Assert.That(g.World.PoolMisses,Is.EqualTo(0));
        }
        [UnityTest] public IEnumerator NitroActionAndCameraModesWork()
        {
            var g=GameManager.Instance;g.StartRun();g.AutomationMode=true;
            var keyboard=InputSystem.AddDevice<Keyboard>();bool hadCamera=PlayerPrefs.HasKey("Moonpop.Camera");int oldCamera=PlayerPrefs.GetInt("Moonpop.Camera");
            try
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.LeftShift));yield return null;
                Assert.That(g.Session.Boosting,Is.True);Assert.That(g.Session.Nitro,Is.LessThan(100));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;Assert.That(g.Session.Boosting,Is.False);
                int mode=g.CameraRig.Mode;g.CycleCamera();g.CycleCamera();g.CycleCamera();Assert.That(g.CameraRig.Mode,Is.EqualTo(mode));
            }
            finally{InputSystem.RemoveDevice(keyboard);if(hadCamera)PlayerPrefs.SetInt("Moonpop.Camera",oldCamera);else PlayerPrefs.DeleteKey("Moonpop.Camera");PlayerPrefs.Save();}
        }
        [UnityTest] public IEnumerator RequiredNightLightingAndPackageBloomExist()
        {
            var g=GameManager.Instance;var lights=g.Player.GetComponentsInChildren<Light>();int headlights=0;
            foreach(var light in lights)if(light.type==LightType.Spot&&light.intensity>0)headlights++;
            Assert.That(headlights,Is.EqualTo(2));Assert.That(RenderSettings.skybox,Is.Not.Null);Assert.That(RenderSettings.sun,Is.Not.Null);
            var volume=g.GetComponentInChildren<PostProcessVolume>();Assert.That(volume,Is.Not.Null);
            Bloom bloom;Assert.That(volume.sharedProfile.TryGetSettings(out bloom),Is.True);Assert.That(bloom.enabled.value,Is.True);Assert.That(bloom.intensity.value,Is.GreaterThan(0));
            yield return null;
        }
        [UnityTest] public IEnumerator FireworksLaunchAndExplodeInTheSky()
        {
            var g=GameManager.Instance;g.AutomationMode=true;yield return new WaitForSecondsRealtime(2.3f);
            Assert.That(g.Effects.FireworksLaunched,Is.GreaterThan(0));Assert.That(g.Effects.FireworksExploded,Is.GreaterThan(0));
        }
        [UnityTest] public IEnumerator InfiniteRoadRecyclingKeepsAConstantObjectCount()
        {
            var g=GameManager.Instance;g.AutomationMode=true;
            int before=Resources.FindObjectsOfTypeAll<GameObject>().Length;
            var tiles=g.GetComponentsInChildren<RoadSegment>();
            System.Array.Sort(tiles,(a,b)=>a.transform.position.z.CompareTo(b.transform.position.z));
            float menuZ=tiles[2].transform.position.z;
            g.World.AdvanceRoad(-1);
            Assert.That(tiles[2].transform.position.z,Is.EqualTo(menuZ+1).Within(.001));
            for(int i=0;i<600;i++)g.World.AdvanceRoad(-8);
            AssertContinuousRoad(g);
            Assert.That(g.Session.State,Is.EqualTo(GameState.Menu));
            g.StartRun();
            tiles=g.GetComponentsInChildren<RoadSegment>();
            System.Array.Sort(tiles,(a,b)=>a.transform.position.z.CompareTo(b.transform.position.z));
            float playZ=tiles[2].transform.position.z;
            g.World.AdvanceRoad(1);
            Assert.That(tiles[2].transform.position.z,Is.EqualTo(playZ-1).Within(.001));
            for(int i=0;i<600;i++)g.World.AdvanceRoad(8);
            AssertContinuousRoad(g);
            Assert.That(Resources.FindObjectsOfTypeAll<GameObject>().Length,Is.EqualTo(before));Assert.That(g.World.PoolMisses,Is.EqualTo(0));yield return null;
        }
        private static void AssertContinuousRoad(GameManager game)
        {
            var tiles=game.GetComponentsInChildren<RoadSegment>();
            Assert.That(tiles.Length,Is.EqualTo(game.Settings.roadSegments));
            System.Array.Sort(tiles,(a,b)=>a.transform.position.z.CompareTo(b.transform.position.z));
            for(int i=1;i<tiles.Length;i++)
                Assert.That(tiles[i].transform.position.z-tiles[i-1].transform.position.z,Is.EqualTo(game.Settings.segmentLength).Within(.02));
            Assert.That(tiles[0].transform.position.z,Is.LessThanOrEqualTo(-game.Settings.segmentLength*.5f+.02f));
            Assert.That(tiles[tiles.Length-1].transform.position.z+game.Settings.segmentLength*.5f,Is.GreaterThanOrEqualTo(game.Settings.spawnHorizon));
        }
    }
}
