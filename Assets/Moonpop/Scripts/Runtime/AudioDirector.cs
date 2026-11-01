using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
    public sealed class AudioDirector : MonoBehaviour
    {
        private AudioSource music,engine,effects;
        private AudioClip coin,fuel,crash,lane,click,firework,nitro,nearMiss,badge;
        private GameManager game;
        private float coinCooldown;
        private bool wasBoosting;
        public bool Muted { get; private set; }
        public void Initialize(GameManager gm)
        {
            game=gm;
            music=MakeSource(true);music.clip=Load("MidnightCircuit");music.volume=gm.Settings.musicVolume;music.Play();
            engine=MakeSource(true);engine.clip=Load("Engine");engine.volume=0;engine.Play();
            effects=MakeSource(false);effects.volume=gm.Settings.effectsVolume;
            coin=Load("Coin");fuel=Load("Fuel");crash=Load("Crash");lane=Load("Lane");click=Load("Click");firework=Load("Firework");
            nitro=Load("Nitro");nearMiss=Load("NearMiss");badge=Load("Badge");
            Muted=PlayerPrefs.GetInt("Moonpop.Muted",0)==1;
            ApplyMute();
        }
        private AudioSource MakeSource(bool loop)
        {
            var source=gameObject.AddComponent<AudioSource>();source.loop=loop;source.playOnAwake=false;
            source.spatialBlend=0;source.priority=loop?64:128;return source;
        }
        private static AudioClip Load(string name)
        {
            AudioClip clip=Resources.Load<AudioClip>("Audio/"+name);
            if(!clip)Debug.LogError("Missing audio: "+name);
            return clip;
        }
        private void ApplyMute(){music.mute=Muted;engine.mute=Muted;effects.mute=Muted;}
        public void ToggleMute(){Muted=!Muted;ApplyMute();PlayerPrefs.SetInt("Moonpop.Muted",Muted?1:0);PlayerPrefs.Save();}
        public void PlayCoin(){if(coinCooldown<=0){effects.PlayOneShot(coin,.55f);coinCooldown=.06f;}}
        public void PlayFuel(){effects.PlayOneShot(fuel,.85f);}
        public void PlayCrash(){effects.PlayOneShot(crash,.9f);}
        public void PlayLane(){effects.PlayOneShot(lane,.22f);}
        public void PlayClick(){effects.PlayOneShot(click,.5f);}
        public void PlayFirework(){if(game.Session.State!=GameState.Paused)effects.PlayOneShot(firework,.12f);}
        public void PlayNearMiss(){effects.PlayOneShot(nearMiss,.30f);}
        public void PlayBadge(){effects.PlayOneShot(badge,.65f);}
        private void Update()
        {
            if(game==null)return;
            coinCooldown-=Time.unscaledDeltaTime;
            bool playing=game.Session.State==GameState.Playing;
            if(game.Session.Boosting&&!wasBoosting)effects.PlayOneShot(nitro,.4f);
            wasBoosting=game.Session.Boosting;
            float dt=Time.unscaledDeltaTime;
            engine.volume=Mathf.MoveTowards(engine.volume,playing?.22f:0,dt*1.8f);
            engine.pitch=Mathf.Lerp(.78f,1.5f,game.Session.Difficulty)+(game.Session.Boosting?.32f:0);
            float targetMusic=game.Settings.musicVolume*(game.Session.State==GameState.Paused?.45f:1f);
            music.volume=Mathf.MoveTowards(music.volume,targetMusic,dt);
        }
    }
}
