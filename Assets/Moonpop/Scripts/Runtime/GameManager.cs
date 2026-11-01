using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
    public sealed class GameManager : MonoBehaviour
    {
        private const string ScoreKey="Moonpop.HighScore";
        public static GameManager Instance { get; private set; }
        public GameSettings Settings { get; private set; }
        public RunSession Session { get; private set; }
        public PlayerController Player { get; private set; }
        public VfxDirector Effects { get; private set; }
        public AudioDirector Audio { get; private set; }
        public UIOverlay UI { get; private set; }
        public InputRouter Input { get; private set; }
        public ThirdPersonRig CameraRig { get; private set; }
        public WorldDirector World { get; private set; }
        public int RecordAtStart { get; private set; }
        public int SelectedSkin{get;private set;}
        public bool LowSpec{get;private set;}
        public bool ReducedMotion{get;private set;}
        public bool AutomationMode{get;set;}
        private int lastBadges;
        private bool initialized;
        private void Awake()
        {
            if(Instance&&Instance!=this){Destroy(gameObject);return;}
            Instance=this;
        }

        public void Initialize(GameSettings settings)
        {
            Settings=settings;
            SelectedSkin=Mathf.Clamp(PlayerPrefs.GetInt("Moonpop.Skin",0),0,3);
            LowSpec=PlayerPrefs.GetInt("Moonpop.LowSpec",0)==1;ReducedMotion=PlayerPrefs.GetInt("Moonpop.Calm",0)==1;
            Session=new RunSession(settings.rules,PlayerPrefs.GetInt(ScoreKey,0));
            Time.timeScale=1;Application.targetFrameRate=60;
            QualitySettings.vSyncCount=1;ApplyQuality();
            var art=new ArtPalette();SetupNight();
            Player=Child("Player coupe").AddComponent<PlayerController>();Player.Initialize(this,art);
            Audio=Child("Audio").AddComponent<AudioDirector>();Audio.Initialize(this);
            Effects=Child("Pooled visual effects").AddComponent<VfxDirector>();Effects.Initialize(this,art);
            World=new WorldDirector(this,art,Child("Pooled world").transform);
            Child("Moon, balloons and ferris wheel").AddComponent<FestivalBackdrop>().Initialize(art);
            CameraRig=Child("Third person camera").AddComponent<ThirdPersonRig>();CameraRig.Initialize(this);
            Input=Child("Input actions").AddComponent<InputRouter>();Input.Initialize(this);
            var uiGo=new GameObject("Interface",typeof(RectTransform));uiGo.transform.SetParent(transform,false);
            UI=uiGo.AddComponent<UIOverlay>();UI.Initialize(this);
            Session.StateChanged+=OnStateChanged;initialized=true;
            OnStateChanged(GameState.Menu);
        }
        private GameObject Child(string name){var go=new GameObject(name);go.transform.SetParent(transform,false);return go;}
        private void SetupNight()
        {
            RenderSettings.skybox=new Material(Resources.Load<Shader>("Rendering/NightSky"));
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.46f,.51f,.70f);
            RenderSettings.ambientEquatorColor=new Color(.32f,.33f,.48f);
            RenderSettings.ambientGroundColor=new Color(.26f,.21f,.32f);
            RenderSettings.ambientIntensity=1.1f;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogDensity=.0035f;RenderSettings.fogColor=new Color(.12f,.12f,.24f);
            var light=Child("Warm moon key + soft shadows").AddComponent<Light>();light.type=LightType.Directional;
            light.color=new Color(1,.89f,.78f);light.intensity=1.2f;light.shadows=LightShadows.Soft;light.shadowStrength=.72f;
            light.shadowBias=.03f;light.shadowNormalBias=.18f;light.transform.rotation=Quaternion.Euler(42,-27,0);
            RenderSettings.sun=light;
            var fill=Child("Cool fill for readable silhouettes").AddComponent<Light>();fill.type=LightType.Directional;
            fill.color=new Color(.61f,.74f,1);fill.intensity=.48f;fill.shadows=LightShadows.None;fill.transform.rotation=Quaternion.Euler(30,145,0);
        }
        private void Update()
        {
            if(!initialized)return;
            float dt=Mathf.Min(Time.deltaTime,.075f);
            if(Session.State==GameState.Playing)
            {
                Player.Tick(dt);double beforeDistance=Session.Distance;
                Session.Tick(dt);
                if(Session.State==GameState.Playing)World.Tick((float)(Session.Distance-beforeDistance)/Mathf.Max(dt,.0001f),dt);
                if(Session.Badges!=lastBadges){lastBadges=Session.Badges;UI.Notify("NEW STICKER!  +500",Settings.cyan);Audio.PlayBadge();Effects.Celebrate();}
            }
            else if(Session.State==GameState.Menu)World.AdvanceRoad(-7*dt);
            Player.TickVisual(dt);
        }
        public void StartRun()
        {
            SaveBest();RecordAtStart=Session.BestScore;
            Time.timeScale=1;
            Player.ResetCar();Effects.ResetRun();
            lastBadges=0;
            Session.Start();World.ResetRun();CameraRig.Snap();UI.ShowState(GameState.Playing);
            UI.Notify("LET'S MAKE SOME HAPPY LITTLE MILES!",Settings.cyan);
        }
        public void TogglePause()
        {
            if(Session.State==GameState.Playing)Session.Pause();
            else if(Session.State==GameState.Paused)Session.Resume();
        }
        public void ReturnToMenu()
        {
            SaveBest();Session.ReturnToMenu();World.ShowMenu();Player.ResetCar();CameraRig.Snap();
        }
        public void ToggleMute(){Audio.ToggleMute();UI.RefreshSound();}
        public void SelectSkin(int index)
        {
            if(Session.State!=GameState.Menu)return;SelectedSkin=Mathf.Clamp(index,0,3);Player.SetSkin(SelectedSkin);
            PlayerPrefs.SetInt("Moonpop.Skin",SelectedSkin);PlayerPrefs.Save();UI.RefreshGarage();
        }
        public void CycleCamera(){if(Session.State==GameState.Playing)CameraRig.Cycle();}
        public void ToggleQuality(){LowSpec=!LowSpec;ApplyQuality();PlayerPrefs.SetInt("Moonpop.LowSpec",LowSpec?1:0);PlayerPrefs.Save();UI.RefreshGarage();}
        public void ToggleCalm(){ReducedMotion=!ReducedMotion;PlayerPrefs.SetInt("Moonpop.Calm",ReducedMotion?1:0);PlayerPrefs.Save();UI.RefreshGarage();}
        private void ApplyQuality()
        {
            QualitySettings.pixelLightCount=LowSpec?4:8;QualitySettings.shadows=LowSpec?ShadowQuality.Disable:ShadowQuality.All;
            QualitySettings.shadowResolution=ShadowResolution.Medium;QualitySettings.shadowDistance=LowSpec?0:75;
        }
        public void AwardNearMiss()
        {
            int award=Session.RegisterNearMiss();if(award<=0)return;
            UI.Notify("CLOSE CALL!  x"+Session.Combo+"  +"+award,new Color(1,.83f,.42f));Audio.PlayNearMiss();
        }
        public void Quit()
        {
            SaveBest();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        public void HitTraffic(Vector3 position)
        {
            if(!Session.HitObstacle())return;
            Player.DamageFlash();CameraRig.Shake();Effects.Crash(position);Audio.PlayCrash();UI.Damage();
            if(Session.State==GameState.Playing)UI.Notify("HIT!  "+Session.Lives+" LIVES LEFT",Settings.pink);
        }
        private void OnStateChanged(GameState state)
        {
            Time.timeScale=state==GameState.Paused?0:1;
            Input.SetState(state);UI.ShowState(state);
            if(state==GameState.GameOver)SaveBest();
        }
        private void SaveBest()
        {
            if(Session==null||AutomationMode)return;
            Session.CommitBest();int stored=PlayerPrefs.GetInt(ScoreKey,0);
            if(Session.BestScore>stored){PlayerPrefs.SetInt(ScoreKey,Session.BestScore);PlayerPrefs.Save();}
        }
        private void PauseForFocusLoss()
        {
            if(!initialized||AutomationMode||Session.State!=GameState.Playing)return;
            Session.Pause();
            UI.ShowPauseHint("Window lost focus.\nClick the game, then press ESC / P to continue.");
        }
        private void OnApplicationFocus(bool focus){if(!focus)PauseForFocusLoss();}
        private void OnApplicationPause(bool pause){if(!initialized||AutomationMode||!pause)return;PauseForFocusLoss();SaveBest();}
        private void OnApplicationQuit(){SaveBest();}
        private void OnDestroy(){if(Instance==this){Instance=null;Time.timeScale=1;}}
    }
}
