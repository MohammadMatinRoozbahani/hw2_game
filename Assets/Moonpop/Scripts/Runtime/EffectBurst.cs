using UnityEngine;

namespace Moonpop
{
    public sealed class EffectBurst : PoolObject
    {
        private ParticleSystem particles;
        private float remaining;
        public static EffectBurst Create(Transform parent,Material material,int index)
        {
            var go=new GameObject("Pooled burst "+index);go.transform.SetParent(parent,false);
            var burst=go.AddComponent<EffectBurst>();
            burst.particles=VfxDirector.MakeParticles(go,material,360);
            return burst;
        }
        public override void OnSpawn(){particles.Clear(true);remaining=3;}
        public override void OnDespawn(){particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        public void Emit(Color color,int count,float speed,float lifetime,float size,bool sparks=false)
        {
            remaining=lifetime+.3f;
            for(int i=0;i<count;i++)
            {
                Vector3 direction=Random.onUnitSphere;
                if(sparks)direction=new Vector3(direction.x,Mathf.Abs(direction.y)*.8f,direction.z-1.1f);
                var p=new ParticleSystem.EmitParams
                {
                    position=transform.position,
                    velocity=direction*Random.Range(speed*.35f,speed),
                    startColor=Color.Lerp(color,Color.white,Random.value*.3f),
                    startSize=Random.Range(size*.5f,size),
                    startLifetime=Random.Range(lifetime*.65f,lifetime)
                };
                particles.Emit(p,1);
            }
            particles.Play(true);
        }
        public void Rocket(Color color,float lifetime)
        {
            remaining=lifetime+.25f;
            var p=new ParticleSystem.EmitParams{position=transform.position,velocity=Vector3.up*26,
                startColor=color,startSize=.42f,startLifetime=lifetime};
            particles.Emit(p,1);particles.Play(true);
        }
        public void Firework(Color color,int pattern)
        {
            remaining=2.9f;
            for(int i=0;i<180;i++)
            {
                float a=i*Mathf.PI*2/180;Vector3 velocity;
                if(pattern==0)
                {
                    float x=16*Mathf.Pow(Mathf.Sin(a),3),y=13*Mathf.Cos(a)-5*Mathf.Cos(2*a)-2*Mathf.Cos(3*a)-Mathf.Cos(4*a);
                    velocity=new Vector3(x*.57f,y*.57f,Random.Range(-1f,1f));
                }
                else if(pattern==1)
                {
                    float r=7+3*Mathf.Cos(a*5);velocity=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Random.Range(-1,1));
                }
                else velocity=Random.onUnitSphere*Random.Range(7,12);
                var p=new ParticleSystem.EmitParams{position=transform.position,velocity=velocity,startColor=Color.Lerp(color,Color.white,Random.value*.18f),startSize=Random.Range(.18f,.32f),startLifetime=Random.Range(2.0f,2.6f)};
                particles.Emit(p,1);
            }
            particles.Play(true);
        }
        public void Tick(float dt){remaining-=dt;if(remaining<=0)Recycle();}
    }
}
