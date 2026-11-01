using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
    public sealed class VfxDirector : MonoBehaviour
    {
        private struct RocketSlot{public bool active;public float age;public Vector3 position;public Color color;public int pattern;}
        private readonly RocketSlot[] rockets=new RocketSlot[6];
        private FixedPool<EffectBurst> bursts;
        private GameManager game;
        private ParticleSystem exhaust;
        private float fireworkTimer=.5f;
        public int FireworksLaunched{get;private set;}
        public int FireworksExploded{get;private set;}
        private readonly Color[] fireworkColors={new Color(.12f,.8f,1),new Color(1,.08f,.42f),new Color(1,.65f,.15f),new Color(.35f,1,.58f)};

        public void Initialize(GameManager gm,ArtPalette a)
        {
            game=gm;
            bursts=new FixedPool<EffectBurst>(24,i=>EffectBurst.Create(transform,a.Particle,i));
            var go=new GameObject("Twin exhaust vapor");go.transform.SetParent(gm.Player.Visual,false);
            go.transform.localPosition=new Vector3(0,.36f,-1.8f);
            go.transform.localRotation=Quaternion.Euler(0,180,0);
            exhaust=MakeParticles(go,a.Particle,160);
            var main=exhaust.main;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.startLifetime=new ParticleSystem.MinMaxCurve(.22f,.6f);main.startSpeed=new ParticleSystem.MinMaxCurve(1,3);
            main.startSize=new ParticleSystem.MinMaxCurve(.12f,.3f);main.startColor=new Color(.17f,.55f,1f,.45f);
            var emission=exhaust.emission;emission.enabled=true;emission.rateOverTime=30;
            var shape=exhaust.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(1.3f,.03f,.01f);
            exhaust.Play(true);
        }
        public static ParticleSystem MakeParticles(GameObject go,Material mat,int max)
        {
            var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.maxParticles=max;
            main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.12f;main.duration=4;
            main.startLifetime=1;main.startSpeed=0;
            var emission=ps.emission;emission.enabled=false;
            var shape=ps.shape;shape.enabled=false;
            var color=ps.colorOverLifetime;color.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.8f,.5f),new GradientAlphaKey(0,1)});
            color.color=new ParticleSystem.MinMaxGradient(gradient);
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,.05f));
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            var trails=ps.trails;trails.enabled=true;trails.ratio=.32f;trails.lifetime=.18f;trails.minVertexDistance=.15f;trails.dieWithParticles=true;
            renderer.trailMaterial=mat;
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);return ps;
        }
        public void Pickup(Vector3 position,bool isFuel)
        {
            var fx=bursts.Rent(position);
            if(fx)fx.Emit(isFuel?new Color(.2f,1,.45f):new Color(1,.6f,.08f),isFuel?35:20,4,.7f,.17f);
        }
        public void Crash(Vector3 position)
        {
            var fx=bursts.Rent(position+Vector3.up*.8f);
            if(fx)fx.Emit(new Color(1,.45f,.08f),100,10,1.0f,.22f,true);
        }
        public void Celebrate()
        {
            var fx=bursts.Rent(new Vector3(0,2.5f,6));if(fx)fx.Emit(new Color(1,.8f,.32f),75,4.5f,1.1f,.21f);
        }
        public void ResetRun()
        {
            bursts.ReturnAll();for(int i=0;i<rockets.Length;i++)rockets[i].active=false;
            exhaust.Clear(true);fireworkTimer=1;
        }
        private void Update()
        {
            if(game==null)return;
            float dt=Time.deltaTime;
            var emission=exhaust.emission;emission.enabled=game.Session.State==GameState.Playing;
            emission.rateOverTime=game.Session.Boosting?85:Mathf.Lerp(18,42,game.Session.Difficulty);
            var exhaustMain=exhaust.main;exhaustMain.startColor=game.Session.Boosting?new Color(1,.54f,.22f,.8f):new Color(.62f,.80f,1,.32f);
            for(int i=bursts.Active.Count-1;i>=0;i--)bursts.Active[i].Tick(dt);
            if(game.Session.State==GameState.Paused)return;
            fireworkTimer-=dt;
            if(fireworkTimer<=0)
            {
                fireworkTimer=Random.Range(2.6f,4.6f);
                for(int i=0;i<rockets.Length;i++)
                {
                    if(rockets[i].active)continue;
                    float x=game.Session.State==GameState.Menu?Random.Range(-38f,-16f):Random.Range(-24f,24f);
                    Vector3 position=new Vector3(x,4,Random.Range(68f,118f));
                    Color color=fireworkColors[Random.Range(0,fireworkColors.Length)];
                    rockets[i]=new RocketSlot{active=true,age=0,position=position,color=color,pattern=Random.Range(0,3)};
                    FireworksLaunched++;
                    var fx=bursts.Rent(position);if(fx)fx.Rocket(color,1.25f);break;
                }
            }
            for(int i=0;i<rockets.Length;i++)
            {
                if(!rockets[i].active)continue;
                rockets[i].age+=dt;
                if(rockets[i].age>=1.25f)
                {
                    var fx=bursts.Rent(rockets[i].position+Vector3.up*32);
                    if(fx)fx.Firework(rockets[i].color,rockets[i].pattern);
                    FireworksExploded++;
                    rockets[i].active=false;game.Audio.PlayFirework();
                }
            }
        }
    }
}
