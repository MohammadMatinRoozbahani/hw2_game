using Moonpop.Core;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
namespace Moonpop
{
 public sealed class ThirdPersonRig:MonoBehaviour
 {
  private GameManager game;private Camera cameraComponent;private float shake;
  public int Mode{get;private set;}
  public string ModeName=>new[]{"CHASE","COZY","WIDE"}[Mode];
  public void Initialize(GameManager gm)
  {
   game=gm;Mode=Mathf.Clamp(PlayerPrefs.GetInt("Moonpop.Camera",0),0,2);
   cameraComponent=gameObject.AddComponent<Camera>();gameObject.tag="MainCamera";
   cameraComponent.allowHDR=true;cameraComponent.allowMSAA=false;cameraComponent.nearClipPlane=.15f;cameraComponent.farClipPlane=360;
   cameraComponent.clearFlags=CameraClearFlags.Skybox;gameObject.AddComponent<AudioListener>();
   var resources=Resources.Load<PostProcessResources>("Rendering/PostProcessResources");
   if(!resources)throw new System.InvalidOperationException("Post-Processing resource asset is missing.");
   var layer=gameObject.AddComponent<PostProcessLayer>();layer.Init(resources);layer.volumeLayer=1;layer.volumeTrigger=transform;
   layer.antialiasingMode=PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
   var go=new GameObject("Real package Bloom + pastel color grade");go.transform.SetParent(gm.transform,false);
   var volume=go.AddComponent<PostProcessVolume>();volume.isGlobal=true;volume.priority=10;
   var profile=ScriptableObject.CreateInstance<PostProcessProfile>();
   var bloom=profile.AddSettings<Bloom>();bloom.enabled.Override(true);bloom.intensity.Override(1.15f);bloom.threshold.Override(1.3f);bloom.diffusion.Override(5.6f);bloom.fastMode.Override(true);
   var vignette=profile.AddSettings<Vignette>();vignette.enabled.Override(true);vignette.intensity.Override(.15f);vignette.smoothness.Override(.7f);
   var grade=profile.AddSettings<ColorGrading>();grade.enabled.Override(true);grade.tonemapper.Override(Tonemapper.Neutral);
   grade.postExposure.Override(.12f);grade.saturation.Override(4);volume.sharedProfile=profile;Snap();
  }
  private Vector3 Offset=>GeometryBatch.Vec(ArtRecipe.CameraPosition(Mode));
  public void Snap()
  {
   bool menu=game.Session.State==GameState.Menu;
   transform.position=menu?new Vector3(6,4.2f,-8.5f):Offset;
   transform.LookAt(menu?new Vector3(-3.2f,1,0):GeometryBatch.Vec(ArtRecipe.CameraLook()));
   cameraComponent.fieldOfView=menu?49:61;
  }
  public void Cycle(){Mode=(Mode+1)%3;PlayerPrefs.SetInt("Moonpop.Camera",Mode);PlayerPrefs.Save();game.UI.Notify(ModeName+" CAMERA",game.Settings.cyan);}
  public void Shake(){if(!game.ReducedMotion)shake=.45f;}
  private void LateUpdate()
  {
   if(game==null||game.Session.State==GameState.Paused)return;
   float dt=Time.deltaTime;bool menu=game.Session.State==GameState.Menu;float x=game.Player.transform.position.x;
   Vector3 target=menu?new Vector3(6+Mathf.Sin(Time.time*.18f)*.25f,4.2f,-8.5f):Offset+Vector3.right*x*.38f;
   Vector3 look=menu?new Vector3(-3.2f,1,0):GeometryBatch.Vec(ArtRecipe.CameraLook())+Vector3.right*x*.5f;
   transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-dt*7));
   shake=Mathf.Max(0,shake-dt);if(shake>0&&!game.ReducedMotion)transform.position+=Random.insideUnitSphere*(shake*.16f);
   transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look-transform.position),1-Mathf.Exp(-dt*10));
   float fov=menu?49:61+game.Session.Difficulty*5+(!game.ReducedMotion&&game.Session.Boosting?8:0);
   cameraComponent.fieldOfView=Mathf.Lerp(cameraComponent.fieldOfView,fov,1-Mathf.Exp(-dt*4));
  }
 }
}
