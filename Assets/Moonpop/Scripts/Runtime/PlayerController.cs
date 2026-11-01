using Moonpop.Core;
using UnityEngine;
namespace Moonpop
{
 public sealed class PlayerController:MonoBehaviour
 {
  public const float HalfWidth=.79f,HalfLength=1.48f;
  public int Lane{get;private set;}=1;
  public Vector3 PreviousPosition{get;private set;}
  public Transform Visual=>visual;
  private Transform visual;private GameManager game;
  private MeshRenderer underglow,paint;private Light glowLight;
  private MaterialPropertyBlock glowProperties,paintProperties;
  private float changeStartX,changeTime=1,damageTime=10;
  private Color skinColor;
  public void Initialize(GameManager gm,ArtPalette art)
  {
   game=gm;visual=VehicleArt.Build(transform,art,0,true,out underglow);
   foreach(var r in visual.GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterial==art.Paint[0])paint=r;
   glowProperties=new MaterialPropertyBlock();paintProperties=new MaterialPropertyBlock();
   var glow=new GameObject("Speed-linked mint underglow");glow.transform.SetParent(visual,false);glow.transform.localPosition=new Vector3(0,.43f,0);
   glowLight=glow.AddComponent<Light>();glowLight.type=LightType.Point;glowLight.range=5.5f;glowLight.color=gm.Settings.cyan;
   for(int side=-1;side<=1;side+=2)
   {
    var go=new GameObject("Warm working headlight");go.transform.SetParent(visual,false);go.transform.localPosition=new Vector3(side*.64f,.97f,1.73f);
    go.transform.localRotation=Quaternion.Euler(14,0,0);var l=go.AddComponent<Light>();l.type=LightType.Spot;
    l.color=new Color(1,.86f,.61f);l.intensity=5.5f;l.range=40;l.spotAngle=62;l.shadows=LightShadows.None;
   }
   SetSkin(gm.SelectedSkin);ResetCar();
  }
  public void SetSkin(int index)
  {
   ArtMaterial m=ArtRecipe.Materials()[Mathf.Clamp(index,0,3)];skinColor=new Color(m.color.x,m.color.y,m.color.z);
   ApplyPaint(0);
  }
  public void ResetCar(){Lane=1;changeTime=1;changeStartX=0;damageTime=10;transform.position=Vector3.zero;PreviousPosition=transform.position;visual.localRotation=Quaternion.identity;ApplyPaint(0);}
  public void ChangeLane(int direction)
  {
   if(game.Session.State!=GameState.Playing)return;int next=Mathf.Clamp(Lane+direction,0,2);if(next==Lane)return;
   Lane=next;changeStartX=transform.position.x;changeTime=0;game.Audio.PlayLane();
  }
  public void Tick(float dt)
  {
   PreviousPosition=transform.position;changeTime+=dt/game.Session.Rules.LaneChangeSeconds;
   float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(changeTime));float x=Mathf.Lerp(changeStartX,(Lane-1)*game.Session.Rules.LaneWidth,t);
   float side=(x-transform.position.x)/Mathf.Max(dt,.0001f);transform.position=new Vector3(x,0,0);
   var q=Quaternion.Euler(0,Mathf.Clamp(side*.6f,-11,11),Mathf.Clamp(-side*.25f,-6,6));
   visual.localRotation=Quaternion.Slerp(visual.localRotation,q,1-Mathf.Exp(-dt*14));
  }
  public void TickVisual(float dt)
  {
   damageTime+=dt;float speed01=game.Session.State==GameState.Menu?.15f:Mathf.InverseLerp(game.Session.Rules.InitialSpeed,game.Session.Rules.MaximumSpeed,game.Session.Speed);
   glowProperties.SetColor("_EmissionColor",game.Settings.cyan*Mathf.Lerp(1.6f,4.6f,speed01));underglow.SetPropertyBlock(glowProperties);
   glowLight.intensity=Mathf.Lerp(1.6f,4.0f,speed01);
   if(game.Session.State==GameState.Menu)
   {
    visual.localRotation=Quaternion.Euler(0,180+Mathf.Sin(Time.time*.35f)*7,0);
    visual.localPosition=new Vector3(0,Mathf.Sin(Time.time*1.5f)*.025f,0);
   }
   else visual.localPosition=Vector3.zero;
   ApplyPaint(damageTime<1.5f&&Mathf.Sin(damageTime*32)>0?1:0);
  }
  private void ApplyPaint(float flash)
  {
   if(!paint)return;paintProperties.SetColor("_Color",Color.Lerp(skinColor,new Color(1,.26f,.36f),flash));
   paintProperties.SetColor("_EmissionColor",new Color(1,.1f,.25f)*flash*.75f);paint.SetPropertyBlock(paintProperties);
  }
  public void DamageFlash(){damageTime=0;}
 }
}
